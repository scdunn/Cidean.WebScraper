using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cidean.WebScraper.Core
{
    public class ProgressChangedEventArgs
    {
        //timestamp of action
        public DateTime TimeStamp { get; set; }

        
        public int ProgressCount { get; set; }
        public int ProgressCurrent { get; set; }
    }
}
