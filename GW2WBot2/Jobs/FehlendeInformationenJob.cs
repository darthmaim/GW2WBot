using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class FehlendeInformationenJob : Job
    {
        public FehlendeInformationenJob(Site site) : base(site) { }


        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();

            var templates = p.GetAllTemplates().Where(t => t.Title.ToLower() == "fehlende informationen");

            foreach (var template in templates)
            {
                var fehlendeInformation = template.Parameters["0"].Trim();

                if(fehlendeInformation.ToLower() == "interwiki" || fehlendeInformation.ToLower() == "fr" || fehlendeInformation.ToLower() == "en" || fehlendeInformation.ToLower() == "es")
                {
                    var before = p.text;

                    p.text = Regex.Replace(p.text, string.Format(@"{0}([^\S\n]*\n)?", Regex.Escape("{{" + template.Text + "}}")), "");

                    if (p.text == before)
                    {
                        throw new Exception("text didn't change");
                    }

                    edit.Save = true;
                    edit.EditComment = "[[Vorlage:Fehlende Informationen]] entfernt (nur interwiki)";
                }
                else if (fehlendeInformation == "")
                {
                    var before = p.text;

                    p.text = Regex.Replace(p.text, string.Format(@"{0}([^\S\n]+\n)?", Regex.Escape("{{" + template.Text + "}}")), "");

                    if(p.text == before)
                    {
                        throw new Exception("text didn't change");
                    }

                    edit.Save = true;
                    edit.EditComment = "[[Vorlage:Fehlende Informationen]] entfernt (leer)";
                }
            }

        }
    }
}