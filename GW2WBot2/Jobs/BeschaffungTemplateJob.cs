using System;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class BeschaffungTemplateJob : Job
    {
        public BeschaffungTemplateJob(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0)
                return;

            p.Load();

            var before = p.text;

            foreach (var template in p.GetAllTemplates())
            {
                if (template.Title == "#dpl:" && template.Parameters.ContainsKey("category") &&
                    template.Parameters.ContainsKey("linksto") && template.Parameters.ContainsKey("format"))
                {
                    var linksTo = template.Parameters["linksto"];
                    linksTo = linksTo == p.title ? "{{PAGENAME}}" : linksTo;

                    if (template.Parameters["category"].ToLower() == "trophäe")
                        p.text = p.text.Replace(template.Text, "Beschaffung|gegenstand=" + linksTo + "|kategorie=Trophäe");
                    else if (template.Parameters["category"].ToLower() == "behälter")
                        p.text = p.text.Replace(template.Text, "Beschaffung|gegenstand=" + linksTo + "|kategorie=Behälter");
                }
            }

            if (p.text.Contains("#dpl:") && p.text.Contains("Behälter"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0} still contains dpl", p.title);
                Console.ResetColor();
            }

            if (p.text != before)
            {
                edit.Save = true;
                edit.EditComment = "DPL durch {{Beschaffung}} ersetzt";
            }
        }
    }
}