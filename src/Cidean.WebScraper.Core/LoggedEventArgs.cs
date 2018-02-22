using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cidean.WebScraper.Core
{
    /// <summary>
    /// Event arguments for a completed action during scraping.
    /// </summary>
    public class LoggedEventArgs
    {
        //timestamp of action
        public DateTime TimeStamp;
        //description of action
        public string Message;
    }
}
