using System;
using System.Collections.Generic;
using System.Linq;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class RemoveUeberfluessigeParameter : Job
    {
        public RemoveUeberfluessigeParameter(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0)
                return;
            p.Load();
            
            var changes = new List<string>();
            
            var templates = p.GetAllTemplates();
            foreach (var template in templates)
            {
                if (template.Title == "Rezept")
                {
                    if (template.Parameters.ContainsKey("seltenheit")
                        &&
                        new[] {"meisterwerk", "selten", "exotisch", "legendär"}.Contains(
                            template.Parameters["seltenheit"].ToLower())
                        && template.Parameters.ContainsKey("gebunden") &&
                        template.Parameters["gebunden"].ToLower() == "benutzung")
                    {
                        template.Parameters.Remove("gebunden");
                        template.Save();
                        changes.Add("Rezept: 'gebunden = benutzung' entfernt");
                    }
                }
                else if (template.Title == "Eventbelohnung")
                {
                    string[] parametersToRemove =
                        {
                            "ep-gold", "ep-silber", "ep-bronze",
                            "ep-gold-niederlage", "ep-silber-niederlage", "ep-bronze-niederlage",
                            "ep-niederlage",
                            "karma-gold", "karma-silber", "karma-bronze",
                            "karma-gold-niederlage", "karma-silber-niederlage", "karma-bronze-niederlage",
                            "karma-niederlage",
                            "münzen-gold", "münzen-silber", "münzen-bronze",
                            "münzen-gold-niederlage", "münzen-silber-niederlage", "münzen-bronze-niederlage",
                            "münzen-niederlage"
                        };
                    var removed = new List<string>();

                    foreach (var parameter in parametersToRemove)
                    {
                        if (template.Parameters.ContainsKey(parameter))
                        {
                            template.Parameters.Remove(parameter);
                            removed.Add(parameter);
                        }
                    }
                    if (removed.Any())
                    {
                        template.Save();
                        changes.Add("Eventbelohnung: '" + string.Join("', '", removed) + "' entfernt");
                    }
                }
                else if (template.Title == "Infobox Aufgabe")
                {
                    string[] parametersToRemove =
                        {
                            "erfahrung", "münzen"
                        };
                    var removed = new List<string>();

                    foreach (var parameter in parametersToRemove)
                    {
                        if (template.Parameters.ContainsKey(parameter))
                        {
                            template.Parameters.Remove(parameter);
                            removed.Add(parameter);
                        }
                    }
                    if (removed.Any())
                    {
                        template.Save();
                        changes.Add("Infobox Aufgabe: '" + string.Join("', '", removed) + "' entfernt");
                    }
                }
                else if (template.Title == "Infobox Farbstoff")
                {
                    string[] parametersToRemove =
                        {
                            "seltenheit"
                        };
                    var removed = new List<string>();

                    foreach (var parameter in parametersToRemove)
                    {
                        if (template.Parameters.ContainsKey(parameter))
                        {
                            template.Parameters.Remove(parameter);
                            removed.Add(parameter);
                        }
                    }
                    if (removed.Any())
                    {
                        template.Save();
                        changes.Add("Infobox Farbstoff: '" + string.Join("', '", removed) + "' entfernt");
                    }
                }
            }

            if (changes.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\tUnbekannt...");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                var comment = "Überflüssige Parameter entfernt: " + string.Join("; ", changes);

                Console.WriteLine("\t" + comment);
                Console.ResetColor();

                edit.EditComment = comment;
                edit.Save = true;
            }
        }
    }

}