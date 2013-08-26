using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetWikiBot;
using DotNetWikiBotExtensions;

namespace GW2WBot2
{
    public abstract class Job
    {
        public Site Site { get; set; }

        protected Job(Site site)
        {
            Site = site;
            Pages = new List<Page>();
        }

        public List<Page> Pages { get; set; }

        public void Run(PageList pageList)
        {
            Pages = new List<Page>(pageList.ToEnumerable());
            Run();
        }

        public void Run(IEnumerable<Page> pages)
        {
            Pages = new List<Page>(pages);
            Run();
        }

        public void Run()
        {
            Start();
            var statusApi = new StatusApi();
            statusApi.SetStatus(true);
            try
            {
                var i = 0;
                lock(Pages) foreach (Page p in Pages.OrderBy(p => p.title).Skip(840))
                {
                    if (!statusApi.GetRunning())
                    {
                        Console.WriteLine("Canceled by status api");
                        return;
                    }

                    var editStatus = new EditStatus();
                    ProcessPage(p, editStatus);

                    if (editStatus.Save)
                    {
                        p.Save("[Bot] " + editStatus.EditComment, true);
                        p.LoadEx();
                        statusApi.AddEdit(p.title, p.lastRevisionID, editStatus.EditComment);
                        Thread.Sleep(10000);
                    }

                    Console.Title = string.Format("({0}/{1})", ++i, Pages.Count());
                }
            }
            finally
            {
                statusApi.SetStatus(false);
                End();
            }
        }

        protected abstract void ProcessPage(Page p, EditStatus edit);

        protected virtual void Start() { }
        protected virtual void End() { }

        protected class EditStatus
        {
            public bool Save { get; set; }
            public string EditComment { get; set; }
        }
    }
}
