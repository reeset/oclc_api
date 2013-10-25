using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oclc_api
{
    public class oclc_api_metadata
    {
        private System.Net.WebProxy pSetProxy = null;
        private string pLastError = "";
        private string pPrincipleID = "";
        private string pPrincipleIDNS = "";
        private string pLastResponseCode = "";
        private string pwskey = "";
        private string pwskey_secret = "";
        private string pWorldCat_Service_URI = @"https://worldcat.org/bib/data";
        private string pDebug_Info = "";
        private string pInstSymbol = "";


        public oclc_api_metadata()
        {
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
            set { pSetProxy = value; }
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
            set { pInstSymbol = value; }
        }

        
        public bool WorldCatAddRecord(string xRecord, string inst,
                              string schema, string holdingCode)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;
                
                
                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "POST", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WorldCatUpdateRecord(string xRecord, string inst,
                                 string schema, string holdingCode)
        {
            try
            {
                LastResponseCode = "";
                string base_url = WorldCat_Service_URI;
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                
                //base_url = BuildAuthorization(base_url);
                base_url += "?instSymbol=" + inst + "&classificationScheme=" + schema + "&holdingLibraryCode=" + holdingCode;
                string response = helpers.MakeHTTP_POST_PUT_Request(base_url, SetProxy, "PUT", xRecord);
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
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
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                base_url += "?classificationScheme=" + schema + "&instSymbol=" + InstSymbol + "&holdingLibraryCode=" + holdingCode + "&oclcNumber=" + oclcNumber;
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "POST");
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
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
                helpers.wskey = Wskey;
                helpers.wskey_secret = Secret_Key;

                
                base_url += "?classificationScheme=" + schema + "&instSymbol=" + InstSymbol  + "&holdingLibraryCode=" + holdingCode + "&oclcNumber=" + oclcNumber + "&cascade=1";
                string response = helpers.MakeHTTPRequest(base_url, SetProxy, "DELETE");
                Debug_Info = helpers.debug_string + "\n\n" + base_url;
                LastResponseCode = response;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string OutputAuthorization_String(string url, string method )
        {
            LastResponseCode = "";
            
            helpers.wskey = Wskey;
            helpers.wskey_secret = Secret_Key;

            
            string response = helpers.GenerateAuthorization(url, method);
            return response;
        }

        internal string BuildAuthorization(string uri)
        {
            uri += "?principalID=" + PrincipleID + "&principalIDNS=" + PrincipleIDNS;
            return uri;
        }

    }
}
