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
        public void Execute(DataMap datamap, string outputFile)
        {
            
            this.DataMap = datamap;

            //only continue if datamap is defined
            if (DataMap == null)
                return;

            XElement xmlRoot = new XElement(DataMap.Name);

            //loop all Urls
            foreach (string url in DataMap.Urls)
            {

                XElement xmlUrl = new XElement("Source", new XAttribute("Url",url));
                
                //crawl
                Console.WriteLine("Grabbing {0}", url);
                var document = GetHtmlDocument(url);
                if (document != null)
                {
                   
                    //loop through all root data map items
                    foreach(var rootMapItem in DataMap.DataMapItems)
                    {
                        if(rootMapItem.Type.ToLower() == "text")
                        {
                            string value = QueryElement(document.DocumentElement, rootMapItem.Path).TextContent;
                            xmlUrl.Add(new XElement(rootMapItem.Name, value.Trim()));

                        }

                        //handle list map types (has child map items)
                        if (rootMapItem.Type.ToLower() == "list")
                        { 
                            
                            //get all list item elements from from selector
                            var elementList = QueryElements(document.DocumentElement, rootMapItem.Path);

                            XElement xmlList = new XElement(rootMapItem.ListName);

                            //loop through all elements and extract all datamapitems
                            foreach(var elementListItem in elementList)
                            {
                                XElement xmlListItem = new XElement(rootMapItem.Name);
                                foreach (var listMapItem in rootMapItem.DataMapItems)
                                {
                                    string value = QueryElement(elementListItem, listMapItem.Path).TextContent;
                                    xmlListItem.Add(new XElement(listMapItem.Name, value.Trim()));
                                }
                                xmlList.Add(xmlListItem);
                            }

                            xmlUrl.Add(xmlList);
                            
                          
                            
                        }
                        
                    }
                }
                xmlRoot.Add(xmlUrl);
            }
            xmlRoot.Save(outputFile);
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
            try
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
            catch (Exception ex)
            {
                //return null if any exception occurs
                return null;
            }
        }

        

        /// <summary>
        /// Query an element within a document given the selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private IElement QueryElement(IElement parent, string selector)
        {
            var element = parent.QuerySelector(selector);
            if (element == null) return null;
            return element;
        }

        private List<IElement> QueryElements(IElement parent, string selector)
        {
            var elements = parent.QuerySelectorAll(selector);
            if (elements == null) return null;
            return elements.ToList();
        }

    }
}
