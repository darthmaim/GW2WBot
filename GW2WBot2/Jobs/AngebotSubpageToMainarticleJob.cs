using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class AngebotSubpageToMainarticleJob : Job
    {
        public AngebotSubpageToMainarticleJob(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0)
                return;
            p.Load();
            
            //Nur Seiten mit Vorlage:Infobox NSC
            if (p.GetAllTemplates().All(t => t.Title.ToLower() != "infobox nsc")) return;

            //Nur Seiten, die eine Unterseite mit Angeboten haben...
            var m = Regex.Match(p.text, "\\{\\{:" + p.title + "/([^}]+)}}");
            if (!m.Success) return;

            var subpageTitle = m.Groups[1].Value;

            var subpage = new Page(p.site, p.title + "/" + subpageTitle);
            subpage.Load();
            if (!subpage.Exists())
            {
                p.text = p.text.Replace(m.Value, "");
                edit.EditComment = "Verweis auf nicht vorhandene Angebots-Unterseite „" + subpage.title + "“ entfernt";
                edit.Save = true;
            }
            else
            {
                var pl2 = new PageList(p.site);
                pl2.FillFromLinksToPage(subpage.title);
                if (pl2.Count() > 1) return;

                var subpageContent = Regex.Replace(subpage.text, "<noinclude>.*?</noinclude>", "").Trim();

                p.text = p.text.Replace(m.Value, subpageContent);
                
                subpage.text = "{{Löschantrag|[Bot] In den Hauptartikel „[[" + p.title + "]]“ verschoben}}\n" +
                               subpage.text;
                subpage.Save("[Bot] In Hauptartikel „[[" + p.title + "]]“ verschoben", true);

                edit.EditComment = "Angebot von „" + subpage.title + "“ in den Hauptartikel verschoben";
                edit.Save = true;
            }
        }
    }
}