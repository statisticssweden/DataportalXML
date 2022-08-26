# DataportalXML
## Sammanfattning
DataportalXML är ett verktyg för att generera RDF/XML-filer enligt standarden DCAT-AP-SE. Varje fil beskriver metadata för en viss databas. Verktyget kan nås via PxWeb som ett admin-verktyg eller via en API-request till PxWeb. För att konfigurera verktyget används klassen structen RdfSettings som beskrivs nedan.

## RdfSettings
**BaseUri:** Baslänki som används inom XML-filen för att länka noder. Behöver ej vara en existerande URL. Ex. "<https://www.baseURI.se/>".

**BaseApiUrl:** Baslänk för API-anrop till en viss databas. Den slutliga länken kommer sedan byggas ut dynamiskt till {BaseApiUrl}/{Språk}/{DBid}/{Sökväg}.
Ex. “<http://api.scb.se/OV0104/v1/doris/>” kommer för tabellen “BefolkningNy” att resultera i länken “<http://api.scb.se/OV0104/v1/doris/sv/ssd/BE/BE0101/BE0101A/BefolkningNy>”. 

**Languages:** Språk som metadatan ska hämtas och tillgängliggöras för. Ges som en lista av ISO 639-1 språkkoder. Ex. {“sv”, “en”, “fi”} för språken svenska, engelska, och finska. 

**PreferredLanguage:** Huvudspråk för databasen. Ges som en ISO 639-1 språkkod. Ex. “sv”. 

**CatalogTitle:** Titel för katalogen som skapas av verktyget. Ex. “Scb:s tabeller”.

**CatalogDescription:** Katalogens beskrivning och den kan upprepas för flera olika språkversioner av samma text. 

**PublisherName:** Namn på enhet som ansvarar för att katalogen är tillgänglig. Ex. "SCB" eller "Skatteverk"

**Fetcher:** En instans av antingen en CNMMFetcher eller en PXFetcher beroende på vilken typ av databas som ska användas. 

**DBid:** Om databasen är en CNMM-databas anges databasens id. Ex. “ssd”. Om databasen är en PX-databas anges en sökväg till basnoden. Ex. "C:/Temp/Databases/Example/Menu.xml"

**LandingPageUrl:** Baslänk för ingångssida till en viss databas. Den slutliga länken kommer sedan byggas ut dynamiskt till {BaseApiUrl}/{Språk}/{DBid}/{Tabell}.
Ex. “<http://www.statistikdatabasen.scb.se/goto/>” kommer för tabellen “BefolkningNy” att resultera i länken “<http://www.statistikdatabasen.scb.se/goto/sv/ssd/BefolkningNy>”.  

**License:** Licens som datan görs tillgänglig för. DCAT-AP-SE version 2.0.0 (dataportal.se).
Ex. “<http://creativecommons.org/publicdomain/zero/1.0/>”. 

**ThemeMapping:** Sökväg till en JSON-fil som beskriver hur teman mappas till Dcat-ap-se standard. Ex. “C:/temp/TMapping.json”. [Mer om detta](#mappning-av-teman).

## Mappning av teman
Dcat-ap-se har en specifik standard för hur tabeller ska delas in i olika teman [DCAT-AP-SE version 2.0.0](https://docs.dataportal.se/dcat/sv/). SCB använder andra beteckningar och dessa måste då konverteras. Beteckningarna kan skilja sig mellan olika databaser och därför behöver mappningen variera. Mappningen bestäms av en JSON-fil med key-value par som beskriver hur varje beteckning ska konverteras. Exempel på en JSON-fil som gör detta:
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

Om DataportalXML används via PxWeb så används filen "/PxWeb/PxWeb/TMapping.json". Om en PX fil används där TMapping.json finns i rotkatalogen för databasen, kommer denna fil att användas istället. Ex. Om PX-databasen Example används och "PxWeb\PXWeb\Resources\PX\Databases\Example\TMapping.json" existerar kommer denna att användas. Om den inte existerar används istället "\PxWeb\PXWeb\TMapping.json"

## PxWeb Admin
DataportalXML kan nås genom ett verktyg på PxWeb under admin-sidorna. 

![image](https://user-images.githubusercontent.com/21987439/186614159-bab31909-583c-40e7-9b1a-e3e22ed37fc5.png)

Välj en databas och fyll i fälten som korresponderar till [RdfSettings](#rdfsettings) parametrar. 
Tryck sedan på generera knappen när dem önskade fälten är ifyllda och en XML/RDF fil kommer att skapas under "PxWeb\PXWeb\dcat-ap.xml". 
Genom att använda sig utav spara-knappen kan man spara de iskrivna fälten för senare tillfällen.

## Api

DataportalXML kan även nås via ett POST api-anrop till "/api/admin/v1/{DatabaseType}/{DBId}" där ``DatabaseType`` är antingen "px" eller "cnmm" beroende på vilken databas som ska användas. ``DBId`` är namnet på databasen t.ex. "ssd" eller "Example". Den slutgiltiga URLen kan då exempelvis bli "/api/admin/v1/cnmm/ssd/" eller "/api/admin/v1/px/Example/".

Dessa parametrar måste anges som query-parametrar:

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

 Dessa korresponderar direkt till [RdfSettings](#rdfsettings).

 Ett exempel på en komplett request: 
 POST "http://localhost:56338/api/admin/v1/dcat/px/StatFin?baseURI=https://base.com/&catalogTitle=Catalog+title&catalogDescription=Beskrivning+av+katalog&license=https://license.com&baseApiUrl=https://api.com/&landingPageUrl=https://landingPage.com/&publisher=SCB&preferredLanguage=en&languages=sv&languages=en&languages=fi"