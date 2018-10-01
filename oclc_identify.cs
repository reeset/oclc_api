using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    public class oclc_identify
    {
        public oclc_identify(string oclc_wskey, string oclc_wskey_secret,
                                 string oclc_principalID, string oclc_principalIDNS)
        {
            helpers.wskey = oclc_wskey;
            helpers.wskey_secret = oclc_wskey_secret;
            helpers.principleID = oclc_principalID;
            helpers.principleDNS = oclc_principalIDNS;

        }

        public string Debug_String
        {
            get { return helpers.debug_string; }
        }

        public string  ResolveRegistrySymbol(string OCLC_Symbol)
        {
            return helpers.ResolveRegistrySymbol(OCLC_Symbol);
        }

        public string ResolveRegistryId(string OCLC_Registry_ID)
        {
            return helpers.ResolveRegistrySymbol(OCLC_Registry_ID);
        }
    }
}
