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
                if (template.Title.Equals("Infobox Gegenstand", StringComparison.OrdinalIgnoreCase))
                {
                    //stapelbar = ja/###/...
                    if (template.Parameters.HasNotValueIgnoreCase("stapelbar", "nein"))
                    {
                        changes.Add(string.Format("'stapelbar = {0}' entfernt", template.Parameters["stapelbar"]));
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
                    if (template.Parameters.HasValueIgnoreCase("seelengebunden", "ja"))
                    {
                        //wenn gebunden schon existiert löschen
                        if (template.Parameters.ContainsKey("gebunden"))
                        {
                            changes.Add(string.Format("'seelengebunden = {0}' entfernt", template.Parameters["seelengebunden"]));
                            template.Parameters.Remove("seelengebunden");
                        }
                        else
                        {
                            changes.Add(string.Format("'seelengebunden = {0}' zu 'gebunden = seele' geändert", template.Parameters["seelengebunden"]));
                            template.ChangeParametername("seelengebunden", "gebunden");
                            template.Parameters["gebunden"] = "seele";
                        }
                    }
                    
                    //seltenheit = ramsch -> seltenheit = scrhott
                    if (template.Parameters.HasValueIgnoreCase("seltenheit", "ramsch"))
                    {
                        changes.Add(string.Format("'seltenheit = {0}' zu 'seltenheit = Schrott' geändert", template.Parameters["seltenheit"]));
                        template.Parameters["seltenheit"] = "Schrott";
                    }

                    //benutzungen = 1
                    if (template.Parameters.HasValue("benutzungen","1"))
                    {
                        changes.Add("'benutzungen = 1' entfernt");
                        template.Parameters.Remove("benutzungen");
                    }

                }
                #endregion

                #region [J] Angebot
                if (template.Title.Equals("angebot", StringComparison.OrdinalIgnoreCase))
                {
                    //seltenheit = ja entfernen
                    if (template.Parameters.HasValueIgnoreCase("seltenheit", "ja"))
                    {
                        changes.Add("'seltenheit = ja' entfernt");
                        template.Parameters.Remove("seltenheit");
                    }
                    //stufe = ja entfernen
                    if (template.Parameters.HasValueIgnoreCase("stufe", "ja"))
                    {
                        changes.Add("'stufe = ja' entfernt");
                        template.Parameters.Remove("stufe");
                    }
                    //typ = ja entfernen
                    if (template.Parameters.HasValueIgnoreCase("typ", "ja"))
                    {
                        changes.Add("'typ = ja' entfernt");
                        template.Parameters.Remove("typ");
                    }
                    //werte = ja entfernen
                    if (template.Parameters.HasValueIgnoreCase("werte", "ja"))
                    {
                        changes.Add("'werte = ja' entfernt");
                        template.Parameters.Remove("werte");
                    }
                }
                #endregion

                #region [R] Rezept
                if (template.Title.Equals("Rezept", StringComparison.OrdinalIgnoreCase))
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
                    if (template.Parameters.HasValueIgnoreCase("gebunden", "benutzung") &&
                        template.Parameters.HasValueIgnoreCase("seltenheit", "meisterwerk", "selten", "exotisch", "legendär"))
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
