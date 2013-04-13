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

        private Regex _regex = new Regex(@"\b(engl|franz|span|fr|es|en)?\.?\-?\s?(([Ii]nter)?[Ww]ikis?|link)\s?(\(?(es|en|fr)\)?)?(,|;|und|\.|\+)?\s?\b");

        protected override void ProcessPage(Page p, EditStatus edit)
        {
            if (p.GetNamespace() != 0) return;
            p.Load();

            var before = p.text;

            var templates = p.GetAllTemplates().Where(t => t.Title.ToLower() == "fehlende informationen");

            foreach (var template in templates)
            {
                var fehlendeInformation = template.Parameters["0"].Trim();

                if(fehlendeInformation.ToLower() == "interwiki" || fehlendeInformation.ToLower() == "fr" || fehlendeInformation.ToLower() == "en" || fehlendeInformation.ToLower() == "es")
                {
                    RemoveTemplate(p, template);

                    edit.Save = true;
                    edit.EditComment = "[[Vorlage:Fehlende Informationen]] entfernt (nur interwiki)";
                }
                else if (fehlendeInformation == "")
                {
                    RemoveTemplate(p, template);

                    edit.Save = true;
                    edit.EditComment = "[[Vorlage:Fehlende Informationen]] entfernt (leer)";
                }
                else
                {
                    var modifiedFehlendeInformationen = fehlendeInformation;

                    while(true)
                    {
                        var match = _regex.Match(modifiedFehlendeInformationen);

                        if (!match.Success) break;

                        modifiedFehlendeInformationen = _regex.Replace(modifiedFehlendeInformationen, "", 1);
                    }

                    if (modifiedFehlendeInformationen == fehlendeInformation) continue;

                    var removed = fehlendeInformation.FindRemovedPart(modifiedFehlendeInformationen);

                    modifiedFehlendeInformationen = modifiedFehlendeInformationen.RemoveTrailingPunctuation();

                    if (string.IsNullOrWhiteSpace(modifiedFehlendeInformationen))
                    {
                        RemoveTemplate(p, template);

                        edit.Save = true;
                        edit.EditComment = string.Format("[[Vorlage:Fehlende Informationen]] entfernt (Inhalt war: „{0}“)", fehlendeInformation);
                    }
                    else
                    {
                        p.text = p.text.Replace(fehlendeInformation, modifiedFehlendeInformationen);

                        edit.Save = true;
                        edit.EditComment = string.Format("Aus fehlenden Informationen entfernt: „{0}“", removed);
                    }
                }
            }
        }

        private void RemoveTemplate(Page p, Template template)
        {
            var before = p.text;

            template.Remove();

            if (p.text == before)
            {
                throw new Exception("text didn't change");
            }
        }
    }
}