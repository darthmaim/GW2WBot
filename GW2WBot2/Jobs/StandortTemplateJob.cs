using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class StandortTemplateJob : Job
    {
        private readonly List<string> _schauplaetze = new List<string> { "Ascalon", "Kryta", "Maguuma-Dschungel", "Meer des Leids", "Ruinen von Orr", "Zittergipfel-Gebirge" };

        public StandortTemplateJob(Site site) : base(site)
        { }

        protected override void Start()
        {
            var pl = new PageList(Site);
            pl.FillFromCategoryTree("Gegend");
            _schauplaetze.AddRange(pl.ToEnumerable().Select(p => p.title));

            pl.FillFromCategoryTree("Gebiet");
            _schauplaetze.AddRange(pl.ToEnumerable().Select(p => p.title));

            pl.FillFromCategoryTree("Stadt");
            _schauplaetze.AddRange(pl.ToEnumerable().Select(p => p.title));
        }

        private static readonly Regex StandortItemRegex = new Regex(@"^(\*+)\s?\[\[([^|]+?)(\|.*?)?\]\]\s?(.*)$");

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();

            //ignore pages which already have {{standort}}
            if (p.GetAllTemplates().Any(t => t.Title == "Standort")) return;

            var section = p.GetSectionByName("Standort", false);
            if (section == null) return;

            var standorte = new List<string>();

            Match lastMatch = null;
            var prependText = "";
            var remainingText = "";
            var foundNonSchauplatz = false;
            var compactTemplate = true;

            var lines = section.Content.GetLines().Skip(1).ToList();
            var addAdditionalStar = !lines.First().Contains('*') && lines.First().Length > 2 && PageIsSchauplatz(lines.First().Trim('[', ']', '\'', ';'));

            foreach (var line in lines)
            {
                //just add the line to the remaining text as soon as we found a non schauplatz
                if (foundNonSchauplatz) remainingText += "\r\n" + line;

                
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var match = StandortItemRegex.Match((addAdditionalStar ? "*" : "") + line);

                //if the line is no listitem and no schauplatz, we are done with the standort list
                //all remaining text will be added to remainingText
                if (!match.Success || !PageIsSchauplatz(match.Groups[2].Value))
                {
                    if (standorte.Count == 0 && lastMatch == null)
                    {
                        prependText += line + "\r\n";
                    }
                    else
                    {
                        foundNonSchauplatz = true;
                        remainingText = line;
                    }
                    continue;
                }

                var added = false;

                //has a descripting text
                if (match.Groups[3].Length > 0)
                {
                    standorte.Add(match.Groups[2].Value + " = " + match.Groups[4].Value);
                    compactTemplate = false;
                    added = true;
                }
                if (lastMatch != null && match.Groups[1].Length <= lastMatch.Groups[1].Length)
                {
                    //add last
                    standorte.Add(lastMatch.Groups[2].Value + (lastMatch.Groups[4].Length > 0 ? " = " + lastMatch.Groups[4].Value : ""));
                    lastMatch = null;
                }

                lastMatch = !added ? match : null;
            }

            if (lastMatch != null)
            {
                standorte.Add(lastMatch.Groups[2].Value + (lastMatch.Groups[4].Length > 0 ? " = " + lastMatch.Groups[4].Value : ""));
            }

            if (standorte.Count == 0) return;

            if (!foundNonSchauplatz) remainingText += "\r\n";
            

            if (standorte.Count > 3) compactTemplate = false;

            //build the new section content
            section.Content = section.Content.GetLines().First() + "\r\n" + prependText + 
                              (compactTemplate
                                   ? "{{Standort|" + string.Join("|", standorte) + "}}"
                                   : "{{Standort\r\n | " + string.Join("\r\n | ", standorte) + "\r\n}}")
                              + "\r\n" + remainingText;

            section.Save();

            edit.Save = true;
            edit.EditComment = "/* "+section.Title+" */ [[Benutzer: Darthmaim Bot/Projekte#Standort|Standortvorlage eingebaut]] ("+standorte.Count+")";
        }

        private bool PageIsSchauplatz(string page)
        {
            if (_schauplaetze.Contains(page)) return true;

            //return true if the page doesn't exist
            var p = new Page(Site, page);
            p.Load();
            if(!p.Exists()) _schauplaetze.Add(page);
            return !p.Exists();
        }
    }
}