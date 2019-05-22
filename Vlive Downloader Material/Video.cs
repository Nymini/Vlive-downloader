using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vlive_Downloader_Material
{
    class Video
    {
        private Dictionary<string, string> resolutions;
        private string name;
        public Video(Dictionary<string, string> res, string name)
        {
            this.resolutions = res;
            this.name = name;
        }

        public bool resExists(string res)
        {
            foreach (string key in resolutions.Keys)
            {
                if (key == res)
                {
                    return true;
                }
            }

            return false;
        }

        public string getLink(string res)
        {
            if (resExists(res))
            {
                return resolutions[res];
            }
            else
            {
                return null;
            }
        }

        public string getName()
        {
            return name;
        }
    }
}
