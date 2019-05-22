using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vlive_Downloader_Material
{
    class Subtitle
    {
        private Dictionary<string, string> subtitles;
        public Subtitle(Dictionary<string, string> subs)
        {
            subtitles = subs;
        }

        public bool subtitleExists(string sub)
        {
            foreach (string s in subtitles.Keys)
            {
                if (s == sub)
                {
                    return true;
                }

            }
            return false;
        }

        public string getSub(string sub)
        {
            if (subtitleExists(sub))
            {
                return subtitles[sub];
            }
            return null;
        }
    }
}
