﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace ProjectsContainer.Controllers
{
    public class SettingsController : Controller
    {
        public ActionResult Index(FormCollection values)
        {
            string xmlFilePath = Server.MapPath("Content/settings") + "\\settings.xml";
            XDocument xmlDoc = XDocument.Load(xmlFilePath);

            //Config
            if (values["btn_addConfigPath"] != null)
            {
                XElement kayit = new XElement("path", values["txt_value"]);
                xmlDoc.Root.Element("webConfig").Add(kayit);
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_deleteConfigPath"] != null)
            {
                var list = xmlDoc.Root.Element("webConfig").Elements("path").Where(x => x.Value == values["btn_deleteConfigPath"]).ToList();
                list.ForEach(x => list.Remove());
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_addConfigFilter"] != null)
            {
                XElement kayit = new XElement("keyName", values["txt_value"]);
                xmlDoc.Root.Element("webConfigFilter").Add(kayit);
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_deleteConfigFilter"] != null)
            {
                var list = xmlDoc.Root.Element("webConfigFilter").Elements("keyName").Where(x => x.Value == values["btn_deleteConfigFilter"]).ToList();
                list.ForEach(x => list.Remove());
                xmlDoc.Save(xmlFilePath);
            }

            //Dll
            else if (values["btn_addDllPath"] != null)
            {
                XElement kayit = new XElement("path", values["txt_value"]);
                xmlDoc.Root.Element("dll").Add(kayit);
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_deleteDllPath"] != null)
            {
                var list = xmlDoc.Root.Element("dll").Elements("path").Where(x => x.Value == values["btn_deleteDllPath"]).ToList();
                list.ForEach(x => list.Remove());
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_addDllFilter"] != null)
            {
                XElement kayit = new XElement("dllName", values["txt_value"]);
                xmlDoc.Root.Element("dllFilter").Add(kayit);
                xmlDoc.Save(xmlFilePath);
            }
            else if (values["btn_deleteDllFilter"] != null)
            {
                var list = xmlDoc.Root.Element("dllFilter").Elements("dllName").Where(x => x.Value == values["btn_deleteDllFilter"]).ToList();
                list.ForEach(x => list.Remove());
                xmlDoc.Save(xmlFilePath);
            }

            List<List<XElement>> data = new List<List<XElement>>();
            data.Add(xmlDoc.Root.Element("webConfig").Elements("path").ToList());
            data.Add(xmlDoc.Root.Element("webConfigFilter").Elements("keyName").ToList());
            data.Add(xmlDoc.Root.Element("dll").Elements("path").ToList());
            data.Add(xmlDoc.Root.Element("dllFilter").Elements("dllName").ToList());



            List<string> autoCompletedList = new List<string>();
            XDocument xml = XDocument.Load("C:\\copy\\web.config");
            foreach (var item in xml.Descendants().Select(x => x.Name.LocalName).Distinct())
            {
                autoCompletedList.Add(item);
            }

            foreach (var item in xml.DescendantNodes())
            {
                XDocument xx = XDocument.Parse("<root>" + item.ToString() + "</root>");
                foreach (var item_2 in xx.Root.Elements().Attributes())
                {
                    autoCompletedList.Add(item_2.Value);
                }
            }
            ViewBag.veri = autoCompletedList.ToArray();


            return View(data);
        }

    }
}
