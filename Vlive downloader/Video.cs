using System;
using System.Collections.Generic;

namespace Vlive_downloader
{
    public class Video
    {

        private Dictionary<string, string> resolutions;

        public Video(Dictionary<string, string> res)
        {
            this.resolutions = res;
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
    }
}
