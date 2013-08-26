using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class LinkAttributeInItemDescriptionJob : Job
    {
        private static Regex _regex = new Regex(@"(?<!\[)(Kraft|Präzision|Vitalität|Zähigkeit|Zustandsschaden|Zustandsdauer|Kritischer Schaden|Heilkraft|Segensdauer|Qual-Widerstand|Magisches Gespür)(?!\])");

        public LinkAttributeInItemDescriptionJob(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;

            p.Load();

            var templates = p.GetAllTemplates().Where(t => t.Title.ToLower() == "infobox gegenstand");

            foreach (var template in templates)
            {
                if (template.Parameters.ContainsKey("beschreibung") && _regex.IsMatch(template.Parameters["beschreibung"]))
                {
                    template.Parameters["beschreibung"] = _regex.Replace(template.Parameters["beschreibung"], "[[$1]]");
                    template.Save();
                    edit.Save = true;
                    edit.EditComment = "Attribute in Gegenstandsbeschreibung verlinkt";
                }
            }
        }
    }
}