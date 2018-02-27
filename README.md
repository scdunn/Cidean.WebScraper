# Cidean WebScraper

Crawls a series of urls and extracts content from the page into an xml file.  The original goal of this project was to extract test data for prototype projects.  

Solution is split into multiple projects.
### Cidean.WebScraper.Core
Contains the core processing logic for scraping data from a webpage.

### Cidean.WebScraper.Runner
Utilizes the Cidean.WebScraper.Core to run a console application executing the scrape process.

Usage:
>webscrape -m "example-datamap.xml" -o "example-output.xml"

### Other
Other projects will be included showcasing using the scraper in a web application.


## Configuration
The configuration for a given scrap/extract is stored in a Data Map, DataMap file in xml format.		