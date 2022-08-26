# DataportalXML
## Summary
DataportalXML is a tool for generating RDF/XML-files based on the DCAT-AP-SE standard. Each file describes metadata for a specific database. The tool can be accessed by PxWeb as a admin-tool or as a API-request to PxWeb. A class named RdfSettings is used to configure the tool which is described down below.

## RdfSettings
**BaseUri:** The base URI that's used within the XML-file does not need to be an existing URL. E.g. "<https://www.baseURI.se/>".

**BaseApiUrl:** Base URL for API-requests to a specific database. The final link will then be built out dynamicly to {BaseApiUrl}/{Language}/{DBid}/{Path}.
E.g. “<http://api.scb.se/OV0104/v1/doris/>” will for the table "BefolkningNy" result in the link: “<http://api.scb.se/OV0104/v1/doris/sv/ssd/BE/BE0101/BE0101A/BefolkningNy>”. 

**Languages:** Languages that the metadata will get and be made avalable for. Given by a list of ISO 639-1 languages codes. E.g. {“sv”, “en”, “fi”} for the languages swedish, english and finnish.

**PreferredLanguage:** Main language for the database. Is given as a 639-1 language code. E.g. "en".

**CatalogTitle:** Title for the created catalog by the tool. E.g. "Scb:s tables".

**CatalogDescription:** The catalogs description and it can be repeated mulitple times for different language versions of the same text.

**PublisherName:** The name of the unit that is responsible for making the catalog available. E.g. "SCB".

**Fetcher:** An instance of either CNMMFetcher or PXFetcher based on what type of database is specified.

**DBid:** If the database is a CNMM-database the id of that database will be specified. E.g. "ssd". If the database is a PX-database a search path to the basenode will be specified. E.g. "C:/Temp/Databases/Example/Menu.xml"

**LandingPageUrl:** Base URL for start page to a specific database. The final link will then be built out dynamicly to {BaseApiUrl}/{Språk}/{DBid}/{Tabell}. 
E.g. “<http://www.statistikdatabasen.scb.se/goto/>” the table "BefolkningNy" will result in the link: “<http://www.statistikdatabasen.scb.se/goto/sv/ssd/BefolkningNy>”.

**License:** License that the data is made available for. [DCAT-AP-SE version 2.0.0](https://docs.dataportal.se/dcat/sv/)
E.g. “<http://creativecommons.org/publicdomain/zero/1.0/>”. 

**ThemeMapping:** The path to a JSON file describing the mapping of themes to Dcat-ap-se standard. E.g. “C:/temp/TMapping.json”. [More about this](#mapping-of-themes).

## Mapping of themes
Dcat-ap-se has a specific standard for the partitioning of databases into themes [DCAT-AP-SE version 2.0.0](https://docs.dataportal.se/dcat/sv/). Other standards may be used and themes must therefore be converted. The theme codes can vary between different databases and thus the mapping of themes must vary for different databases. The mapping is read from a JSON-fil with key-value pairs describing how each code should be converted. 
An example of a valid JSON file:
<pre>
{
"AM": "SOCI",
"BE": "SOCI",
"BO": "SOCI",
"ME": "JUST",
"EN": "ENER",
"FM": "ECON",
"HA": "ECON",
"HE": "ECON",
"HS": "HEAL",
"JO": "AGRI",
"KU": "EDUC",
"LE": "SOCI",
"MI": "ENVI",
"NR": "ECON",
"NV": "ECON",
"OE": "GOVE",
"PR": "ECON",
"SO": "SOCI",
"TK": "TRAN",
"UF": "EDUC",
"AA": "SOCI"
}
</pre>

If DataportalXML is used through PxWeb the json-file under "/PxWeb/PxWeb/TMapping.json" will be used. If a PX database is used where TMapping.json exists in the root directory of the database, this file will be used insted. Example: If the PX database Example is used and "PxWeb\PXWeb\Resources\PX\Databases\Example\TMapping.json" does exist, this mapping will be used. If it does not exist the mapping  "\PxWeb\PXWeb\TMapping.json" will be used instead. 

## PxWeb Admin
DataportalXML kan be reached as a tool under the PxWeb admin pages.  

![image](https://user-images.githubusercontent.com/21987439/186614159-bab31909-583c-40e7-9b1a-e3e22ed37fc5.png)

Choose a database and fill in the settings corresponding to [RdfSettings](#rdfsettings). 
Use the generate button to generate the RDF/XML file, it will be saved under "PxWeb\PXWeb\dcat-ap.xml". 
The settings can be saved by utilising the save button. 

## Api

DataportalXML can be reached with a POST api request to "/api/admin/v1/{DatabaseType}/{DBId}" where ``DatabaseType`` is either "px" or "cnmm" depending on the type of database. ``DBId`` is the name of the database e.g. "ssd" or "Example". The final url can look something like: "/api/admin/v1/cnmm/ssd/" or "/api/admin/v1/px/Example/".

These following parameters must be set as query params:

<pre>
baseURI
catalogTitle
catalogDescription
license
baseApiUrl
landingPageUrl
publisher
preferredLanguage
languages
</pre>

These correspond directly to [RdfSettings](#rdfsettings).

Example of a complete request:
POST "http://localhost:56338/api/admin/v1/dcat/px/StatFin?baseURI=https://base.com/&catalogTitle=Catalog+title&catalogDescription=Beskrivning+av+katalog&license=https://license.com&baseApiUrl=https://api.com/&landingPageUrl=https://landingPage.com/&publisher=SCB&preferredLanguage=en&languages=sv&languages=en&languages=fi"

A valid request will result in the RDF/XML being created under "PxWeb\PXWeb\dcat-ap.xml".