using ProjectsContainer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace ProjectsContainer.Controllers
{
    public class CheckDllController : Controller
    {
        List<Dll> data = new List<Dll>();

        List<string> path = new List<string>();

        public string Get_type(string path)
        {
            Assembly testAssembly = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(path));
            string result = "";
            object[] attribs = testAssembly.GetCustomAttributes(typeof(DebuggableAttribute), false);
            if (attribs.Length > 0)
            {
                DebuggableAttribute debuggableAttribute = attribs[0] as DebuggableAttribute;
                if (debuggableAttribute != null)
                {
                    result = debuggableAttribute.IsJITOptimizerDisabled ? "Debug" : "Release";
                }
            }
            else
            {
                result = "Release";
            }
            return result;
        }

        void start()
        {
            string base_url = Server.MapPath("~/Content").ToString() + "/tmp_dll/";
            data.Clear();
            foreach (var item in path)
            {
                string[] spl = { };
                try
                {
                    spl = item.Split('\\');
                    System.IO.File.Copy(item, base_url + (spl[spl.Length - 1]));
                    data.Add(new Dll() { FullPath = item, FileName = (spl[spl.Length - 1]), Type = Get_type(base_url + (spl[spl.Length - 1])) });
                }
                catch (Exception)
                {
                    data.Add(new Dll() { FullPath = item, FileName = (spl[spl.Length - 1]), Type = "Don't Read" });
                }

                System.IO.File.Delete(base_url + (spl[spl.Length - 1]));
            }
        }

        public void read_xml()
        {
            path.Clear();
            string xmlFilePath = Server.MapPath("Content/settings") + "\\settings.xml";
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var pathValueList = xmlDoc.Root.Element("dll").Elements("path").ToList();
            foreach (var item in pathValueList)
            {
                string[] files = Directory.GetFiles(item.Value, "*.dll", SearchOption.AllDirectories);
                path.AddRange(files.ToList());
            }
            start();
            /*StreamReader oku = new StreamReader(source);
            string line;
            while ((line = oku.ReadLine()) != null)
            {
                string[] files = Directory.GetFiles(line, "*.dll", SearchOption.AllDirectories);
                path.AddRange(files.ToList());
            }
            oku.Close();
            start();*/
        }

        public ActionResult Index(FormCollection values)
        {
            ViewBag.activeDll = "active";
            List<Dll> filterData = new List<Dll>();            

            try
            {
                if (values["cb_nameFilter"] == "on" || values["cb_debug"] == "on" || values["cb_release"] == "on" || values["cb_dontRead"] == "on")
                {
                    read_xml();
                }
                if (values["cb_nameFilter"] == "on")
                {
                    string xmlFilePath = Server.MapPath("Content/settings") + "\\settings.xml";
                    XDocument xmlDoc = XDocument.Load(xmlFilePath);
                    var filterValueList = xmlDoc.Root.Element("dllFilter").Elements("dllName").ToList();
                    foreach (var item in filterValueList)
                    {
                        List<Dll> nameFilter = new List<Dll>(data.Where(x => x.FileName == item.Value).ToList());
                        for (int i = 0; i < nameFilter.Count; i++)
                        {
                            data.Remove(nameFilter[i]);
                        }
                    }
                    ViewBag.cb_nameFilter = "checked";


                    /*StreamReader oku = new StreamReader(Server.MapPath("Content/tmp_dll") + "\\Filter.txt");
                    string line;
                    while ((line = oku.ReadLine()) != null)
                    {
                        List<Dll> nameFilter=new List<Dll>(data.Where(x => x.FileName == line).ToList());
                        for (int i = 0; i < nameFilter.Count; i++)
                        {
                            data.Remove(nameFilter[i]);
                        }
                    }*/
                    
                }
                if (values["cb_debug"] == "on")
                {
                    filterData.AddRange(data.Where(x => x.Type == "Debug").ToList());
                    ViewBag.cb_debug = "checked";
                }
                if (values["cb_release"] == "on")
                {
                    filterData.AddRange(data.Where(x => x.Type == "Release").ToList());
                    ViewBag.cb_release = "checked";
                }
                if (values["cb_dontRead"] == "on")
                {
                    filterData.AddRange(data.Where(x => x.Type == "Don't Read").ToList());
                    ViewBag.cb_dontRead = "checked";
                }
            }
            catch (Exception)
            {
            }
            return View(filterData);
        }

    }
}
