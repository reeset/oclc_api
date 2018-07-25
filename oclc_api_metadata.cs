using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oclc_api
{
    public class oclc_api_metadata
    {

        public const string BIB_DATA_URI = @"https://worldcat.org/bib/data";
        public const string HOLDINGS_DATA_URI = @"https://worldcat.org/ih/data";
        public const string HOLDINGS_CODES_DATA_URI = @"https://worldcat.org/bib/holdinglibraries";
        public const string LOCAL_BIB_DATA_URI = @"https://worldcat.org/lbd/data";
        public const string LOCAL_BIB_DATA_SEARCH_URI = @"https://worldcat.org/lbd/search";
        public const string VALIDATE_DATA_URI = @"https://worldcat.org/bib/validateFull";


        private System.Net.WebProxy pSetProxy = null;
        private string pLastError = "";
        private string pPrincipleID = "";
        private string pPrincipleIDNS = "";
        private string pLastResponseCode = "";
        private string pwskey = "";
        private string pwskey_secret = "";
        private string pWorldCat_Service_URI = "";
        private string pDebug_Info = "";
        private string pInstSymbol = "";
        private string pAttributeInst = "";


        public oclc_api_metadata()
        {
            WorldCat_Service_URI = BIB_DATA_URI;
        }

        public oclc_api_metadata(string oclc_wskey, string oclc_wskey_secret,
                                 string oclc_principalID, string oclc_principalIDNS)
        {
            Wskey = oclc_wskey;
            Secret_Key = oclc_wskey_secret;
            PrincipleID = oclc_principalID;
            PrincipleIDNS = oclc_principalIDNS;

        }

        public string LastResponseCode
        {
            get { return pLastResponseCode; }
            set { pLastResponseCode = value; }
        }

        public string Debug_Info
        {
            get { return pDebug_Info; }
            set { pDebug_Info = value; }
        }

        public string LastError
        {
            get { return pLastError; }
            set { pLastError = value; }
        }

        public string PrincipleID {
            get {return pPrincipleID;}
            set {
                pPrincipleID = value;
                helpers.principleID = value;       
            }
        }

        public string PrincipleIDNS {
            get {return pPrincipleIDNS;}
            set {
                pPrincipleIDNS = value;
                helpers.principleDNS = value;
            }
        }

        public System.Net.WebProxy SetProxy
        {
            get { return pSetProxy; }
            set { pSetProxy = value;
                helpers.internal_proxy = value;
            }
        }

        public string WorldCat_Service_URI
        {
            get { return pWorldCat_Service_URI; }
            set { pWorldCat_Service_URI = value; }
        }

        public string Wskey
        {
            get { return pwskey; }
            set { 
                pwskey = value;
                helpers.wskey = value;
            }
        }

        public string Secret_Key
        {
            get { return pwskey_secret; }
            set { 
                pwskey_secret = value;
                helpers.wskey_secret = value;
            }
        }

        public string InstSymbol
        {
            get { return pInstSymbol; }
            set {
                if (pInstSymbol != value)
                {
                    //Tokens are cleared because we may have
                    //swapped institutions
                    helpers.ClearTokens();
                }
                pInstSymbol = value;
                if (helpers.IsNumeric(pInstSymbol) == false)
                {
                    helpers.institution_id = helpers.ResolveRegistrySymbol(value);
                }
                else
                {
                    helpers.institution_id = value;
                    pInstSymbol = helpers.ResolveRegistrySymbol(value, true);
                }
            }
        }

        public string AttributeSymbol
        {
            get { return pAttributeInst; }
            set {
                if (pAttributeInst != value)
                {
                    //Tokens are cleared because we may have swapped
                    //affiliate institutions
                    helpers.ClearTokens();
                }
                pAttributeInst = value;
                if (helpers.IsNumeric(pAttributeInst) == false)
                {
                    helpers.attribute_id = helpers.ResolveRegistrySymbol(value);
                } else
                {
                    helpers.attribute_id = value;
                    pAttributeInst = helpers.ResolveRegistrySymbol(value, true);
                }
            }
        }

        public bool UseToken
        {
            get { return helpers.useToken; }
            set { helpers.useToken = value; }
        }

        public string WorldCatValidateRecord(string xRecord, string inst, 
            string schema)
        {
            
                try
                {
                    LastResponseCode = "";
                    string ErrorResponse = "";
                    string base_url = WorldCat_Service_URI;
                    helpers.wskey = Wskey;
                    helpers.wskey_secret = Secret_Key;


                    base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema;
                    string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "POST", xRecord);
                    Debug_Info = helpers.debug_string + "\n\n" + base_url;
                    LastResponseCode = response;
                    if (helpers.IsError(response, out ErrorResponse) == true)
                    {
                        LastResponseCode = ErrorResponse;
                        return "";
                    }
                    return response;
                }
                catch
                {
                    return "";
                }
            
        }

        public bool WorldCatAddRecord(string xRecord, string inst,
                              string schema, string holdingCode)
        {
            try
            {
                LastResponseCode = "";
                string ErrorResponse = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;
                
                
                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "POST", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch (System.Exception response_error)
            {
                LastError = response_error.ToString();
                return false;
            }
        }

        public bool WorldCatUpdateRecord(string xRecord, string inst,
                                 string schema, string holdingCode)
        {
            try
            {
                LastResponseCode = "";
                string ErrorResponse = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                
                //base_url = BuildAuthorization(base_url);
                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "PUT", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //https://worldcat.org/bib/data/823520553?holdingCode=ORE&classificationScheme=LibraryOfCongress&holdingLibraryCode=MAIN
        public bool WorldCatGetRecord(string schema, string holdingCode, string holdingLibraryCode,
                                       string oclcNumber, out string sRecord)
        {
            sRecord = "";
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                if (base_url.EndsWith("/") == false)
                {
                    base_url += "/";
                }
                base_url += oclcNumber + "?holdingCode=" + holdingCode + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingLibraryCode;
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "GET");
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                sRecord = response;
                return true;
            }
            catch (System.Net.WebException e)
            {
                Debug_Info += "\n\n" + e.ToString() + "\n\n";
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool WorldCatAddHolding(string schema, string holdingCode,
                                       string oclcNumber)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                base_url += "?classificationScheme=" + schema + "&instSymbol=" + InstSymbol + "&holdingLibraryCode=" + holdingCode + "&oclcNumber=" + oclcNumber;
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "POST");

                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
                catch (System.Net.WebException e) {
                    Debug_Info += "\n\n" + e.ToString() + "\n\n";
                    return false;
                }
            catch
            {
                return false;
            }
        }

        public bool WorldCatDeleteHolding(string schema, string holdingCode,
                                          string oclcNumber)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                
                base_url += "?classificationScheme=" + schema + "&instSymbol=" + InstSymbol  + "&holdingLibraryCode=" + holdingCode + "&oclcNumber=" + oclcNumber + "&cascade=1";
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "DELETE");
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string[] WorldCatRetrieveHoldingCodes(int inst)
        {
            return WorldCatRetrieveHoldingCodes(inst, null);
        }

        public string[] WorldCatRetrieveHoldingCodes(string instSymbol)
        {
            return WorldCatRetrieveHoldingCodes(-1, instSymbol);
        }

        public string[] WorldCatRetrieveHoldingCodes(int inst, string instSymbol)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                if (inst > 0)
                {
                    base_url += "?inst=" + inst.ToString();
                }
                else if (instSymbol != null)
                {
                    base_url += "?instSymbol=" + instSymbol;
                }

                
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "GET");
               
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                //System.Windows.Forms.MessageBox.Show(Debug_Info);
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return null;
                }
                else
                {
                    return ProcessHoldingResponse(response);
                }
                
            }
            catch
            {
                return null;
            }
        }


        //https://worldcat.org/lbd/search?oclcNumber={oclcNumber}&classificationScheme={scheme}&holdingLibraryCode={holdingLibraryCode}

        public bool WorldCatSearchForLocalBibRecords(string schema, string holdingCode,
                                               string oclcNumber, out string sRecord)
        {
            sRecord = "";
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;


                base_url += "?oclcNumber=" + oclcNumber + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "GET");
                sRecord = response;
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //https://worldcat.org/lbd/data/{accessionNumber}?classificationScheme={scheme}&holdingLibraryCode={holdingLibraryCode} 

        public bool WorldCatReadLocalBibRecord(string schema, string holdingCode,
                                               string oclcNumber, out string sRecord)
        {
            sRecord = "";
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                if (base_url.EndsWith("/") == false)
                {
                    base_url += "/";
                }

                base_url += oclcNumber + "?classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "GET");
                sRecord = response;
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //is this a post request?
        //https://worldcat.org/lbd/data?inst={inst}&classificationScheme={scheme}&holdingLibraryCode={holdingLibraryCode}
        public bool WorldCatDeleteLocalBibRecord(string xRecord, string schema, string holdingCode,
                                                 string instSymbol)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                string ErrorResponse = "";
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;


                base_url += "?classificationScheme=" + schema + "&instSymbol=" + InstSymbol + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "DELETE", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                Debug_Info += "\n" + LastResponseCode + "\n";

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        //https://worldcat.org/lbd/data?inst={inst}&classificationScheme={scheme}&holdingLibraryCode={holdingLibraryCode}
        public bool WorldCatAddLocalBibRecord(string xRecord, string inst,
                              string schema, string holdingCode)
        {
            try
            {
                
               

                LastResponseCode = "";
                string ErrorResponse = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;


                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "POST", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

       


        //https://worldcat.org/lbd/data?inst={inst}&classificationScheme={scheme}&holdingLibraryCode={holdingLibraryCode}
        public bool WorldCatUpdateLocalBibRecord(string xRecord, string inst,
                                 string schema, string holdingCode)
        {
            try
            {
                LastResponseCode = "";
                string ErrorResponse = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;


                //base_url = BuildAuthorization(base_url);
                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "PUT", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;

                if (helpers.IsError(response, out ErrorResponse) == true)
                {
                    LastResponseCode = ErrorResponse;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string[] ProcessHoldingResponse(string xmlResponse)
        {
            List<string> xml_list = new List<string>();
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(new System.IO.StringReader(xmlResponse)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        //if (reader.IsStartElement() || reader.MoveToContent() == System.Xml.XmlNodeType.Element) {
                            if (reader.LocalName == "holdingCode")
                            {
                                //XElement el = XNode.ReadFrom(reader) as XElement;
                                xml_list.Add(reader.ReadElementContentAsString());
                            }
                        //}
                    }
                }
            }

            if (xml_list.Count > 0)
            {
                return xml_list.ToArray();
            } else
            {
                return null;
            }
        }
           //string sMessage = "";

            //System.Xml.XmlDocument objXML = new System.Xml.XmlDocument();
            //objXML.LoadXml(xmlResponse);
            //System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(objXML.NameTable);
            //nsmgr.AddNamespace("x", "http://www.w3.org/2005/Atom");
            //nsmgr.AddNamespace("os", "http://a9.com/-/spec/opensearch/1.1/");
            //nsmgr.AddNamespace("oclc", "http://worldcat.org/rb/holdinglibrary");

            //sMessage = "";

            //System.Xml.XmlNodeList nodes = objXML.SelectNodes("/x:feed/x:entry", nsmgr);
            //if (nodes != null)
            //{
            //    System.Windows.Forms.MessageBox.Show(nodes.Count.ToString());
            //    foreach (System.Xml.XmlNode node in nodes)
            //    {
            //        sMessage = node.SelectSingleNode("x:content/x:library/x:holdingCode", nsmgr).InnerText;
            //        alist.Add(sMessage);
            //    }
            //}

            //if (alist.Count > 0)
            //{
            //    string[] arrOut = new string[alist.Count];
            //    alist.CopyTo(arrOut);
            //    return arrOut;
            //}

        public string OutputAuthorization_String(string url, string method )
        {
            LastResponseCode = "";
            
            helpers.wskey = Wskey;
            helpers.wskey_secret = Secret_Key;

            
            string response = helpers.GenerateAuthorization(url, method);
            return response;
        }

        public string OuputAccessToken()
        {
            LastResponseCode = "";
            helpers.wskey = Wskey;
            helpers.wskey_secret = Secret_Key;
            if (helpers.access_token_table != null)
            {
                return (string)helpers.access_token_table["access_token"];
            }
            string response = helpers.GenerateAccessToken(SetProxy);
            return response;

        }
        internal string BuildAuthorization(string uri)
        {
            uri += "?principalID=" + PrincipleID + "&principalIDNS=" + PrincipleIDNS;
            return uri;
        }

    }
}
