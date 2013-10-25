using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    public class oclc_api_search
    {


        private int pRecordCount = 0;
        private System.Net.WebProxy pSetProxy = null;
        private string pwskey = "";
        private string pURL = "http://www.worldcat.org/webservices/catalog/search/sru?query={query}&wskey={wskey}";

        public enum Query_Type {
            keyword=0,
            title=1,
            author=2,
            subject=3
        }

        public struct struct_Records
        {
            public string main;
            public string xml;
            public string display;
        }

        public string OCLC_WORLDCAT_URL
        {
            set { pURL = value; }
            get { return pURL; }
        }


        public string wskey
        {
            set { pwskey = value; }
            get { return pwskey; }
        }

        public int RecordCount
        {
            get { return pRecordCount; }
            set { pRecordCount = value; }
        }

        public System.Net.WebProxy SetProxy
        {
            get { return pSetProxy; }
            set { pSetProxy = value; }
        }

        public oclc_api_search(string OCLC_Wskey)
        {
            wskey = OCLC_Wskey;
        }

        public oclc_api_search() { }

        public string SearchWorldCat(string query, oclc_api_search.Query_Type qtype) 
        {
            string sindex = "srw.keyword";
            //http://www.worldcat.org/webservices/catalog/search/sru?query=srw.su%3D%22globalization%22&sortKeys=Date,,0&wskey=[key]
            switch (qtype)
            {
                case Query_Type.keyword:
                    sindex = System.Uri.EscapeDataString("srw.kw=\"" + query + "\"");
                    break;
                case Query_Type.author:
                    sindex = System.Uri.EscapeDataString("srw.au=\"" + query + "\"");
                    break;
                case Query_Type.subject:
                    sindex = System.Uri.EscapeDataString("srw.su=\"" + query + "\"");
                    break;
                case Query_Type.title:
                    sindex = System.Uri.EscapeDataString("srw.ti=\"" + query + "\"");
                    break;
                default:
                    sindex = System.Uri.EscapeDataString("srw.kw=\"" + query + "\"");
                    break;
            }

            string url = OCLC_WORLDCAT_URL.Replace("{query}", sindex).Replace("{wskey}", wskey);
            string xml = helpers.MakeHTTPRequest(url, SetProxy);
            if (xml == null || xml == "")
            {
                return null;
            }
            else
            {
                return xml;
            }
        }
        
        public string SearchWorldCatMulti(string[] query, oclc_api_search.Query_Type[] qtype, string sconditional)
        {
            string sindex = "";
            //http://www.worldcat.org/webservices/catalog/search/sru?query=srw.su%3D%22globalization%22&sortKeys=Date,,0&wskey=[key]

            for (int x = 0; x < query.Length; x++)
            {
                switch (qtype[x])
                {
                    case Query_Type.keyword:
                        sindex += "srw.kw=\"" + query[x] + "\" " + sconditional + " ";
                        break;
                    case Query_Type.author:
                        sindex += "srw.au=\"" + query[x] + "\" " + sconditional + " ";
                        break;
                    case Query_Type.subject:
                        sindex += "srw.su=\"" + query[x] + "\" " + sconditional + " ";
                        break;
                    case Query_Type.title:
                        sindex += "srw.ti=\"" + query[x] + "\" " + sconditional + " ";
                        break;
                    default:
                        sindex += "srw.kw=\"" + query[x] + "\" " + sconditional + " ";
                        break;
                }
            }

            sindex = sindex.Substring(0, sindex.Length - (" " + sconditional + " ").Length);
            sindex = System.Uri.EscapeDataString(sindex);
            string url = OCLC_WORLDCAT_URL.Replace("{query}", sindex).Replace("{wskey}", wskey);
            
            string xml = helpers.MakeOpenHTTPRequest(url, SetProxy, "GET");
            if (xml == null || xml == "")
            {
                return null;
            }
            else
            {
                return xml;
            }
        }

        public string DoSearch(string url, string terms)
        {
            string escaped_terms = System.Uri.EscapeDataString(terms);
            url = url.Replace("{terms}", escaped_terms);
            return helpers.MakeOpenHTTPRequest(url, SetProxy, "GET");
        }

        public System.Collections.ArrayList DoSearchEx(string url, string terms)
        {
            struct_Records st_Recs = new struct_Records();

            string escaped_terms = System.Uri.EscapeDataString(terms);
            url = url.Replace("{terms}", escaped_terms);
            string xml = helpers.MakeOpenHTTPRequest(url, SetProxy, "GET");
            if (xml == null || xml == "")
            {
                return null;
            }
            else
            {
                return Process_SRU(xml);
            }

        }

        private System.Collections.ArrayList Process_SRU(string xml)
        {


            System.Xml.XmlTextReader rd;
            System.Collections.ArrayList tp = new System.Collections.ArrayList();

            System.Xml.XmlDocument objDoc = new System.Xml.XmlDocument();
            objDoc.XmlResolver = null;
            System.Xml.XmlNamespaceManager Manager = new System.Xml.XmlNamespaceManager(objDoc.NameTable);
            System.Xml.XmlNodeList objNodes;
            string RetrievedRecords = "0";
            System.Collections.ArrayList RecordSet = new System.Collections.ArrayList();

            rd = new System.Xml.XmlTextReader(xml, System.Xml.XmlNodeType.Document, null);
            string RecordPosition = "1";

            while (rd.Read())
            {
                if (rd.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (rd.Name.IndexOf("numberOfRecords") > -1)
                    {
                        RetrievedRecords = rd.ReadString();
                    }
                    if (rd.Name.IndexOf("recordData") > -1)
                    {
                        RecordSet.Add(rd.ReadInnerXml());
                        //this needs to go somewhere
                    }
                    if (rd.Name.IndexOf("recordPosition") > -1)
                    {
                        RecordPosition = rd.ReadString();
                    }
                }
            }

            rd.Close();



            for (int x = 0; x < RecordSet.Count; x++)
            {
                struct_Records st_recs = new struct_Records();
                st_recs.xml = (string)RecordSet[x];

                Manager.AddNamespace("marc", "http://www.loc.gov/MARC21/slim");
                //try
                //{
                objDoc.LoadXml((string)RecordSet[x]);
                objNodes = objDoc.SelectNodes("marc:record/marc:datafield[@tag='150']", Manager);
                if (objNodes == null)
                {
                    objNodes = objDoc.SelectNodes("record/datafield[@tag='150']", Manager);
                }
                foreach (System.Xml.XmlNode objNode in objNodes)
                {
                    st_recs.xml = objNode.InnerXml;

                    System.Xml.XmlNodeList codes = objNode.SelectNodes("marc:subfield", Manager);
                    if (codes == null)
                    {
                        codes = objNode.SelectNodes("subfield", Manager);
                    }


                    foreach (System.Xml.XmlNode objN in codes)
                    {
                        st_recs.display += objN.InnerText + " -- ";
                        st_recs.main += "$" + objN.Attributes["code"].InnerText + objN.InnerText;
                    }

                    if (st_recs.display != null)
                    {
                        st_recs.display = st_recs.display.TrimEnd(" -".ToCharArray());
                    }
                    else
                    {
                        st_recs.display = "";
                    }

                }

                if (objNodes.Count <= 0)
                {

                    st_recs.main = "Undefined";
                    st_recs.xml = "<undefined>undefined</undefined>";
                }
                //}
                //catch
                //{
                //    return null;
                //}
                tp.Add(st_recs);
            }

            RecordCount = System.Convert.ToInt32(RetrievedRecords);
            return tp;

        }
        
    }
    
}
