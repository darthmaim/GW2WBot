using System.IO;
using System.Net;

namespace GW2WBot2
{
    public class StatusApi
    {
        private const string URL = "http://gw2wbot.darthmaim.de/api.php";
        private readonly string _authentication;
        private readonly WebClient _wc;

        public StatusApi()
        {
            _authentication = Program.ApiAuthentication;

            _wc = new WebClient();
        }

        public bool GetRunning()
        {
            var json = _wc.DownloadString(URL + "?action=getStatus");
            var status = new { running = false };
            status = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, status);
            return status.running;
        }

        public void SetStatus(bool running)
        {
            _wc.DownloadString(URL + string.Format("?action=setStatus&status={0}&", running ? "1" : "0") + _authentication);
        }

        public void AddStatus(string title, string description)
        {
            _wc.DownloadString(URL + string.Format("?action=addStatus&title={0}&description={1}&", title, description) + _authentication);
        }

        public void AddEdit(string title, string oldid, string description)
        {
            title = System.Web.HttpUtility.UrlEncode(title);
            description = System.Web.HttpUtility.UrlEncode(description);
            var resp = _wc.DownloadString(URL + string.Format("?action=addEdit&title={0}&description={1}&oldid={2}&", title, description, oldid) + _authentication);
        }

    }
}
