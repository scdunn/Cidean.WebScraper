using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Cidean.WebScraper.Core
{
    /// <summary>
    /// Definition file for content to scrape and output configuration
    /// </summary>
    [XmlRoot("DataMap")]
    public class DataMap
    {

        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlArray]
        [XmlArrayItem(ElementName = "Url")]
        public List<string> Urls { get; set; }

        [XmlAttribute("Delay")]
        public int Delay { get; set; }

        [XmlArray]
        [XmlArrayItem(ElementName = "DataMapItem")]
        public List<DataMapItem> DataMapItems { get; set; }


        /// <summary>
        /// Load datamap xml file and deserialize into object.  Static method on class.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>DataMap</returns>
        public static DataMap LoadFile(string filename)
        {
            //return value
            DataMap map;

            //create serializer
            XmlSerializer serializer = new XmlSerializer(typeof(DataMap));

            //open filestream and create reader
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                XmlReader reader = XmlReader.Create(fs);

                //deserialize to datamap object and close filestream
                map = (DataMap)serializer.Deserialize(reader);
                fs.Close();
            }
                
            //return result map
            return map;
        }


    }
}
