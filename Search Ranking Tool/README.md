# Automated Search Ranking Tool

As part of our on-going efforts to improve our search results within the EES website, we have been tweaking the various parameters we have available to us. In order to know whether what we are tweaking is helping or hindering, we built this tool, which takes a search query and looks to see where our expected top result ranks. If all of our expected search results come first, then our work is done. Or the tool is broken.

## Usage

The tool is written in .net c# and is a commandline tool. 

There are two modes of operation, a single query mode and a csv file mode.

### Search Types
| Search Type | Description                                                                                       |
| ----------- |---------------------------------------------------------------------------------------------------|
| Semantic | The standard text search including semantic reranking                                             |
| SemanticSpellChecked | Utilising the spelling correcter introduced in api version 2025-05-01-preview                     |
| FullText | A plain full text search without semantic reranking                                               |                          
| FullTextFuzzy2 | Performs both a fuzzy search for every word - standard fuzzy distance of up to 2                  |
| FullTextFuzzy3 | Performs an extended fuzzy search for every word - fuzzy distance of up to 3                      |
| FullTextFuzzy2Wildcard | Performs both a wildcard and standard fuzzy search for every word.                                |
| SemanticScoringProfile2 | Uses a second scoring profile against which we were making changes and wanted to compare results. |

#### Wildcard and Fuzzy Searches
Selecting the full text search types above perform the corresponding manipulation of the search term automatically.

By appending a `*`, a **_wildcard search_** treats the search term as a prefix for any matches. e.g. "education*" would match "educational" 

By appending a `~`, a **_fuzzy search_** will match words that up to 2 letters different to the search term. Similarly, `~3` will allow up to 3 differences. e.g. the misspelled "educatoin" would match "education"

## Searches
### Common command line arguments
| Argument    | Description                                                                                                                     | Example                                                                                   |
|-------------|---------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| azure url   | This is the url to which the request will be posted                                                                             | "https://mysearch-ees-srch.search.windows.net/indexes('myindex')/docs/search.post.search" |
| api key     | The api key to grant access to the index                                                                                        | In Azure Portal, navigate to the search service > settings > Keys |
| search type | The type of search to perform. See Search Type table                                                                            | Semantic |


### Single Query Mode
Command: **_search_**

```text
Command line Example:

.\SearchRankingTool.exe search "https://mysearchservicename-ees-srch.search.windows.net/indexes('myindex')/docs/search.post.search" "myapikey" FullText "dfe attendance data" "https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools"
```

| Argument    | Description                                                                                                                     | Example                                                                                   |
|-------------|---------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| query | The search query                                                                                                                | "special educational needs in england" |
| url to rank | Typically, this is the url of the result we consider to be the top ranked. The tool will tell us how this page actually ranked. | "https://explore-education-statistics.service.gov.uk/find-statistics/special-educational-needs-in-england" |

### CSV File Mode
Command: **_csv_**

```text
Command line Example:

.\SearchRankingTool.exe csv "https://mysearchservicename-ees-srch.search.windows.net/indexes('myindex')/docs/search.post.search" "myapikey" FullText searches.csv
```
| Argument | Description                                                            | Example       |
|----------|------------------------------------------------------------------------|---------------|
| input    | the filename of a csv file containing search query / url to rank pairs | "searches.csv" |

The input csv file contains a list of query, url pairs, one per line, separated by a comma

```text
dfe attendance data,https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools
schools pupils and their characteristics,https://explore-education-statistics.service.gov.uk/find-statistics/school-pupils-and-their-characteristics
```

Each query will be run through the search service, and the specified url found and the rank returned.

The results from these queries are placed into two files along side the input file:
- one file contains the input query, url and rank
- the other file contains an output of useful information such as for any searches where the expected url was not in the top 10, it will report what the top ten results were.

The files will be named based on the input filename, the search type and the output type.
e.g. for a semantic search with input file _searches.csv_, the output files are called _searches-semantic-results.csv_ and _searches-semantic-report.txt_.
