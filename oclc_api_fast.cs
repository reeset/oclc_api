using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    public class oclc_api_fast
    {
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
            //http://fast.oclc.org/fastIndex/select/?q=keywords%3A(Gettysburg+gac%23n%23us%23pa%23+)&rows=10&start=0&version=2.2&indent=on&fl=id,fullphrase,type,usage,status&sort=usage%20desc
            return helpers.MakeOpenHTTPRequest(url, SetProxy, "GET"); // helpers.MakeHTTPRequest(url, SetProxy);
        }
    }
}
