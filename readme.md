# Xiaoya Search

Xiaoya Search is a simple search engine for LAN, which provides clear interface and structure.

Xiaoya Search mainly aims at educational use, and doesn't support distributed crawling and indexing, which means the proficiency may be not as good as expected.

Currently, Xiaoya Search will load all urls to crawl into memory, so it cannot support large-scale crawling now.

However, it is sufficient for a small LAN, such as campus network. 

My initial goal is to deploy this search engine in Beijing Normal University.

## Dependencies

Xiaoya Search uses: `boost`, `rocksdb`, `protobuf`, `grpc` and `uriparser`.

## Install

### Prepare include and library files of the dependecies above.

[vcpkg](https://github.com/Microsoft/vcpkg) is recommended tool to install all these libraries.

Just run `vcpkg install boost rocksdb protobuf grpc uriparser`.

### Build XiaoyaSearch

1. Clone this repo
2. Open solution file with Visual Studio
3. Build solution

### Run

1. Run `XiaoyaStorePlatform (.exe)` first, it'll create database files in the same directory.  
   It's written in C++, so you can just double click it to run.
2. Run `XiaoyaCrawlerInterface (.dll)` to crawl and index webpages (Set parameters as you need)
   It's .NET program, so you should execute it using dotnet command or directly in Visual Studio.
3. Run `XiaoyaSearchInterface (.dll)` to search webpages (Set parameters as you need)
   It's .NET program, so you should execute it using dotnet command or directly in Visual Studio.
4. Run `XiaoyaSearchWeb (.dll)` to host web interface of XiaoyaSearch (Set parameters as you need)
   It's ASP.NET program, so you should execute it using dotnet command or directly in Visual Studio.

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