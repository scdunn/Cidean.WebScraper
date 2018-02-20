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

        //Status events during scrape executing
        public event EventHandler<LoggedEventArgs> LoggedEvent;

        public DataMap DataMap;

        //Delay between url downloads to prevent overloading server
        public int Delay; //milliseconds
        

        public Scraper()
        {
            //default delay between page downloads
            Delay = 5000; //5 seconds
        }

        /// <summary>
        /// Execute the scrap of data from urls
        /// </summary>
        public void Execute(DataMap datamap, string outputFile)
        {
            //set current datamap
            this.DataMap = datamap;

            //update delay if defined
            if (DataMap.Delay > 0)
                this.Delay = DataMap.Delay;

            //only continue if datamap is defined
            if (DataMap == null)
            {
                LogEvent("Data Map is empty, cancel execution."); 
                return;
            }

            //create root output element
            XElement xmlRoot = new XElement(DataMap.Name);

            //loop all Urls
            foreach (string url in DataMap.Urls)
            {

                //create url output element
                XElement xmlUrl = new XElement("Source", new XAttribute("Url",url));
                
                //Fetch document and extract data
                LogEvent("Fetching Document at " + url);
                var document = GetHtmlDocument(url);

                if(document != null)
                {
                    LogEvent("Extracting Document data.");
                    //Extract Map Items
                    ExecuteDataMapItems(DataMap.DataMapItems, document.DocumentElement, xmlUrl);
                }

                //add url output element to output root
                xmlRoot.Add(xmlUrl);

                //delay between next url grab in milliseconds
                //to prevent overloading server.
                LogEvent("Delay for " + Delay + "ms");
                System.Threading.Thread.Sleep(Delay);

            }

            //write output xml to file
            LogEvent("Saving xml output file " + outputFile);
            xmlRoot.Save(outputFile);
        }
        


        /// <summary>
        /// Execute child items within map item list
        /// </summary>
        private void ExecuteDataMapItems(List<DataMapItem> dataMapItems, IElement element, XElement output)
        {
            
            //loop through all data map items
            foreach (var dataMapItem in dataMapItems)
            {
                //handle text item
                if (dataMapItem.Type.ToLower() == "text")
                {
                    IElement valueElement = QueryElement(element, dataMapItem.Path);
                    string value = ""; //default empty value

                    //check if value was found for path
                    if(valueElement != null)
                    { 
                        value = valueElement.TextContent.Trim();
                        LogEvent("Extracting path(" + dataMapItem.Path + ") to " + dataMapItem.Name + "=" + value);
                    }
                    else
                    {
                        //element does not exist, path may be incorrect, value will
                        //be written as empty text.
                        LogEvent("Path " + dataMapItem.Path + " does not exist.");
                    }

                    //write xml output element for value
                    output.Add(new XElement(dataMapItem.Name, value));
                }

                //handle list map types (has child map items)
                if (dataMapItem.Type.ToLower() == "list")
                {
                    //get all list item elements from from selector
                    var elementList = QueryElements(element, dataMapItem.Path);
                    
                    //create list output xml element
                    XElement xmlList = new XElement(dataMapItem.ListName);
                    LogEvent("Creating List..." + dataMapItem.ListName);

                    //loop through all elements and extract all datamapitems
                    foreach (var elementListItem in elementList)
                    {
                        XElement xmlListItem = new XElement(dataMapItem.Name);
                        ExecuteDataMapItems(dataMapItem.DataMapItems, elementListItem, xmlListItem);
                        xmlList.Add(xmlListItem);
                    }

                    //add list to xml output element
                    output.Add(xmlList);
                    
                }

            }
        }

        /// <summary>
        /// Log scraping action
        /// </summary>
        /// <param name="message"></param>
        private void LogEvent(string message)
        {
            if (LoggedEvent != null)
                LoggedEvent(this, new LoggedEventArgs() { TimeStamp = DateTime.Now, Message = message });
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
                LogEvent("File " + uri + " not found or document is empty.");
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
