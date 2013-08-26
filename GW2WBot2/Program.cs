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
            pl.FillFromFile("pagelist.txt");
            pl.Sort();

            new MuenzenJob(s).Run(pl);

            //new ReplacementJob(s,
            //    new Dictionary<string, string>()
            //        {
            //            {"{{Ausblick}}", "{{Aussichtspunkt}}"},
            //            {"{{ausblick}}", "{{Aussichtspunkt}}"},
            //        }, new Dictionary<Regex, string>()).Run(pl);
        }
    }
}
