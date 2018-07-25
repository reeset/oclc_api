using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oclc_api
{
    class oclc_identify
    {
        public string  ResolveRegistrySymbol(string OCLC_Symbol)
        {
            return helpers.ResolveRegistrySymbol(OCLC_Symbol);
        }

        public string ResolveRegistryId(string OCLC_Registry_ID)
        {
            return helpers.ResolveRegistrySymbol(OCLC_Registry_ID, true);
        }
    }
}
