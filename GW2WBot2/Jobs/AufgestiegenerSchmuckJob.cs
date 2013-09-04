using System;
using System.Linq;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2.Jobs
{
    public class AufgestiegenerSchmuckJob : Job
    {
        public AufgestiegenerSchmuckJob(Site site) : base(site)
        { }

        protected override void ProcessPage(Page p, Job.EditStatus edit)
        {
            if(p.GetNamespace() != 0) return;

            p.Load();

            foreach (var template in p.GetAllTemplates().Where(
                template => (template.Title.Equals("Ausrüstungswerte", StringComparison.OrdinalIgnoreCase)
                             || template.Title.Equals("Rezept", StringComparison.OrdinalIgnoreCase)) &&
                            (template.Parameters.HasValueIgnoreCase("seltenheit", "Aufgestiegen") &&
                             !template.Parameters.ContainsKey("aufwertung"))))
            {
                template.InsertParameterAfter("aufwertung", "nein", "infusion2", "infusion");

                template.Save();

                edit.EditComment = "'aufwertung = nein' hinzugefügt";
                edit.Save = true;
            }
        }

        protected override void Start()
        {
            if(Pages.Any()) return;
    
            var pl = new PageList(Site);
            
            pl.FillAllFromCategoryTree("Kategorie:Schmuck");
            Pages.AddRange(pl.ToEnumerable());

            pl.FillAllFromCategoryTree("Kategorie:Rücken");
            Pages.AddRange(pl.ToEnumerable());
        }
    }
}