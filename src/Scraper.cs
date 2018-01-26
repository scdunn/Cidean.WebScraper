using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom;
using System.Net;

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

            //loop all Urls
            foreach(string url in DataMap.Urls)
            {
                //crawl
                Console.WriteLine("Grabbing {0}", url);
                

            }

        }
        

        /// <summary>
        /// Log scraping event
        /// </summary>
        /// <param name="message"></param>
        public void LogEvent(string message)
        {

        }

        /// <summary>
        /// Retrieve remote html document
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private IDocument GetHtmlDocument(string uri)
        {
            //html to extract from document
            string html = "";
            string url = uri;
            
            //download html document from remote URI
            using (WebClient client = new WebClient())
            {
                //add request headers to appear as browser
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                client.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                html = client.DownloadString(url);
            }

            //parse html document
            var parser = new AngleSharp.Parser.Html.HtmlParser();

            //return parsed IDocument
            return parser.Parse(html);
        }

    }
}
