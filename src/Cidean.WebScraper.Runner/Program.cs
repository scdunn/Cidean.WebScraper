using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cidean.WebScraper.Core;

namespace Cidean.WebScraper.Runner
{
    internal class Program
    {

        //text divider for a line in console ie '============='
        private static string LineDivider = new string('=', 120);

        //base directory and all files relative to this dir
        private static readonly string baseDirectory;

        //default static constructor
        static Program()
        {
            //set base directory given environment
            #if DEBUG
                baseDirectory = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\";
            #else
                baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            #endif
        }

        /// <summary>
        /// main console start
        /// </summary>
        /// <param name="args"></param>
        internal static void Main(string[] args)
        {
            //data map file
            string dataMapFile = "";

            //data output file
            string dataOutFile = "";

            //page download delay
            int delay = 0;

            Console.WriteLine("Welcome to Cidean's WebScraper.");
            Console.WriteLine(LineDivider);
            Console.WriteLine("Please use this application responsibly and respect all copyrighted material.");

            try
            {
                //Check for command arguments, required for continuing
                if (args.Length == 0)
                    throw new ArgumentNullException("No arguments set.");

                //argument index for looping
                int argsIndex = 0;

                //grab command arguments and parameters
                while (true)
                {
                    switch (args[argsIndex].ToLower())
                    {
                        //data map file command
                        case "-m":
                            if (((args.Length) > (argsIndex)) && (!args[argsIndex + 1].StartsWith("-")))
                            {
                                dataMapFile = args[argsIndex + 1];
                                Console.WriteLine("Data Map File: " + dataMapFile);
                                argsIndex++;
                            }
                            break;
                        //data output file command
                        case "-o":
                            if (((args.Length) > (argsIndex)) && (!args[argsIndex + 1].StartsWith("-")))
                            {
                                dataOutFile = args[argsIndex + 1];
                                Console.WriteLine("Data Output File: " + dataOutFile);
                                argsIndex++;
                            }
                            break;
                        //delay between page downloads
                        case "-d":
                            if (((args.Length) > (argsIndex)) && (!args[argsIndex + 1].StartsWith("-")))
                            {
                                if (!int.TryParse(args[argsIndex + 1], out delay))
                                    throw new ArgumentException("Delay is not an number.");
                                Console.WriteLine("Data Output File: " + dataOutFile);
                                argsIndex++;
                            }
                            break;
                        case "-r"://test: remove
                            break;
                        default:
                            //bad argument
                            throw new ArgumentException("Unknown argument " + args[argsIndex]);
                    }

                    argsIndex++;
                    //check if all arguments have been processed.
                    if (argsIndex >= args.Length) break;
                }

                if (string.IsNullOrEmpty(dataMapFile) || string.IsNullOrEmpty(dataOutFile))
                    throw new ArgumentNullException("Missing Datamap file or Output file.");

                //set file to relative path
                dataMapFile = Path.Combine(baseDirectory, dataMapFile);

                dataOutFile = Path.Combine(baseDirectory, dataOutFile);

                //initialize web scraper and load data map
                Scraper scraper = new Scraper();
                scraper.LoggedEvent += Scraper_LoggedEvent;
                scraper.Delay = delay;

                //create datamap from xml file
                DataMap map = DataMap.LoadFile(dataMapFile);
                Console.WriteLine("Data map {0} loaded successfully.", dataMapFile);

                //execute webscraping
                scraper.Execute(map, Path.Combine(baseDirectory, dataOutFile));
            }
            catch (Exception ex)
            {
                //arguments didn't work out
                Console.WriteLine(ex.Message);
                Console.WriteLine("Exiting WebScraper Runner.");
                Console.ReadKey();
                return;
            }
            finally
            {
                //exit application
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            

        }

        //Event handler for scraping events.  Log to console.
        private static void Scraper_LoggedEvent(object sender, LoggedEventArgs e)
        {
            //log event to console
            Console.WriteLine(e.TimeStamp.ToString() + ": " + e.Message);
        }

        

    }
}
