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
        private DataMap dataMap;

        /// <summary>
        /// Execute the scrap of data from urls
        /// </summary>
        public void Execute()
        {
            //only continue if datamap is defined
            if (dataMap == null)
                return;
        }

        /// <summary>
        /// Open datamap xml file and populate DataMap object
        /// </summary>
        public void LoadDataMapFile(string filename)
        {
            //check if file exists
            if (!File.Exists(filename))
                throw new FileNotFoundException();


            //load from xml file into xdocument
            XElement document = XElement.Load(new FileStream(filename, FileMode.Open));

            //create new datamap
            dataMap = new DataMap();

            //read all urls to crawl
            foreach(XElement element in document.Descendants("url"))
            {
                dataMap.Urls.Add(element.Value);
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
