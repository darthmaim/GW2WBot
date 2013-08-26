using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetWikiBot;

namespace GW2WBot2.Jobs
{
    public class MuenzenJob :Job
    {
        private static readonly Regex MuenzenRegex = new Regex(@"((((?<gold>[0-9]+)\s*\{\{Gold}})|((?<silber>[0-9]+)\s*\{\{Silber}})|((?<kupfer>[0-9]+)\s*\{\{Kupfer}}))\s*){1,3}", RegexOptions.IgnoreCase);

        public MuenzenJob(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();

            var before = p.text;

            var changes = new List<string>();

            Match m;
            while ((m = MuenzenRegex.Match(p.text)).Success)
            {
                var kupfer = 0;
                var silber = 0;
                var gold = 0;

                int.TryParse(m.Groups["kupfer"].Value, out kupfer);
                int.TryParse(m.Groups["silber"].Value, out silber);
                int.TryParse(m.Groups["gold"].Value, out gold);

                var muenzen = kupfer + 100*silber + 10000*gold;

                p.text = p.text.Replace(m.Value.Trim(), "{{Münzen|" + muenzen + "}}");

                changes.Add(string.Format("{0}g {1}s {2}k → {3}", gold, silber, kupfer, muenzen));
            }

            if (changes.Count > 0)
            {
                edit.EditComment = string.Format("Münzen ({0}x): {1}", changes.Count, string.Join(", ", changes));
                edit.Save = true;
            }
            
        }
    }
}
