using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Cidean.WebScraper.Core
{

   
    /// <summary>
    /// Map information for page element to scrape
    /// </summary>
    
    public class DataMapItem
    {
        //type of element to extract (text/link/list)
        [XmlAttribute("Type")]
        public string Type { get; set; }

        //query path of element
        [XmlAttribute("Path")]
        public string Path { get; set; }

        //Export element name
        [XmlAttribute("Name")]
        public string Name { get; set; }

        //Value to be extracted (default is inner element text)
        [XmlAttribute("Value")]
        public string Value { get; set; }

        //True prefixes element value with Base Url (authority)
        [XmlAttribute("PrefixBaseUrl")]
        public bool PrefixBaseUrl { get; set; }

        //True indicates source of progress (count progression)
        [XmlAttribute("IsProgress")]
        public bool IsProgress { get; set; }

   
        //Export element name for list item
        [XmlAttribute("ListName")]
        public string ListName { get; set; }

        //Max Items to get (from list type)
        [XmlAttribute("MaxItems")]
        public int MaxItems { get; set; }

        //Exclude link url from link element
        [XmlAttribute("ExcludeLinkUrl")]
        public bool ExcludeLinkUrl { get; set; }


        //Child Map Items
        [XmlArrayItem(ElementName = "DataMapItem")]
        public List<DataMapItem> DataMapItems { get; set; }
    }
}
