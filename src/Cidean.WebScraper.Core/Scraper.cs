using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom;
using System.Net;
using System.Xml;
using System.IO.Compression;

namespace Cidean.WebScraper.Core
{
    /// <summary>
    /// Scraping manager for executing scrap and loading maps
    /// </summary>
    public class Scraper
    {

        private XElement xmlRoot;

        //Status events during scrape executing
        public event EventHandler<LoggedEventArgs> LoggedEvent;

        //Marks progress on scraping (as much as possible)
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        //total number of links to download
        public int ProgressCount { get; set; }
        public int ProgressCurrent { get; set; }

        public DataMap DataMap;

        public string OutputRootPath;
        public string DataPath;

        //Delay between url downloads to prevent overloading server
        public int Delay; //milliseconds
        
        /// <summary>
        /// Default contructor to set defaults
        /// </summary>
        public Scraper()
        {
            //default delay between page downloads
            Delay = 5000; //5 seconds
        }

        /// <summary>
        /// Execute the scrap of data from urls
        /// </summary>
        public void Execute(DataMap datamap, string outputRootPath)
        {
            //set root output path for data/images
            this.OutputRootPath = outputRootPath;

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


            //create output folder if not already existing
            //and data folder will be timestamped
            //and auto created
            try {
                
                if (!Directory.Exists(this.OutputRootPath))
                        Directory.CreateDirectory(this.OutputRootPath);

                //generate timestamped data fold to house xml and other data
                //for scrape.
                this.DataPath = Path.Combine(this.OutputRootPath, this.DataMap.Name + "-" + DateTime.Now.ToFileTimeUtc().ToString());
                Directory.CreateDirectory(this.DataPath);

            }
            catch (Exception exOutputPath)
            {
                LogEvent("Output folder could not be created.");
                return;
            }

            //create root output element
            xmlRoot = new XElement(DataMap.Name);

            //set total links to urls to crawl
            ProgressCount = DataMap.Urls.Count();

            //loop all Urls
            foreach (string url in DataMap.Urls)
            {

                //create url output element
                XElement xmlUrl = new XElement("Source", new XAttribute("Url",url));
                
                //Fetch document and extract data
                LogEvent("Fetching Document at " + url);
                var document = GetHtmlDocument(url);

                IncreaseProgress("Crawling..." + url, 1);

                if(document != null)
                {   
                    //add url output element to output root
                    xmlRoot.Add(xmlUrl);

                    LogEvent("Extracting Document data.");
                    //Extract Map Items
                    ExecuteDataMapItems(DataMap.DataMapItems, document.DocumentElement, xmlUrl, url);
                    
                }



            }


            SaveXml();

        }
        
        public void SaveXml()
        {
            //write output xml to file
            LogEvent("Saving xml output file.");
            xmlRoot.Save(Path.Combine(this.DataPath, "data.xml"));
        }

        /// <summary>
        /// Execute child items within map item list
        /// </summary>
        private void ExecuteDataMapItems(List<DataMapItem> dataMapItems, IElement sourceElement, XElement output, string url)
        {
            var element = sourceElement;
            var dataMapItemIndex = 0;




            //base url (authority) to attach to relative urls
            var baseUrl = "";
            if(!string.IsNullOrEmpty(url))
                baseUrl = (new Uri(url)).GetLeftPart(UriPartial.Authority);

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
                        //custom value for attribute on element vs element inner value text
                        if(!string.IsNullOrEmpty(dataMapItem.Value))
                        {
                            //default is empty unless found in following statements
                            value = "";

                            //var attribute value
                            var attrName = dataMapItem.Value.Replace("@", "");

                            if(dataMapItem.Value.StartsWith("@"))
                                if (valueElement.HasAttribute(attrName))
                                    value = baseUrl + valueElement.GetAttribute(attrName);

                        }
                        else
                        {
                            //if no value property set, grab inner text of element
                            //by default.
                            value = valueElement.TextContent.Trim();
                        }

                        LogEvent("Extracting path(" + dataMapItem.Path + ") to " + dataMapItem.Name + "=" + value);
                    }
                    else
                    {
                        //element does not exist, path may be incorrect, value will
                        //be written as empty text.
                        LogEvent("Path " + dataMapItem.Path + " does not exist.");
                    }

                    //write xml output element for value
                    output.Add(new XElement(dataMapItem.Name, CleanString(value)));
                }

                //handle image item
                if (dataMapItem.Type.ToLower() == "image")
                {
                    IElement valueElement = QueryElement(element, dataMapItem.Path);
                    string src = ""; 
                    string value = "";

                    //check if value was found for path
                    if (valueElement != null)
                    {
                        src = valueElement.GetAttribute("src");
                        value = Path.GetFileName(src);

                        LogEvent("Extracting path(" + dataMapItem.Path + ") to " + dataMapItem.Name + "=" + value);
                        LogEvent("Download image..." + src);
                        if (src.StartsWith("/"))
                            src = (new Uri(url)).GetLeftPart(UriPartial.Authority) + src;
                        DownloadImage(src, value);
                    }
                    else
                    {
                        //element does not exist, path may be incorrect, value will
                        //be written as empty text.
                        LogEvent("Path " + dataMapItem.Path + " does not exist.");
                    }

                    //write xml output element for value
                    output.Add(new XElement(dataMapItem.Name, CleanString(value)));
                }

