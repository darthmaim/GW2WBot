using System;
using System.Threading;
using DotNetWikiBot;

namespace GW2WBot2
{
    public abstract class Job
    {
        public void Run(Site s, PageList pl)
        {
            var statusApi = new StatusApi();
            statusApi.SetStatus(true);
            try
            {
                foreach (Page p in pl)
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
            }
        }

        protected abstract void ProcessPage(Page p, EditStatus edit);

        protected class EditStatus
        {
            public bool Save { get; set; }
            public string EditComment { get; set; }
        }
    }
}
