using System.Text.RegularExpressions;
using DotNetWikiBotExtensions;
using System.Collections.Generic;
using DotNetWikiBot;

namespace GW2WBot2.Jobs
{
    public class ReplacementJob : Job
    {
        public IDictionary<string, string> Replacements { get; set; }
        public IDictionary<Regex, string> RegexReplacements { get; set; }

        public ReplacementJob(Site site, IDictionary<string, string> replacements, IDictionary<Regex, string> regexReplacements)
            : base(site)
        {
            Replacements = replacements;
            RegexReplacements = regexReplacements;
        }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();
            
            var changes = new List<string>();

            p.InsertPlaceholders(GeneralExtensions.Placeholder.Default);

            foreach (var replacement in Replacements)
            {
                if (p.text.Contains(replacement.Key))
                {
                    p.text = p.text.Replace(replacement.Key, replacement.Value);
                    changes.Add(replacement.Key + " → " + replacement.Value);
                }
            }
            foreach (var replacement in RegexReplacements)
            {
                //this is all not fast, but it works
                //does not work :P

                //var match = replacement.Key.Match(p.text);
                //if (match.Success)
                //{
                //    p.text = replacement.Key.Replace(p.text, replacement.Value);
                //    changes.Add(match.Value + " → " + replacement.Key.Replace(match.Value, replacement.Value));
                //}
            }

            p.RemovePlaceholders();

            if (changes.Count > 0)
            {
                edit.Save = true;
                edit.EditComment = "Ersetzt: " + string.Join(", ", changes);
            }
        }
    }
}