                //handle link item--follow page
                if (dataMapItem.Type.ToLower() == "link")
                {
                    IElement valueElement = QueryElement(element, dataMapItem.Path);
                    string value = ""; //default empty value

                    //check if value was found for path
                    if (valueElement != null)
                    {
                        

                        value = valueElement.GetAttribute("href");
                        if (!value.StartsWith("http"))
                            value = baseUrl + value;


                        LogEvent("Extracting path(" + dataMapItem.Path + ") to " + dataMapItem.Name + "=" + value);

                        //Fetch document and extract data
                        LogEvent("Fetching Document at " + value);
                        var document = GetHtmlDocument(value);

                        if (document != null)
                        {
                            LogEvent("Extracting Document data.");
                            //Extract Map Items
                            var xmlLink = new XElement(dataMapItem.Name);
                            if(!dataMapItem.ExcludeLinkUrl)
                            { 
                                xmlLink.Add(new XAttribute("Url", value));
                            }
                            //write xml output element for value
                            output.Add(xmlLink);


                            ExecuteDataMapItems(dataMapItem.DataMapItems, document.DocumentElement, xmlLink, value);
                            
                        }
                    }
                    else
                    {
                        //element does not exist, path may be incorrect, value will
                        //be written as empty text.
                        LogEvent("Path " + dataMapItem.Path + " does not exist.");
                    }
                    


                }

                //handle list map types (has child map items)
                if (dataMapItem.Type.ToLower() == "list")
                {
                    //get all list item elements from from selector
                    var elementList = QueryElements(element, dataMapItem.Path, dataMapItem.MaxItems);

                    
                        if (elementList.Count!=0)
                    {
                        //add total list items discovered to progress count
                        if (dataMapItem.IsProgress)
                            ProgressCount += elementList.Count;

                        //create list output xml element
                        XElement xmlList = new XElement(dataMapItem.ListName);
                        //add list to xml output element
                        output.Add(xmlList);
                        LogEvent("Creating List..." + dataMapItem.ListName + "...(count:" + elementList.Count + ")");

                        //loop through all elements and extract all datamapitems
                        foreach (var elementListItem in elementList)
                        {
                            if(!string.IsNullOrEmpty(dataMapItem.Name))
                            { 
                                XElement xmlListItem = new XElement(dataMapItem.Name);
                                xmlList.Add(xmlListItem);
                                ExecuteDataMapItems(dataMapItem.DataMapItems, elementListItem, xmlListItem, url);
                            }
                            else
                            {
                                ExecuteDataMapItems(dataMapItem.DataMapItems, elementListItem, xmlList, url);
                            }

                            //add total list items discovered to progress count
                            if (dataMapItem.IsProgress)
                            {
                                SaveXml();
                                IncreaseProgress("Crawling..." + dataMapItem.ListName, 1);
                            }
                        }

                        
                    }
                    else
                    {
                        LogEvent("No list of elements for path " + dataMapItem.Path + " was found.");

                    }
                }

            }
        }

        private void IncreaseProgress(string message, int value)
        {
            ProgressCurrent += value;
            if (ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(){ TimeStamp = DateTime.Now, ProgressCount = this.ProgressCount, ProgressCurrent = this.ProgressCurrent});

        }

        private void DoDelay()
        {
            //delay between next action (url grab, file download)
            LogEvent("Delay for " + Delay + "ms");
            System.Threading.Thread.Sleep(Delay);
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
                //delay before grab of document/file
                DoDelay();

                //html to extract from document
                string html = "";
                string url = uri;

                //download html document from remote URI
                html = DownloadString(url);

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


        public string DownloadString(string uri)
        {
            WebClient webClient = null;
            try
            {
                webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                webClient.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                byte[] data = webClient.DownloadData(uri);

                MemoryStream output = new MemoryStream();
                using (GZipStream stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    stream.CopyTo(output);
                }

                return Encoding.UTF8.GetString(output.ToArray());
            }
            catch
            {

            }
            finally
            {
                if (webClient != null)
                    webClient.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Query an element within a document given the selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private IElement QueryElement(IElement parent, string selector)
        {
            IElement element;

            if (selector == ".")
                element = parent;
            else
                element = parent.QuerySelector(selector);
            if (element == null) return null;
            return element;
        }

        /// <summary>
        /// Query ALL elements within a document given the selector
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private List<IElement> QueryElements(IElement parent, string selector, int maxItems)
        {
            var elements = parent.QuerySelectorAll(selector);
            if (elements == null) return null;
            if (maxItems > 0)
                return elements.Take(maxItems).ToList();
            else
                return elements.ToList();
        }


        private string CleanString(string value)
        {
            char[] validXmlChars = value.Trim().Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
            
        }

        /// <summary>
        /// Download Image from remote location
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        private bool DownloadImage(string uri, string fileName)
        {

            return true;
            //delay before grab of document/file
            DoDelay();

            var url = uri;
            
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Check that the remote file was found. The ContentType
                // check is performed since a request for a non-existent
                // image file might be redirected to a 404-page, which would
                // yield the StatusCode "OK", even though the image was not
                // found.
                if ((response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect) &&
                    response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {

                    // if the remote file was found, download oit
                    using (Stream inputStream = response.GetResponseStream())
                    using (Stream outputStream = File.OpenWrite(Path.Combine(this.DataPath,fileName)))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        do
                        {
                            bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                            outputStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead != 0);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogEvent(ex.Message + "  " + url);
                return false;
            }

        }

    }
}
