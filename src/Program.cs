using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cidean.WebScraper
{
    class Program
    {

        //text divider for a line in console ie '============='
        static string LineDivider = new string('=', 120);


        /// <summary>
        /// main console start
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Cidean's WebScraper.");
            Console.WriteLine(LineDivider);
            Console.WriteLine("Please use this application responsibly and respect all copyrighted material.");

            //Todo: BEGIN INPUT FOR PROCESSING

            //Exit Application
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
   
   
        }
        
    }
}
