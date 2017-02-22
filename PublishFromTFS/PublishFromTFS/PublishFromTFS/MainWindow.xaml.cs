using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PublishFromTFS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void txt_url_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_url.Text == "Team Foundation Server URL")
            {
                txt_url.Text = "";
            }
        }

        private void txt_url_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txt_url.Text))
            {
                txt_url.Text = "Team Foundation Server URL";
            }
        }

        private void txt_serverPath_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_serverPath.Text == "Project Path on Server For Example: $/team_name/project_name")
            {
                txt_serverPath.Text = "";
            }
        }

        private void txt_serverPath_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txt_serverPath.Text))
            {
                txt_serverPath.Text = "Project Path on Server For Example: $/team_name/project_name";
            }
        }

        private void txt_userName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_userName.Text == "User Name")
            {
                txt_userName.Text = "";
            }
        }

        private void txt_userName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txt_userName.Text))
            {
                txt_userName.Text = "User Name";
            }
        }

        private void txt_password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_password.Password == "1234")
            {
                txt_password.Password = "";
            }
        }
        private void txt_password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txt_userName.Text))
            {
                txt_userName.Text = "1234";
            }
        }

        string url, path, user, pass;

        void start()
        {
            try
            {
                string teamProjectCollectionUrl = url;
                string serverPath = path;
                string localPath = @"C:\\TFS";
                var tfsCredentials = new NetworkCredential(user, pass);
                TfsTeamProjectCollection teamProjectCollection = new TfsTeamProjectCollection(new Uri(teamProjectCollectionUrl), tfsCredentials);

                VersionControlServer versionControlServer = teamProjectCollection.GetService<VersionControlServer>();

                Item[] files = versionControlServer.GetItems(serverPath, VersionSpec.Latest, RecursionType.Full, DeletedState.NonDeleted, ItemType.Any, true).Items;
                for (int i = 0; i < files.Length; i++)
                {
                    txt_status.Dispatcher.Invoke(new Action(() => txt_status.Text = "Downloading.. " + (i + 1) + "/" + files.Length));
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

                txt_status.Dispatcher.Invoke(new Action(() => txt_status.Text = "Download Success | Publishing now.."));

                Process p = new Process();
                p.StartInfo.FileName = "C:\\deployment\\publish.bat";
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                txt_status.Dispatcher.Invoke(new Action(() => txt_status.Text = "Finished"));
            }
            catch (Exception h)
            {
                MessageBox.Show(h.Message);
            }
        }

        private void btn_process_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(start));
            try
            {
                Directory.CreateDirectory(@"C:\TFS");
                url = txt_url.Text;
                path = txt_serverPath.Text;
                user = txt_userName.Text;
                pass = txt_password.Password;

                t.Start();
            }
            catch (Exception h)
            {
                MessageBox.Show(h.Message);
                t.Suspend();
            }
        }
    }
}
