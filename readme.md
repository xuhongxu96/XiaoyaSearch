# Xiaoya Search

Xiaoya Search is a simple search engine for LAN, which provides clear interface and structure.

Xiaoya Search mainly aims at educational use, and doesn't support distributed crawling and indexing, which means the proficiency may be not as good as expected.

Currently, Xiaoya Search will load all urls to crawl into memory, so it cannot support large-scale crawling now.

However, it is sufficient for a small LAN, such as campus network. 

My initial goal is to deploy this search engine in Beijing Normal University.

## Dependencies

Xiaoya Search uses: `boost`, `rocksdb`, `protobuf`, `grpc` and `uriparser`.

## Structure

- XiaoyaCommon  
  Including some common algorithms and functions
- XiaoyaCrawler  
  A multi-thread continuous crawler and indexer
	- UrlFrontier
	- Fetcher
	- SimilarContentManager
	- UrlFilter (To confine urls within the specific LAN domain)
- XiaoyaCrawlerInterface  
  Commandline interface for XiaoyaCrawler
- XiaoyaFileParser
	- UniversalFileParser
	- Parsers
		- TextFileParser
		- HtmlFileParser
		- PdfFileParser
- XiaoyaNLP
  NLP library
	- Text Segmentation
	- Encoding Detector
	- Word Stemmer
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
- XiaoyaSearch   
  Execute searching workflow (Query Parse -> Retrieve -> Rank -> Represent)
- XiaoyaSearchInterface  
  Commandline interface for XiaoyaSearch
- XiaoyaSearchWeb  
  Web interface for XiaoyaSearch
- XiaoyaStore
	- UrlFrontierItemStore  
	  Manage urls in UrlFrontier
	- UrlFileStore  
	  Manage fetched web content
	- PostingListStore   
	  Manage postings of words
	- InvertedIndexStore  
	  Manage Inverted Indices and their other props
	- LinkStore   
	  Manage links of UrlFiles
- XiaoyaStorePlatform (C++)  
  Core store procedure implemented in C++ equipped with `rocksdb`   
  Provides RPC interface for XiaoyaStore (C#) using `grpc`
- XiaoyaStorePlatformInterface (C++)  
  Commandline interface for XiaoyaStorePlatform
- XiaoyaLogger  
  A concurrent logger

## Line Count

Line count is calculated by: 

``` shell
git ls-files *.{cs,cpp,h} | xargs wc -l > linecount.txt
```

  ---

  Hongxu Xu (R) 2018