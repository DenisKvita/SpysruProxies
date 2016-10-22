using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpysruProxiesEngineNs
{
    class ProxyPortDecoder
    {
        Dictionary<string, string> aDigits;
        public ProxyPortDecoder()
        {
            aDigits = new Dictionary<string, string>();
        }

        public bool init(string[] dsrc)
        {
            if (dsrc.Count() != 10) return false;
            foreach (string d in dsrc)
            {
                string[] dparts = d.Split(new char[]{'='});
                int di=0;
                if(!Int32.TryParse(dparts[1].Substring(0,1), out di)) return false;
                aDigits.Add(dparts[0], dparts[1].Substring(0,1));
            }

            return true;
        }

        public string decode(string coded)
        {
            List<string> pp = coded.Split(new string[]{"+("}, StringSplitOptions.None).ToList<string>();
            pp.RemoveAt(0);
            string res="";
            foreach (string pd in pp)
            {
                if(pd[6]!='^') return "";
                res += aDigits[pd.Substring(0, 6)];
            }
            return res;
        }
    }
}
