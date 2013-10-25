Implementation Example:

string oclcnumber = "823520553";
string principleID = "[enter your principalID]";
string principlelDNS = "[enter your principalIDNS]";
string wskey = "[your oclc wskey]";
string secret = "[your secret key]";

string instSymbol = "[oclc provide institution code]"; 
string holdingsid = "[oclc provided code]"; //a four digit code from oclc; example MAIN
            
string schema = "LibraryOfCongress";
           
oclc_api.oclc_api_metadata obj_om = new oclc_api.oclc_api_metadata(wskey, secret, principleID, principlelDNS);
obj_om.WorldCat_Service_URI = @"https://worldcat.org/ih/data";
            
bool results = obj_om.WorldCatDeleteHolding(schema, instSymbol, oclcnumber);
System.Windows.MessageBox.Show(obj_om.LastResponseCode);
            