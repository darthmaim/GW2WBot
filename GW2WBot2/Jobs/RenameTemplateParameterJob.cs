using System.Collections.Generic;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class RenameTemplateParameterJob : Job
    {
        public Dictionary<string, Dictionary<string, string>> Replacements { get; private set; }

        public RenameTemplateParameterJob(Site site, Dictionary<string, Dictionary<string, string>> replacements) : base(site)
        {
            Replacements = replacements;
        }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();

            var allChanges = new List<string>();
            var before = p.text;

            foreach (var template in p.GetAllTemplates())
            {
                var templateChanges = new List<string>();
                if (Replacements.ContainsKey(template.Title))
                {
                    foreach (var parameter in template.Parameters)
                    {
                        if (Replacements[template.Title].ContainsKey(parameter.Key))
                        {
                            template.ChangeParametername(parameter.Key, Replacements[template.Title][parameter.Key]);
                            templateChanges.Add(parameter.Key + " → " + Replacements[template.Title][parameter.Key]);
                        }
                    }
                }

                if (templateChanges.Count > 0)
                {
                    template.Save();
                    allChanges.Add(template.Title + ": " + string.Join(", ", templateChanges));
                }
            }

            if (allChanges.Count > 0)
            {
                edit.Save = true;
                edit.EditComment = "Parameter umbenannt: " + string.Join("; ", allChanges);
            }
        }

        protected override void Start()
        {
            var pl = new PageList(Site);
            Pages.Clear();

            foreach (var key in Replacements.Keys)
            {
                pl.FillFromLinksToPage("Vorlage:" + key);
                Pages.AddRange(pl.ToEnumerable());
            }
        }
    }
}