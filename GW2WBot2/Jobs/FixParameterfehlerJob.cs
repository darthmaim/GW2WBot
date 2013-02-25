using System;
using System.Collections.Generic;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    class FixParameterfehlerJob : Job
    {
        protected override void ProcessPage(Page p, EditStatus edit)
        {
            var changes = new List<string>();
            
            var templates = p.GetAllTemplates();
            foreach (var template in templates)
            {
                var changeCountBeforeTemplate = changes.Count;

                #region Infobox Gegenstand
                if (template.Title.ToLower() == "Infobox Gegenstand".ToLower())
                {
                    //stapelbar = ja/###/...
                    if (template.Parameters.ContainsKey("stapelbar") && template.Parameters["stapelbar"].ToLower() != "nein")
                    {
                        changes.Add("'stapelbar = " + template.Parameters["stapelbar"] + "' entfernt");
                        template.Parameters.Remove("stapelbar");
                    }

                    //wert -> händlerwert
                    if (template.Parameters.ContainsKey("wert"))
                    {
                        //wenn händlerwert schon existiert löschen
                        if (template.Parameters.ContainsKey("händlerwert"))
                        {
                            template.Parameters.Remove("wert");
                            changes.Add("'wert' entfernt");
                        }
                        else
                        {
                            template.ChangeParametername("wert", "händlerwert");
                            changes.Add("'wert' zu 'händlerwert' geändert");
                        }
                    }

                    //seelengebunden = ja -> gebunden = seele
                    if (template.Parameters.ContainsKey("seelengebunden") && template.Parameters["seelengebunden"].ToLower() == "ja")
                    {
                        //wenn gebunden schon existiert löschen
                        if (template.Parameters.ContainsKey("gebunden"))
                        {
                            changes.Add("'seelengebunden = " + template.Parameters["seelengebunden"] + "' entfernt");
                            template.Parameters.Remove("seelengebunden");
                        }
                        else
                        {
                            changes.Add("'seelengebunden = " + template.Parameters["seelengebunden"] + "' zu 'gebunden = seele' geändert");
                            template.ChangeParametername("seelengebunden", "gebunden");
                            template.Parameters["gebunden"] = "seele";
                        }
                    }
                    if (template.Parameters.ContainsKey("seltenheit") && template.Parameters["seltenheit"].ToLower() == "ramsch")
                    {
                        changes.Add("'seltenheit = " + template.Parameters["seltenheit"] + "' zu 'seltenheit = Schrott' geändert");
                        template.Parameters["seltenheit"] = "Schrott";
                    }
                }
                #endregion

                #region Rezept
                if (template.Title.ToLower() == "Rezept".ToLower())
                {
                    var changedSomething = false;
                    for (var i = 1; i <= 4; i++)
                    {
                        var key = "attribut" + i + "-wert";
                        if (template.Parameters.ContainsKey(key) && template.Parameters[key].StartsWith("+"))
                        {
                            template.Parameters[key] = template.Parameters[key].TrimStart('+');
                            changedSomething = true;
                        }
                    }
                    if (changedSomething)
                        changes.Add("Überflüssiges + aus dem [[Vorlage:Rezept|Rezept]] " + (template.Parameters.ContainsKey("name") ? template.Parameters["name"] : p.title) + " entfernt");
                }
                #endregion

                if (changeCountBeforeTemplate != changes.Count)
                    template.Save();
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
                var comment = "Parameterfehler behoben: " + string.Join("; ", changes);

                Console.WriteLine("\t" + comment);
                Console.ResetColor();

                edit.EditComment = comment;
                edit.Save = true;
            }
        }
    }
}
