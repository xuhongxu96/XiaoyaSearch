# Xiaoya Search

Xiaoya Search is a simple search engine for LAN, which provides clear interface and structure.

Xiaoya Search mainly aims at educational use, and doesn't support distributed crawling and indexing, which means the proficiency may be not as good as expected.

However, it is sufficient for a small LAN, such as campus network. 

My initial goal is to deploy this search engine in Beijing Normal University.

## Line Count

Line count is calculated by: 

``` shell
git ls-files *.cs | xargs wc -l > linecount.txt
```

## Structure

- XiaoyaCommon  
  Including some common algorithms and functions
- XiaoyaCrawler  
  A multi-thread continuous crawler
    - UrlFrontier
	- Fetcher
	- Parser
	- SimilarContentManager
	- UrlFilter (To confine urls within the specific LAN domain)
- XiaoyaCrawlerInterface  
  Commandline interface for XiaoyaCrawler
- XiaoyaFileParser
	- UniversalFileParser
	- Parsers
		- TextFileParser
		- HtmlFileParser
- XiaoyaIndexer
- XiaoyaIndexerInterface  
  Commandline interface for XiaoyaIndexer
- XiaoyaNLP
  NLP library
	- TextSegmentation
- XiaoyaRetriever
	- BooleanRetriever
	- InexactTopKRetriever
	- SearchExpression
- XiaoyaRanker
	- QueryTermProximityRanker
	- VectorSpaceModelRanker
	- DomainDepthRanker
- XiaoyaQueryParser  
  Parse free-text query to SearchExpression for XiaoyaRetriever
- XiaoyaStore
	- UrlFrontierItemStore  
	  Manage urls in UrlFrontier
	- UrlFileStore  
	  Manage fetched web content
	- InvertedIndexStore  
	  Manage Inverted Index
	- IndexStatStore  
	  Manage word frequency and document frequency of words in inverted index
	- UrlFileIndexStatStore  
	  Manage word frequency in each UrlFile
	- LinkStore
		Manage links in UrlFiles
- XiaoyaLogger  
  A concurrent logger

  ---

  Hongxu Xu (R) 2018