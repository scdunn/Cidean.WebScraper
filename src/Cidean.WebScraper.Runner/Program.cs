﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cidean.WebScraper.Core;

namespace Cidean.WebScraper.Runner
{
    public class Program
    {

        //text divider for a line in console ie '============='
        static string LineDivider = new string('=', 120);
        static readonly string baseDirectory;

        /// <summary>
        /// main console start
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Cidean's WebScraper.");
            Console.WriteLine(LineDivider);
            Console.WriteLine("Please use this application responsibly and respect all copyrighted material.");

            //Todo: allow input of datamap file, hardcode for now

            string filename = Path.Combine(baseDirectory, "datamaps\\amazon.xml");

            Scraper scraper = new Scraper();
            DataMap map = DataMap.LoadFile(filename);
            Console.WriteLine("Data map {0} loaded successfully.", filename);

            scraper.LoggedEvent += Scraper_LoggedEvent;
            scraper.Execute(map, Path.Combine(baseDirectory, "data.xml"));


            //Exit Application
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();


        }

        private static void Scraper_LoggedEvent(object sender, LoggedEventArgs e)
        {
            //log event too console
            Console.WriteLine(e.TimeStamp.ToString() + ": " + e.Message);
        }

        static Program()
        {
            //set base directory given environment
#if DEBUG
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\";
#else
                baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        }

    }
}
