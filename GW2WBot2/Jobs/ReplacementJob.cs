using System.Collections;
using System.Linq;
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
        public GeneralExtensions.Placeholder Placeholders { get; set; }

        public ReplacementJob(Site site) : 
            this(site, new Dictionary<string, string>(), new Dictionary<Regex, string>())
        { }
        
        public ReplacementJob(Site site, IDictionary<string, string> replacements, IDictionary<Regex, string> regexReplacements)
            : this(site, replacements, regexReplacements, GeneralExtensions.Placeholder.Default) 
        { }

        public ReplacementJob(Site site,
                              IDictionary<string, string> replacements,
                              IDictionary<Regex, string> regexReplacements,
                              GeneralExtensions.Placeholder placeholders)
            : base(site)
        {
            Replacements = replacements;
            RegexReplacements = regexReplacements;
            Placeholders = placeholders;
        }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();
            
            var changes = new List<string>();

            p.InsertPlaceholders(GeneralExtensions.Placeholder.Default);

            foreach (var replacement in Replacements.Where(replacement => p.text.Contains(replacement.Key)))
            {
                p.text = p.text.Replace(replacement.Key, replacement.Value);
                changes.Add(replacement.Key + " → " + replacement.Value);
            }
            foreach (var replacement in RegexReplacements)
            {
                var pattern = replacement.Key;
                var replace = replacement.Value;

                pattern.Replace(p.text, match =>
                    {
                        string replaceWith = RegexParseReplaceWithString(match, replace);
                        changes.Add(match.Value + " → " + replaceWith);
                        return replaceWith;
                    });
            }

            p.RemovePlaceholders();

            if (changes.Count > 0)
            {
                edit.Save = true;
                edit.EditComment = "Ersetzt: " + string.Join(", ", changes);
            }
        }

        private string RegexParseReplaceWithString(Match match, string replacement)
        {
            return match.Groups.Cast<Group>()
                        .Aggregate(replacement, (current, group) => current.Replace("$" + group.Index, group.Value));
        }
    }
}