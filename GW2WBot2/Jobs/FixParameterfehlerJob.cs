using System;
using System.Collections.Generic;
using System.Linq;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    class FixParameterfehlerJob : Job
    {
        public FixParameterfehlerJob(Site site) : base(site) { }

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            var changes = new List<string>();

            if (p.GetNamespace() != 0) return;
            p.Load();
            
            var templates = p.GetAllTemplates();
            foreach (var template in templates)
            {
                var changeCountBeforeTemplate = changes.Count;

                #region [G] Infobox Gegenstand
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
                    
                    //seltenheit = ramsch -> seltenheit = scrhott
                    if (template.Parameters.ContainsKey("seltenheit") && template.Parameters["seltenheit"].ToLower() == "ramsch")
                    {
                        changes.Add("'seltenheit = " + template.Parameters["seltenheit"] + "' zu 'seltenheit = Schrott' geändert");
                        template.Parameters["seltenheit"] = "Schrott";
                    }

                    //benutzungen = 1
                    if (template.Parameters.ContainsKey("benutzungen") && template.Parameters["benutzungen"] == "1")
                    {
                        changes.Add("'benutzungen = 1' entfernt");
                        template.Parameters.Remove("benutzungen");
                    }

                }
                #endregion

                #region [J] Angebot
                if (template.Title.ToLower() == "angebot")
                {
                    //seltenheit = ja entfernen
                    if (template.Parameters.ContainsKey("seltenheit") &&
                        template.Parameters["seltenheit"].ToLower() == "ja")
                    {
                        changes.Add("'seltenheit = ja' entfernt");
                        template.Parameters.Remove("seltenheit");
                    }
                    //stufe = ja entfernen
                    if (template.Parameters.ContainsKey("stufe") &&
                        template.Parameters["stufe"].ToLower() == "ja")
                    {
                        changes.Add("'stufe = ja' entfernt");
                        template.Parameters.Remove("stufe");
                    }
                    //typ = ja entfernen
                    if (template.Parameters.ContainsKey("typ") &&
                        template.Parameters["typ"].ToLower() == "ja")
                    {
                        changes.Add("'typ = ja' entfernt");
                        template.Parameters.Remove("typ");
                    }
                    //werte = ja entfernen
                    if (template.Parameters.ContainsKey("werte") &&
                        template.Parameters["werte"].ToLower() == "ja")
                    {
                        changes.Add("'werte = ja' entfernt");
                        template.Parameters.Remove("werte");
                    }
                }
                #endregion

                #region [R] Rezept
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


                    //anzahl = 1
                    if (template.Parameters.ContainsKey("anzahl") && template.Parameters["anzahl"] == "1")
                    {
                        changes.Add("'anzahl = 1' entfernt");
                        template.Parameters.Remove("anzahl");
                    }

                    //gebunden = benutzung bei seltenheit meisterwerk/selten/exotisch/legendär entfernen
                    if (template.Parameters.ContainsKey("gebunden") &&
                        template.Parameters["gebunden"].ToLower() == "benutzung" &&
                        template.Parameters.ContainsKey("seltenheit") &&
                        (template.Parameters["seltenheit"].ToLower() == "meisterwerk" ||
                         template.Parameters["seltenheit"].ToLower() == "selten" ||
                         template.Parameters["seltenheit"].ToLower() == "exotisch" ||
                         template.Parameters["seltenheit"].ToLower() == "legendär"))
                    {
                        changes.Add(string.Format("'gebunden = {0}' entfernt, da 'seltenheit = {1}'", template.Parameters["gebunden"], template.Parameters["seltenheit"]));
                        template.Parameters.Remove("gebunden");
                    }
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

        protected override void Start()
        {
            if (Pages.Count > 0) return;
            
            var pl = new PageList(Site);
            pl.FillFromCategoryTree("Parameterfehler");

            Pages = pl.ToEnumerable().ToList();
        }
    }
}
