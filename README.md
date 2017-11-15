# Musigger Scrapper

[Musigger.com](http://musigger.com)
Web application for searching and filtering music releases. Musiggger provide flexible release filtering system by genres, types, labels, votes and even artists. Also it offers superrior perfomance and modern design.

## Getting Started

Clone a copy of git repository 
```
git clone git://github.com/Musigger/MusiggerScraper
```
There will be two projects. Run the **ReleaseCrawler** to generate database with CodeFirst. Then ensure that your connection string in the second project **CrawlerTask** corresponds your actual database in the App.config file.
```
<add name="DefaultConnection" connectionString="Data Source=your_dbserver;Initial Catalog=your_db;Persist Security Info=True;User ID=dbuser;Password='your_password'" providerName="System.Data.SqlClient" />
```

After that you can run the **CrawlerTask** to fill your database with new releases. You can adjust the amount of crawled pages in the ReleaseParser.Run(). 



## Built With

* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/) - HTML parser that builds a read/write DOM

## Authors

* **[Anton Zolotov](https://github.com/joseph2)** - *Client single page application*
* **[Egor Shoba](https://github.com/silentfobos)** - *Server Web API*

## License

This project is licensed under the Mozilla Public License 2.0 - see the LICENSE.md file for details