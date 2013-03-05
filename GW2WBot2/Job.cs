using System;
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
        }


        public void Run(PageList pl)
        {
            Start();
            var statusApi = new StatusApi();
            statusApi.SetStatus(true);
            try
            {
                foreach (Page p in pl.ToEnumerable().SkipWhile(p => !p.title.StartsWith("Arkanist")))
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
