using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PublishTFS.Controllers
{
    public class PublishController : Controller
    {
        //
        // GET: /Publish/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection data)
        {
            try
            {
                Directory.CreateDirectory("C:\\TFS");

                string teamProjectCollectionUrl = ViewBag.url = data["txt_url"];
                string serverPath = ViewBag.path = data["txt_path"];
                string localPath = @"C:\\TFS";
                ViewBag.user = data["txt_user"];
                ViewBag.pass = data["txt_pass"];
                var tfsCredentials = new NetworkCredential(data["txt_user"], data["txt_pass"]);
                TfsTeamProjectCollection teamProjectCollection = new TfsTeamProjectCollection(new Uri(teamProjectCollectionUrl), tfsCredentials);

                VersionControlServer versionControlServer = teamProjectCollection.GetService<VersionControlServer>();
                ViewBag.result = "Start";
                Item[] files = versionControlServer.GetItems(serverPath, VersionSpec.Latest, RecursionType.Full, DeletedState.NonDeleted, ItemType.Any, true).Items;
                for (int i = 0; i < files.Length; i++)
                {
                    ViewBag.result = i.ToString();
                    //txt_status.Dispatcher.Invoke(new Action(() => txt_status.Text = "Downloading.. " + (i + 1) + "/" + files.Length));
                    Item item = files[i];
                    string target = System.IO.Path.Combine(localPath, item.ServerItem.Substring(2));

                    if (item.ItemType == ItemType.Folder && !Directory.Exists(target))
                    {
                        Directory.CreateDirectory(target);
                    }
                    else if (item.ItemType == ItemType.File)
                    {
                        item.DownloadFile(target);
                    }
                }

                StreamWriter write = new StreamWriter("C:\\deployment\\publish.bat");
                write.WriteLine(@"cd C:\Windows\Microsoft.NET\Framework\v4.0.30319");
                write.WriteLine(@"MSBuild.exe " + Directory.GetFiles("C:\\TFS", "*.csproj", SearchOption.AllDirectories)[0] + " /m /T:Build /p:Configuration=Release;DeployOnBuild=true;PublishProfile=Test;OutputPath=C:\\deployment");
                write.Close();

                ViewBag.result = "Download Success | Publishing now..";

                Process p = new Process();
                p.StartInfo.FileName = "C:\\deployment\\publish.bat";
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                try
                {
                    p.StandardOutput.ReadToEnd();
                }
                catch (Exception)
                {
                }
                finally 
                {
                    ViewBag.result = "Finished";
                }
            }
            catch (Exception h)
            {
                ViewBag.result = h.Message;
            }
            return View();
        }
	}
}