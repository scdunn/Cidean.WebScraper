# Cidean WebScraper

Crawls a series of urls and extracts content from the page into an xml file.  The original goal of this project was to extract test data for prototype projects.  

[![Build Status](https://dev.azure.com/chris0850/chris/_apis/build/status/scdunn.Cidean.WebScraper?branchName=master)](https://dev.azure.com/chris0850/chris/_build/latest?definitionId=1&branchName=master)

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

```<?xml version="1.0" encoding="utf-8" ?>
<DataMap Name="Amazon"  >
<Urls>
  <Url><![CDATA[amazon-mystery-list.html]]></Url>
  <Url><![CDATA[https://web.archive.org/web/20150616214557/http://www.amazon.com/gp/bestsellers/books/18]]></Url>
</Urls>
<DataMapItems>
  <DataMapItem Type="text" Path="#zg_listTitle" Name="Title"/>
  <DataMapItem Type="list" Path=".zg_itemImmersion" ListName="Books" Name="Book">
      <DataMapItems>
        <DataMapItem Type="text" Path=".zg_rankDiv" Name="Rank"/>
        <DataMapItem Type="text" Path=".zg_title" Name="Title"/>
        <DataMapItem Type="text" Path=".zg_byline" Name="Byline"/>
        <DataMapItem Type="text" Path=".price" Name="Price"/>
        <DataMapItem Type="image" Path="img" Name="Thumb"/>
      </DataMapItems>
    </DataMapItem>
</DataMapItems>    

</DataMap>
```

