using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cidean.WebScraper
{
    /// <summary>
    /// Scraping manager for executing scrap and loading maps
    /// </summary>
    public class Scraper
    {

        public DataMap DataMap;

        public Scraper()
        {
            
        }

        /// <summary>
        /// Execute the scrap of data from urls
        /// </summary>
        public void Execute(DataMap datamap)
        {
            this.DataMap = datamap;

            //only continue if datamap is defined
            if (DataMap == null)
                return;

            foreach(string url in DataMap.Urls)
            {
                //crawl
                Console.WriteLine("Crawling..." + url);
            }

        }
        

        /// <summary>
        /// Log scraping event
        /// </summary>
        /// <param name="message"></param>
        public void LogEvent(string message)
        {

        }

}
}
