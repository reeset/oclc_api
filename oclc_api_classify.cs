using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    public class oclc_api_classify
    {
        //lcc = Library of Congress
        //dd = dewey decimal
        //nn = none

        public enum Class_Type { lcc = 0, dd = 1, nn = 2 }
        private string pLastError = "";
        private System.Net.WebProxy pSetProxy = null;

        public string LastError
        {
            get { return pLastError; }
            set { pLastError = value; }
        }

        public System.Net.WebProxy SetProxy
        {
            get { return pSetProxy; }
            set { pSetProxy = value; }
        }

        

        public string GetFastRecord(string url)
        {
            return helpers.MakeHTTPRequest(url, SetProxy);
        }

        public string DoSearch(string url, oclc_api_classify.Class_Type ctype, bool bsummary, out string[] subjects)
        {
            LastError = "";
            
            string sreturn = helpers.MakeOpenHTTPRequest(url, SetProxy, "GET"); // helpers.MakeHTTPRequest(url, SetProxy);
            
            //process results
            return ProcessClassify(sreturn, ctype, bsummary, out subjects);
        }

        private string ProcessClassify(string s, oclc_api_classify.Class_Type ctype, bool bsummary, out string[] subjects)
        {
            LastError = "";
            
            subjects = null;
            System.Xml.XmlDocument objXML = new System.Xml.XmlDocument();
            System.Xml.XmlNamespaceManager oMan = new System.Xml.XmlNamespaceManager(objXML.NameTable);
            oMan.AddNamespace("oclc", "http://classify.oclc.org");
            try
            {
                
                objXML.LoadXml(s);
                


                //First check to see if this is is a record with multiple works
                System.Xml.XmlNodeList test_nodes = objXML.SelectNodes("//oclc:works/oclc:work", oMan);

                //System.Windows.Forms.MessageBox.Show(test_nodes.Count.ToString());
                if (test_nodes != null && test_nodes.Count > 0)
                {
                    
                    string url = "http://classify.oclc.org/classify2/Classify?swid=" + test_nodes[0].Attributes["swid"].InnerText + "&summary=" + bsummary.ToString();
                    string txml = helpers.MakeOpenHTTPRequest(url, SetProxy, "GET"); // helpers.MakeHTTPRequest(url, SetProxy);
                    
                    if (txml != "")
                    {
                        objXML.LoadXml(txml);
                    }
                    else
                    {
                        return "";
                    }
                }

            }
            catch
            {
                LastError = s;
            }

            System.Xml.XmlNodeList objList;

            
            try
            {
                System.Collections.ArrayList tsubject = new System.Collections.ArrayList();
                //look to see if subjects are present
                objList = objXML.SelectNodes("//oclc:fast/oclc:headings/oclc:heading", oMan);
                if (objList != null)
                {
                    foreach (System.Xml.XmlNode objN in objList)
                    {
                        if (objN.Attributes["ident"] != null)
                        {
                            tsubject.Add(objN.Attributes["ident"].InnerText);
                        }
                    }
                    subjects = new string[tsubject.Count];
                    tsubject.CopyTo(subjects);
                }

                if (ctype == Class_Type.dd)
                {
                    objList = objXML.SelectNodes("//oclc:ddc/oclc:mostPopular", oMan);
                    if (objList != null)
                    {
                        foreach (System.Xml.XmlNode objN in objList)
                        {

                            if (objN.Attributes["nsfa"] != null)
                            {
                                return objN.Attributes["nsfa"].InnerText;
                                break;
                            }
                        }
                    }
                }
                else
                {

                    objList = objXML.SelectNodes("//oclc:lcc/oclc:mostPopular", oMan);
                    if (objList != null)
                    {
                        foreach (System.Xml.XmlNode objN in objList)
                        {
                            if (objN.Attributes["nsfa"] != null)
                            {
                                return objN.Attributes["nsfa"].InnerText;
                                break;
                            }
                        }
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("miss");
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                LastError = e.ToString();
                return "";
            }
        }


    }
}
