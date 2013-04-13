using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using DotNetWikiBot;
using DotNetWikiBotExtensions;
using GW2WBot2.Jobs;

namespace GW2WBot2
{
    public class Program
    {
        public static string User;
        public static string Pass;
        public static string ApiAuthentication;

        static void Main(string[] args)
        {
            foreach (var line in File.ReadLines("config.txt"))
            {
                var indexOfSep = line.IndexOf('=');
                var key = line.Substring(0,indexOfSep).Trim();
                var val = line.Substring(indexOfSep + 1).Trim();

                if (key == "user") User = val;
                if (key == "pass") Pass = val;
                if (key == "apiAuthentication") ApiAuthentication = val;
            }

            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pass) || string.IsNullOrEmpty(ApiAuthentication))
            {
                Console.WriteLine("Incomplete config");
                return;
            }

            var statusApi = new StatusApi();
            statusApi.SetStatus(false);
            
            
            var s = new Site("http://wiki-de.guildwars2.com/wiki/", User, Pass);

            if (args.Contains("-generatePagelist") || !File.Exists("pagelist.txt"))
            {
                Console.WriteLine("Generating pagelist...");
                var pagelist = new PageList(s);
                pagelist.FillFromCategoryTree("Guild Wars 2");
                pagelist.SaveTitlesToFile("pagelist.txt");
            }

            var pl =  new PageList(s);
            //pl.FillFromFile("pagelist.txt");
            pl.FillFromCategory("Kategorie:Fehlende Informationen");
            pl.Sort();
            
            //new BeschaffungTemplateJob().Run(pl);

            //new ReplacementJob(s
            //                   new Dictionary<string, string>()
            //                       {
            //                           {"Juwelenschleifermeistern", "Meister-Juwelieren"},
            //                           {"Kochmeistern", "Meister-Köchen"},
            //                           {"Konstrukteurmeistern", "Meister-Konstrukteuren"},
            //                           {"Lederermeistern", "Meister-Lederern"},
            //                           {"Rüstungsschmiedemeistern", "Meister-Rüstungsschmieden"},
            //                           {"Schneidermeistern", "Meister-Schneidern"},
            //                           {"Waffenschmiedmeistern", "Meister-Waffenschmieden"},
            //                           {"Waidmannmeistern", "Meister-Waidmännern"},

            //                           {"[[Juwelenschleifermeister]]n", "[[Meister-Juwelier]]en"},
            //                           {"[[Kochmeister]]n", "[[Meister-Koch|Meister-Köchen]]"},
            //                           {"[[Konstrukteurmeister]]n", "[[Meister-Konstrukteur]]en"},
            //                           {"[[Lederermeister]]n", "[[Meister-Lederer]]n"},
            //                           {"[[Rüstungsschmiedemeister]]n", "[[Meister-Rüstungsschmied]]en"},
            //                           {"[[Schneidermeister]]n", "[[Meister-Schneider]]n"},
            //                           {"[[Waffenschmiedmeister]]n", "[[Meister-Waffenschmied]]en"},
            //                           {"[[Waidmannmeister]]n", "[[Meister-Waidmann|Meister-Waidmännern]]"},

            //                           {"Juwelenschleifermeister", "Meister-Juwelier"},
            //                           {"Kochmeister", "Meister-Koch"},
            //                           {"Konstrukteurmeister", "Meister-Konstrukteur"},
            //                           {"Lederermeister", "Meister-Lederer"},
            //                           {"Rüstungsschmiedemeister", "Meister-Rüstungsschmied"},
            //                           {"Schneidermeister", "Meister-Schneider"},
            //                           {"Waffenschmiedmeister", "Meister-Waffenschmied"},
            //                           {"Waidmannmeister", "Meister-Waidmann"}
            //                       }, new Dictionary<Regex, string>()).Run(pl);
            
            //new ReplacementJob(s,
            //    new Dictionary<string, string>()
            //        {
            //            {"Schlichter Wiederverwertungskit", "Schlichtes Wiederverwertungskit"},
            //            {"Gespannter Rohlederflicken", "Gespannter Rohleder-Flicken"}
            //        }, new Dictionary<Regex, string>()).Run(pl);

            //new StandortTemplateJob(s).Run(pl);
            //new StandortTemplateJob(s).Run(pl.ToEnumerable().SkipWhile(p => !p.title.StartsWith("Blopp")));

            //new RenameTemplateParameterJob(s, new Dictionary<string, Dictionary<string, string>> {
            //        {"Infobox Event", new Dictionary<string, string> {
            //            {"sektor",  "gegend"},
            //            {"sektor1", "gegend1"},
            //            {"sektor2", "gegend2"},
            //            {"sektor3", "gegend3"},
            //            {"sektor4", "gegend4"},
            //            {"sektor5", "gegend5"},
            //            {"sektor6", "gegend6"},
            //            {"sektor7", "gegend7"},
            //            {"sektor8", "gegend8"},
            //            {"sektor9", "gegend9"},
            //        } },
            //        {"Infobox Aufgabe", new Dictionary<string, string> {
            //            {"sektor",  "gegend"},
            //        } }
            //    }).Run();

            new FehlendeInformationenJob(s).Run(pl);
        }
    }
}
