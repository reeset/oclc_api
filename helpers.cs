using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    internal static class helpers
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

        internal static string MakeHTTPRequest(string url)
        {
            
            return MakeHTTPRequest(url, null, "GET");
        }

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
        internal static string MakeHTTPRequest(string url, System.Net.WebProxy proxy)
        {
            return MakeHTTPRequest(url, proxy, "GET");
        }

        internal static void ClearTokens()
        {
            if (access_token_table != null)
            {
                access_token_table.Clear();
                access_token_table = null;
            }
        }

        internal static string MakeHTTPRequest(string url, System.Net.WebProxy proxy, string sMethod) {
            System.Net.HttpWebRequest wr;
            System.IO.Stream objStream = null;

            

            debug_string = "";
            //string secure_auth = GenerateAuthorization(url, sMethod);
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
                secure_auth = "Bearer" + " " + access_token_table["access_token"];
            }
            else
            {
                secure_auth = GenerateAuthorization(url, sMethod);
            }
            
            debug_string += secure_auth;
            //System.Windows.Forms.MessageBox.Show("secure path: " + secure_auth);
            try
            {
                wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (proxy != null)
                {
                    wr.Proxy = proxy;
                }

                wr.UserAgent = "MarcEdit";
                wr.Headers.Add(System.Net.HttpRequestHeader.Authorization, secure_auth);
                wr.ContentType = "application/atom+xml";
                wr.Method = sMethod;
            
                System.Net.WebResponse response = wr.GetResponse();
                for (int i = 0; i < response.Headers.Count; ++i)
                    debug_string += "Key: " + response.Headers.Keys[i] + "\n" +
                                    "Value: " + response.Headers[i];

                objStream = response.GetResponseStream();

                
                // Releases the resources of the response.


                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                string sresults = reader.ReadToEnd();

                //System.Windows.Forms.MessageBox.Show("Results: " + sresults + "\n" + debug_string);
                reader.Close();
                return sresults;
            }
            catch (System.Net.WebException web_e)
            {
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                //System.Windows.Forms.MessageBox.Show(resp);
                return resp;
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.ToString());
                return e.ToString();
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
                                     
                } else
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

                try
                {
                    System.Net.WebResponse response = wr.GetResponse();
                    System.IO.Stream response_stream = response.GetResponseStream();
                    string response_string = new System.IO.StreamReader(response_stream).ReadToEnd();
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
                    //System.Windows.Forms.MessageBox.Show(oclc_regid);
                    
                    //System.Windows.Forms.Clipboard.SetText(response_string);
                    //System.Windows.Forms.MessageBox.Show(response_string);
                    //if (response.Headers["Location"] != null)
                    //{
                    //    string id = response.Headers["Location"];
                    //    Console.WriteLine(id);

                    //    string[] parts = id.Split("/".ToCharArray());
                    //    return parts.Last<string>();
                    //}
                }
                catch (System.Exception innererror) { } //System.Windows.Forms.MessageBox.Show("Error in inner loop" + "\n" + innererror.ToString()); }
            }
            catch { } // System.Windows.Forms.MessageBox.Show("Error in outer loop"); }

            return "";

        }

        

        internal static string ReadJson(string json, string path)
        {
            Newtonsoft.Json.Linq.JToken jtoken = Newtonsoft.Json.Linq.JObject.Parse(json);
            //Newtonsoft.Json.Linq.JToken jttoken = Newtonsoft.Json.Linq.JObject.Parse(work_tmp);
            //int page = (int)token.SelectToken("page");
            //int totalPages = (int)token.SelectToken("total_pages");
            //

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
            string digest = signature_base_string(timestamp, noce, uri, "POST");

            //System.Windows.Forms.MessageBox.Show(uri + "\nattributid: " + attribute_id);
            /*
             * auth  = ""
            auth += "#{scheme_url} "
            auth += "clientId=\"#{client_id}\", "
            auth += "timestamp=\"#{req_timestamp}\", "
            auth += "nonce=\"#{req_nonce}\", "
            auth += "signature=\"#{signature(signature_base)}\""
            if @principal_on_header and !@principal_id.nil?
            auth += ", principalID=\"#{@principal_id}\", principalIDNS=\"#{@principal_idns}\""

             * */
            /*
             * digest = HMAC-SHA256 ( wskey_secret, prehashed_string )
signature = base64 ( digest )
            */

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
                wr.ContentType = "application/json";
                //wr.Accept = "application/json";
                //wr.Host = accesstoken_url;
                wr.ContentLength = 0;
                wr.UserAgent = "MarcEdit";

                wr.Method = "POST";

                

                System.Net.WebResponse response = wr.GetResponse();
                objStream = response.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(objStream);
                System.Threading.Thread.Sleep(300); //small sleep to ensure that the stream is captured?
                string sresults = reader.ReadToEnd();
                reader.Close();
                //System.Windows.Forms.MessageBox.Show("results: " + sresults);

                access_token_table = ParseJson(sresults);

                return "true";

            }
            catch (System.Net.WebException web_e)
            {
                string resp = new System.IO.StreamReader(web_e.Response.GetResponseStream()).ReadToEnd();
                //System.Windows.Forms.MessageBox.Show(resp + "\n" + web_e.ToString());
                return resp;
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.ToString());
                return e.ToString();
            }

            


        }

        internal static  System.Collections.Hashtable ParseJson(string json)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jsonobject = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsonobject.Deserialize<System.Collections.Hashtable>(json);            
        }
        internal static string GenerateAuthorization(string uri, string method)
        {
            string timestamp = ConvertToTimestamp(DateTime.UtcNow);
            string noce = RandomString(30);
            

            
            string digest = signature_base_string(timestamp, noce, uri, method);

            /*
             * auth  = ""
            auth += "#{scheme_url} "
            auth += "clientId=\"#{client_id}\", "
            auth += "timestamp=\"#{req_timestamp}\", "
            auth += "nonce=\"#{req_nonce}\", "
            auth += "signature=\"#{signature(signature_base)}\""
            if @principal_on_header and !@principal_id.nil?
            auth += ", principalID=\"#{@principal_id}\", principalIDNS=\"#{@principal_idns}\""

             * */   
            /*
             * digest = HMAC-SHA256 ( wskey_secret, prehashed_string )
signature = base64 ( digest )
            */

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
            /*
             *     digest = OpenSSL::Digest::SHA256.new
            hmac = OpenSSL::HMAC.digest( digest, @secret, base_string )
            Base64.encode64( hmac ).chomp.gsub( /\n/, '' )

             * */
            
            System.Security.Cryptography.HMACSHA256 objhmac = new System.Security.Cryptography.HMACSHA256(new ASCIIEncoding().GetBytes(wskey_secret));
            string signature = Convert.ToBase64String(objhmac.ComputeHash(new ASCIIEncoding().GetBytes(digest))).TrimEnd("\n".ToCharArray()).Replace("\n", "");
            return signature;
        }

        private static string signature_base_string(string timestamp, string noce, string uri, string method)
        {

            /*
             * def signature_base_string(req_timestamp, req_nonce, http_body, http_method, url)
            str  = ""
            str += "#{client_id}\n"
            str += "#{req_timestamp}\n"
            str += "#{req_nonce}\n"
            str += "\n" # Not using a body hash
            str += "#{http_method}\n"
            str += "www.oclc.org\n"
            str += "443\n"
            str += "/wskey\n"
            str += "#{normalized_query_str(url)}\n" if normalized_query_str(url).strip != ''
            puts "'#{str}'" if @debug_mode
            str
            end
            */

            string str = "";

            str += wskey + "\n";
            str += timestamp + "\n";
            str += noce + "\n";
            str += "\n";    //Not using body hash
            str += method + "\n";
            str += "www.oclc.org\n";
            str += "443\n";
            str += "/wskey\n";
            str += normalize_query_string(uri) +"\n";

            debug_string += str + "\n\n";
            return str;

        }

        private static string normalize_query_string(string uri)
        {
            System.Collections.ArrayList escaped_params = new System.Collections.ArrayList();
            System.Uri main_uri = new Uri(uri);
            string[] query_params = main_uri.Query.Substring(1).Split("&".ToCharArray());
            foreach (string i_param in query_params) {
                if (i_param.Trim().Length > 0) {
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

        internal static string MakeHTTP_POST_PUT_Request(string url, System.Net.WebProxy proxy, 
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


        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
