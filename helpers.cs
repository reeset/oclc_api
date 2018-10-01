using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    internal class helpers
    {
       
        internal static string OCLC_DATA_URL = @"https://worldcat.org/lbd/data";
        internal static string schema_url = @"http://www.worldcat.org/wskey/v2/hmac/v1";
        internal static string accesstoken_url = @"https://authn.sd00.worldcat.org/oauth2/accessToken";
        //internal static string registry_url = @"http://www.worldcat.org/webservices/registry/lookup/Institutions/oclcSymbol/";
        internal static string registry_url = @"https://worldcat.org/oclc-config/institution/search?q=local.oclcSymbol:";
        internal static string registry_id_url = @"https://worldcat.org/oclc-config/institution/data/";
        internal static string wskey = "";
        internal static string wskey_secret = "";
        internal static string principleID = "";
        internal static string principleDNS = "";
        internal static string debug_string = "";
        internal static string attribute_id = "";
        internal static string institution_id = "";
        internal static string oclcHoldings = "";
        internal static string afficliateHoldings = "";
        internal static Random random = new Random((int)DateTime.Now.Ticks);
        internal static System.Collections.Hashtable access_token_table = null;
        internal static bool useToken = true;
        internal static System.Net.WebProxy internal_proxy = null;

        

        internal static bool IsNumeric(string s, string allowed = "")
        {
            if (String.IsNullOrEmpty(s)) return false;
            for (int x = 0; x < s.Length; x++)
            {
                if (Char.IsNumber(s[x]) == false)
                {
                    if (allowed.Trim().Length != 0)
                    {
                        if (allowed.IndexOf(s[x]) == -1)
                        {
                            return false;

                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #region Authentication Code

        internal static void ClearTokens()
        {
            if (access_token_table != null)
            {
                access_token_table.Clear();
                access_token_table = null;
            }
        }

        internal static string ResolveRegistrySymbol(string OCLC_Symbol, bool bid = false)
        {
            if (String.IsNullOrEmpty(OCLC_Symbol)) { return ""; }
            System.Net.HttpWebRequest wr;

            try
            {
                string url = "";
                string secure_auth = "";
                if (bid == false)
                {
                    url = registry_url + OCLC_Symbol;
                    //System.Windows.Forms.MessageBox.Show(url);
                    secure_auth = GenerateAuthorization(url, "GET");
                    //System.Windows.Forms.MessageBox.Show(secure_auth);

                }
                else
                {
                    url = registry_id_url + OCLC_Symbol;
                    if (access_token_table == null)
                    {
                        GenerateAccessToken(internal_proxy);
                    }
                    else
                    {
                        DateTime right_now = DateTime.Now;
                        DateTime expires_at = DateTime.Parse((string)access_token_table["expires_at"]);

                        if (right_now < expires_at)
                        {
                            GenerateAccessToken(internal_proxy);
                        }
                    }
                    if (access_token_table == null ||
                        access_token_table["access_token"]==null)
                    {
                        debug_string += "Unable to generate access token";
                        return "";
                    }
                    secure_auth = "Bearer" + " " + access_token_table["access_token"];



                }

                //System.Windows.Forms.MessageBox.Show(secure_auth);
                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                wr.Headers.Add(System.Net.HttpRequestHeader.Authorization, secure_auth);
                //wr.ContentType = "application/vnd.oclc.marc21+xml"; // "application/atom+xml;content=\"application/vnd.oclc.marc21+xml\""; // "application/atom+xml";
                //wr.UserAgent = "MarcEdit";
                wr.AllowAutoRedirect = false;
                wr.UserAgent = "MarcEdit";

                wr.Accept = "application/json";
                wr.Method = "GET";
                string response_string = "";
                try
                {
                    System.Net.WebResponse response = wr.GetResponse();
                    System.IO.Stream response_stream = response.GetResponseStream();
                    response_string = new System.IO.StreamReader(response_stream).ReadToEnd();
                    response_stream.Close();

                    string oclc_regid = "";
                    if (bid == false)
                    {
                        oclc_regid = ReadJson(response_string, "entries[0].content.institution.identifier");
                        string[] parts = oclc_regid.Split("/".ToCharArray());
                        return parts.Last<string>();

                    }
                    else
                    {
                        return ReadJson(response_string, "entries[0].content.institution.identifiers.oclcSymbol");
                    }                    
                }
                catch (System.Exception innererror) {
                    debug_string += "Inner Error: \n" + innererror.ToString() + "\nResponse String: \n" + response_string;
                } //System.Windows.Forms.MessageBox.Show("Error in inner loop" + "\n" + innererror.ToString()); }
            }
            catch (System.Exception outererror) {
                debug_string += "Outer Error: \n" + outererror.ToString();
            } // System.Windows.Forms.MessageBox.Show("Error in outer loop"); }

            return "";

        }



        internal static string ReadJson(string json, string path)
        {
            Newtonsoft.Json.Linq.JToken jtoken = Newtonsoft.Json.Linq.JObject.Parse(json);
                        string jstring = "";

            jstring = (string)jtoken.SelectToken(path);

            //System.Windows.Forms.MessageBox.Show(turi);
            if (!string.IsNullOrEmpty(jstring))
            {
                return jstring;
            }
            else
            {
                return "";
            }

        }
        internal static string GenerateAccessToken(System.Net.WebProxy proxy)
        {           
            string timestamp = ConvertToTimestamp(DateTime.UtcNow);
            string noce = RandomString(30);

            if (attribute_id == "") { attribute_id = institution_id; }

            string uri = accesstoken_url + "?grant_type=client_credentials&authenticatingInstitutionId=" + institution_id +
                                           "&contextInstitutionId=" + attribute_id + "&scope=WorldCatMetadataAPI";

            debug_string += "Access Token URI: " + uri;
            string digest = signature_base_string(timestamp, noce, uri, "POST");

            string auth = "";
            auth += schema_url + " ";
            auth += "clientId=\"" + wskey + "\", ";
            auth += "timestamp=\"" + timestamp + "\", ";
            auth += "nonce=\"" + noce + "\", ";
            auth += "signature=\"" + encrypt(digest) + "\", ";
            auth += "principalID=\"" + principleID + "\", ";
            auth += "principalIDNS=\"" + principleDNS + "\"";

            debug_string += "\n\n" + auth + "\n\n";

            System.Net.HttpWebRequest wr;
            System.IO.Stream objStream = null;
            //System.Windows.Forms.MessageBox.Show(debug_string);
            //System.Windows.Forms.MessageBox.Show(uri);
            try
            {
                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
                if (proxy != null)
                {
                    wr.Proxy = proxy;
                }

                //System.Windows.Forms.MessageBox.Show(auth);
                wr.Headers.Add(System.Net.HttpRequestHeader.Authorization, auth);
                System.Diagnostics.Debug.Print(auth);

                //wr.ContentType = "application/json";
                wr.Accept = "application/json";
                //wr.Accept = "application/json";
                //wr.Host = accesstoken_url;
                wr.ContentLength = 0;
                wr.UserAgent = "MarcEdit";

                wr.Method = "POST";



                System.Net.WebResponse response = wr.GetResponse();
                objStream = response.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                //System.Threading.Thread.Sleep(300); //small sleep to ensure that the stream is captured?
                //string sresults = reader.ReadToEnd();
                string sresults = "";
                char[] readBuffer = new char[256];
                int count = reader.Read(readBuffer, 0, 256);
                while (count > 0)
                {
                    sresults += new string(readBuffer, 0, count);
                    count = reader.Read(readBuffer, 0, 256);
                }
                reader.Close();
                //System.Windows.Forms.MessageBox.Show("results: " + sresults);
                System.Diagnostics.Debug.Print(sresults);
                access_token_table = ParseJson(sresults);

                return "true";

            }
            catch (System.Net.WebException web_e)
            {
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                debug_string += "Generate Access Token Error: \n" + web_e.ToString();
                //System.Windows.Forms.MessageBox.Show(resp + "\n" + web_e.ToString());
                return resp;
            }
            catch (Exception e)
            {
                debug_string += "Generate Access Token Error: \n" + e.ToString();
                //System.Windows.Forms.MessageBox.Show(e.ToString());
                return e.ToString();
            }
        }

        internal static System.Collections.Hashtable ParseJson(string json)
        {
            //standards based deserialize -- this is supported on the mac side.
            //System.Web.Script.Serialization.JavaScriptSerializer jsonobject = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Hashtable>(json);
        }
        internal static string GenerateAuthorization(string uri, string method)
        {
            string timestamp = ConvertToTimestamp(DateTime.UtcNow);
            string noce = RandomString(30);



            string digest = signature_base_string(timestamp, noce, uri, method);

            string auth = "";
            auth += schema_url + " ";
            auth += "clientId=\"" + wskey + "\", ";
            auth += "timestamp=\"" + timestamp + "\", ";
            auth += "nonce=\"" + noce + "\", ";
            auth += "signature=\"" + encrypt(digest) + "\", ";
            auth += "principalID=\"" + principleID + "\", ";
            auth += "principalIDNS=\"" + principleDNS + "\"";

            debug_string += "\n\n" + auth + "\n\n";
            return auth;


        }

        private static string encrypt(string digest)
        {
 
            System.Security.Cryptography.HMACSHA256 objhmac = new System.Security.Cryptography.HMACSHA256(new ASCIIEncoding().GetBytes(wskey_secret));
            string signature = Convert.ToBase64String(objhmac.ComputeHash(new ASCIIEncoding().GetBytes(digest))).TrimEnd("\n".ToCharArray()).Replace("\n", "");
            return signature;
        }

        private static string signature_base_string(string timestamp, string noce, string uri, string method)
        {

            string str = "";

            str += wskey + "\n";
            str += timestamp + "\n";
            str += noce + "\n";
            str += "\n";    //Not using body hash
            str += method + "\n";
            str += "www.oclc.org\n";
            str += "443\n";
            str += "/wskey\n";
            str += normalize_query_string(uri) + "\n";

            debug_string += str + "\n\n";
            return str;

        }

        private static string normalize_query_string(string uri)
        {
            System.Collections.ArrayList escaped_params = new System.Collections.ArrayList();
            System.Uri main_uri = new Uri(uri);
            string[] query_params = main_uri.Query.Substring(1).Split("&".ToCharArray());
            foreach (string i_param in query_params)
            {
                if (i_param.Trim().Length > 0)
                {
                    string key = i_param.Split("=".ToCharArray())[0];
                    string val = i_param.Split("=".ToCharArray())[1];
                    val = System.Uri.UnescapeDataString(val);
                    escaped_params.Add(key + "=" + System.Uri.EscapeDataString(val).Replace("+", "%20"));
                }
            }
            escaped_params.Sort();
            string[] clone_array = new string[escaped_params.Count];
            escaped_params.CopyTo(clone_array);
            return string.Join("\n", clone_array);
        }

        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            string ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)).ToString();
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private static string ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            //TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime());

            //return the total seconds (which is a UNIX timestamp)
            //return ((int)span.TotalSeconds).ToString();

            var date = new DateTime(1970, 1, 1, 0, 0, 0, value.Kind);
            var unixTimestamp = System.Convert.ToInt64((value - date).TotalSeconds);

            return unixTimestamp.ToString();

        }

        #endregion

        #region MakeHTTPRequest
        internal static string MakeHTTPRequest(string url)
        {

            return MakeHTTPRequest(url, null, "GET");
        }

        internal static string MakeHTTPRequest(string url, System.Net.WebProxy proxy)
        {
            return MakeHTTPRequest(url, proxy, "GET");
        }

        
        internal static string MakeHTTPRequest(string url, System.Net.WebProxy proxy, string sMethod, string accept_code = "application/atom+xml")
        {
            System.Net.Http.HttpClient client = null;            

            //debug_string = "";
            //string secure_auth = GenerateAuthorization(url, sMethod);
            string secure_auth = "";
            

            System.Diagnostics.Debug.Print(secure_auth);
            System.Diagnostics.Debug.Print(url);
            debug_string += secure_auth;
            //System.Windows.Forms.MessageBox.Show("secure path: " + secure_auth);
            try
            {
                debug_string += "Create URL: " + url;

                System.Net.Http.HttpClientHandler httpClientHandler = new System.Net.Http.HttpClientHandler();
                if (proxy != null)
                {
                    httpClientHandler.Proxy = proxy;
                }

                client = new System.Net.Http.HttpClient(httpClientHandler);
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(secure_auth);
                if (useToken == true)
                {
                    if (access_token_table == null)
                    {
                        ClearTokens();
                        GenerateAccessToken(proxy);
                    }
                    else
                    {
                        DateTime right_now = DateTime.Now;
                        DateTime expires_at = DateTime.Parse((string)access_token_table["expires_at"]);

                        if (right_now < expires_at)
                        {
                            ClearTokens();
                            GenerateAccessToken(proxy);
                        }
                    }
                    if (access_token_table == null ||
                        access_token_table.ContainsKey("access_token") == false)
                    {
                        debug_string += "Generate Access Token Error\n";
                        return "";
                    }

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token_table["access_token"].ToString());                    
                    secure_auth = "Bearer" + " " + access_token_table["access_token"];
                }
                else
                {
                    secure_auth = GenerateAuthorization(url, sMethod);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", secure_auth);
                }
                client.DefaultRequestHeaders.Add("User-Agent", "MarcEdit");
                //client.DefaultRequestHeaders.Add("Content-Type", "application/atom+xml");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", accept_code);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/atom+xml");

                System.Net.Http.HttpResponseMessage response = null;
                switch (sMethod.ToLower())
                {
                    case "get":
                        response = client.GetAsync(url).Result;
                        break;
                    case "delete":
                        response = client.DeleteAsync(url).Result;
                        break;
                    case "post":
                        response = client.PostAsync(url, new System.Net.Http.StringContent("")).Result;
                        break;
                    case "put":
                        response = client.PutAsync(url, new System.Net.Http.StringContent("")).Result;
                        break;

                }
                
                System.Diagnostics.Debug.Print("response message");
                //response.EnsureSuccessStatusCode();
                string sresults = response.Content.ReadAsStringAsync().Result;
                client.Dispose();
                return sresults;
            }
            catch (System.Net.Http.HttpRequestException re)
            {
                return re.ToString();
            }
            catch (System.Net.WebException web_e)
            {
                System.Diagnostics.Debug.Print("web exception: " + web_e.ToString());
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.Print("WebException: " + resp);
                //System.Windows.Forms.MessageBox.Show(resp);
                return resp;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("General Error: " + e.ToString());
                //System.Windows.Forms.MessageBox.Show(e.ToString());
                return e.ToString();
            }
        }


        #endregion

        #region MakeHTTP_POST_PUT_Request

        internal static string MakeHTTP_POST_PUT_Request(string url, System.Net.WebProxy proxy, 
                                                         string sMethod, string rec)
        {
            System.Net.Http.HttpClient client = null;

            //debug_string = "";
            //string secure_auth = GenerateAuthorization(url, sMethod);
            string secure_auth = "";


            System.Diagnostics.Debug.Print(secure_auth);
            System.Diagnostics.Debug.Print(url);
            debug_string += secure_auth;
            //System.Windows.Forms.MessageBox.Show("secure path: " + secure_auth);
            try
            {
                debug_string += "Create URL: " + url;

                System.Net.Http.HttpClientHandler httpClientHandler = new System.Net.Http.HttpClientHandler();
                if (proxy != null)
                {
                    httpClientHandler.Proxy = proxy;
                }

                client = new System.Net.Http.HttpClient(httpClientHandler);
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(secure_auth);
                if (useToken == true)
                {
                    if (access_token_table == null)
                    {
                        ClearTokens();
                        GenerateAccessToken(proxy);
                    }
                    else
                    {
                        DateTime right_now = DateTime.Now;
                        DateTime expires_at = DateTime.Parse((string)access_token_table["expires_at"]);

                        if (right_now < expires_at)
                        {
                            ClearTokens();
                            GenerateAccessToken(proxy);
                        }
                    }

                    if (access_token_table == null ||
                         access_token_table.ContainsKey("access_token") == false)
                    {
                        //an issue occurred with the registry API -- unable to 
                        //create token
                        debug_string += "Generate Access Token Error\n";
                        return "";
                    }

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token_table["access_token"].ToString());                    
                    secure_auth = "Bearer" + " " + access_token_table["access_token"];
                }
                else
                {
                    secure_auth = GenerateAuthorization(url, sMethod);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", secure_auth);
                }
                client.DefaultRequestHeaders.Add("User-Agent", "MarcEdit");
                //client.DefaultRequestHeaders.Add("Content-Type", "application/atom+xml");
                

                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/atom+xml;content=\"application/vnd.oclc.marc21+xml\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/vnd.oclc.marc21+xml");

                System.Net.Http.HttpResponseMessage response = null;
                switch (sMethod.ToLower())
                {                     
                    case "post":
                        response = client.PostAsync(url, new System.Net.Http.StringContent(rec, System.Text.Encoding.UTF8, "application/vnd.oclc.marc21+xml")).Result;
                        break;
                    case "put":
                        response = client.PutAsync(url, new System.Net.Http.StringContent(rec, System.Text.Encoding.UTF8, "application/vnd.oclc.marc21+xml")).Result;
                        break;

                }

                string sresults = response.Content.ReadAsStringAsync().Result;
                client.Dispose();
                //System.Windows.Forms.MessageBox.Show("results: " + sresults);
                return sresults;

            }
            catch (System.Net.WebException web_e)
            {
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                debug_string += "Put/Post Exception: \n" + web_e.ToString();
                return resp;
            }
            catch (Exception e)
            {
                debug_string += "Put/Post Exception: \n" + e.ToString();
                return e.ToString();
            }
        }

        #endregion
        #region MakeOpenHTTPRequests
        internal static string MakeOpenHTTPRequest(string url)
        {
            return MakeOpenHTTPRequest(url, null, "GET");
        }

        
        /// <summary>
        /// This is the non-authorized version of the HTTP request function
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxy"></param>
        /// <param name="sMethod"></param>
        /// <returns></returns>
        internal static string MakeOpenHTTPRequest(string url, System.Net.WebProxy proxy, string sMethod)
        {
            System.Net.HttpWebRequest wr;
            System.IO.Stream objStream = null;
            debug_string = "";

            try
            {
                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (proxy != null)
                {
                    wr.Proxy = proxy;
                }

                wr.UserAgent = "MarcEdit";
                wr.Method = sMethod;
                
                System.Net.WebResponse response = wr.GetResponse();
                objStream = response.GetResponseStream();


                // Releases the resources of the response.


                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                string sresults = reader.ReadToEnd();
                reader.Close();
                return sresults;
            }
            catch (Exception e)
            {

                return "";
            }
        }

        #endregion

        #region IsError
        internal static bool IsError(string xmlResponse, out string sMessage)
        {
            System.Xml.XmlDocument objXML = new System.Xml.XmlDocument();
            objXML.LoadXml(xmlResponse);
            System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(objXML.NameTable);
            nsmgr.AddNamespace("x", objXML.DocumentElement.NamespaceURI);
            sMessage = "";

            if (objXML.SelectSingleNode("/x:error/x:code", nsmgr) != null)
            {
                //System.Windows.Forms.MessageBox.Show(objXML.SelectSingleNode("/x:error/x:code", nsmgr).InnerText);
                sMessage = objXML.SelectSingleNode("/x:error/x:code", nsmgr).InnerText + ": " + objXML.SelectSingleNode("/x:error/x:message", nsmgr).InnerText;
                if (objXML.SelectSingleNode("/x:error/detail", nsmgr) !=null) {
                    //System.Windows.Forms.MessageBox.Show("here");
                    foreach (System.Xml.XmlNode node in objXML.SelectNodes("/x:error/x:detail/validationErrors")) {
                        sMessage += ": " + node.SelectSingleNode("validationError/message").InnerText;
                    }
                }
                return true;
            }
            
            return false;
        }
        #endregion


        /*
         * old code that doesn't work on macos
         * **************************************/
        #region OldMakeHTTPRequest
        internal static string MakeHTTPRequest2(string url, System.Net.WebProxy proxy, string sMethod, string accept_code = "application/atom+xml")
        {
            System.Net.HttpWebRequest wr;
            System.IO.Stream objStream = null;

            System.Net.ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);


            //debug_string = "";
            //string secure_auth = GenerateAuthorization(url, sMethod);
            string secure_auth = "";
            if (useToken == true)
            {
                if (access_token_table == null)
                {
                    ClearTokens();
                    GenerateAccessToken(proxy);
                }
                else
                {
                    DateTime right_now = DateTime.Now;
                    DateTime expires_at = DateTime.Parse((string)access_token_table["expires_at"]);

                    if (right_now < expires_at)
                    {
                        ClearTokens();
                        GenerateAccessToken(proxy);
                    }
                }
                if (access_token_table == null ||
                        access_token_table.ContainsKey("access_token") == false)
                {
                    debug_string += "Generate Access Token Error\n";
                    return "";
                }

                secure_auth = "Bearer" + " " + access_token_table["access_token"];
            }
            else
            {
                secure_auth = GenerateAuthorization(url, sMethod);
            }

            System.Diagnostics.Debug.Print(secure_auth);
            System.Diagnostics.Debug.Print(url);
            debug_string += secure_auth;
            //System.Windows.Forms.MessageBox.Show("secure path: " + secure_auth);
            try
            {
                debug_string += "Create URL: " + url;

                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (proxy != null)
                {
                    wr.Proxy = proxy;
                }

                System.Diagnostics.Debug.Print(wr.GetType().ToString());
                wr.UserAgent = "MarcEdit";
                wr.Headers.Add(System.Net.HttpRequestHeader.Authorization, secure_auth);
                wr.AllowReadStreamBuffering = false;
                wr.AllowWriteStreamBuffering = false;
                wr.ReadWriteTimeout = 15000;
                wr.ContentType = "application/atom+xml";
                //wr.Accept = accept_code;
                wr.Method = sMethod;


                System.Diagnostics.Debug.Print("Making the request");
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)wr.GetResponse();
                //System.Net.WebResponse response = wr.GetResponse();
                System.Diagnostics.Debug.Print("before headers");

                for (int i = 0; i < response.Headers.Count; ++i)
                    debug_string += "Key: " + response.Headers.Keys[i] + "\n" +
                                    "Value: " + response.Headers[i];

                System.Diagnostics.Debug.Print(debug_string);
                objStream = response.GetResponseStream();


                // Releases the resources of the response.

                System.Diagnostics.Debug.Print("making request for stream");
                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                //System.Threading.Thread.Sleep(1000);
                System.Diagnostics.Debug.Print("Reading data");
                string sresults = "";
                char[] readBuffer = new char[256];
                int count = reader.Read(readBuffer, 0, 256);
                while (count > 0)
                {
                    sresults += new string(readBuffer, 0, count);
                    count = reader.Read(readBuffer, 0, 256);
                }
                //string sresults = reader.ReadToEnd();

                //System.Windows.Forms.MessageBox.Show("Results: " + sresults + "\n" + debug_string);
                reader.Close();
                return sresults;
            }
            catch (System.Net.WebException web_e)
            {
                System.Diagnostics.Debug.Print("web exception: " + web_e.ToString());
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.Print("WebException: " + resp);
                //System.Windows.Forms.MessageBox.Show(resp);
                return resp;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("General Error: " + e.ToString());
                //System.Windows.Forms.MessageBox.Show(e.ToString());
                return e.ToString();
            }
        }
        #endregion

        #region OldMakeHTTP_POST_PUT_REQUEST
        internal static string MakeHTTP_POST_PUT_Request2(string url, System.Net.WebProxy proxy,
                                                         string sMethod, string rec)
        {
            System.Net.HttpWebRequest wr;
            System.IO.Stream objStream = null;

            debug_string = "";

            string secure_auth = "";
            if (useToken == true)
            {
                if (access_token_table == null)
                {
                    GenerateAccessToken(proxy);
                }
                else
                {
                    DateTime right_now = DateTime.Now;
                    DateTime expires_at = DateTime.Parse((string)access_token_table["expires_at"]);

                    if (right_now < expires_at)
                    {
                        GenerateAccessToken(proxy);
                    }
                }

                if (access_token_table == null ||
                        access_token_table.ContainsKey("access_token") == false)
                {
                    debug_string += "Generate access token error\n";
                    return "";
                }
                secure_auth = "Bearer" + " " + access_token_table["access_token"];
            }
            else
            {
                secure_auth = GenerateAuthorization(url, sMethod);
            }
            debug_string += secure_auth;

            try
            {
                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (proxy != null)
                {
                    wr.Proxy = proxy;
                }

                wr.Headers.Add(System.Net.HttpRequestHeader.Authorization, secure_auth);

                wr.ContentType = "application/vnd.oclc.marc21+xml"; // "application/atom+xml;content=\"application/vnd.oclc.marc21+xml\""; // "application/atom+xml";
                wr.Accept = "application/atom+xml;content=\"application/vnd.oclc.marc21+xml\"";
                wr.UserAgent = "MarcEdit";

                wr.Method = sMethod;

                System.IO.StreamWriter writer = new System.IO.StreamWriter(wr.GetRequestStream(), System.Text.Encoding.UTF8);
                writer.Write(rec);
                writer.Flush();
                writer.Close();

                System.Net.WebResponse response = wr.GetResponse();
                objStream = response.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                System.Threading.Thread.Sleep(300); //small sleep to ensure that the stream is captured?
                string sresults = reader.ReadToEnd();
                reader.Close();
                //System.Windows.Forms.MessageBox.Show("results: " + sresults);
                return sresults;

            }
            catch (System.Net.WebException web_e)
            {
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                return resp;
            }
            catch (Exception e)
            {

                return e.ToString();
            }
        }
        #endregion
        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
