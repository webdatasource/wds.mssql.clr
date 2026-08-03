# Web Data Source for MS SQL Server

Embeds web scraping capabilities directly into T-SQL queries. This is an MS SQL CLR library with user-defined types (UDTs) and functions. Please find documentation by release at [webdatasource.com](https://webdatasource.com/releases/index.html)

# Installing CLR library to MS SQL Server

The following MS SQL Server versions are supported and tested:
- MS SQL Server 2022 running on a Windows machine (Linux-based versions that can be run in a docker container are not supported CLR functions)

> **NOTE:** This doesn't mean that the other versions are not supported at all. WDS just haven't been tested with them yet, so follow the releases. 

To install CLR functions into an SQL Server instance, on the Releases page choose the [latest](https://github.com/webdatasource/wds.mssql.clr/releases/tag/latest)
1. **WDS.MsSql.Clr.dll** - .NET Framework 4.8 assembly with the CLR functions
2. **WDS.MsSql.Clr.hash** - a hash of the WDS.MsSql.Clr.dll that is required for adding the assembly into an SQL Server instance
3. **WdsClrFunctions.sql** - SQL script that adds the functions into an SQL Server instance
4. **Install.bat** - Windows script that configures and runs the WdsClrFunctions.sql against a particular instance of SQL Server. This script is idempotent so it can be run multiple times and all components will be reinstalled from scratch.

**WdsClrFunctions.sql** and **Install.bat** scripts are available in the [wds.mssql.clr](https://github.com/webdatasource/wds.mssql.clr)

**WDS.MsSql.Clr.dll**, and **WDS.MsSql.Clr.hash** files are built by GitHub actions automatically. Nonetheless, these two files can be compiled from the source code (the WDS.MsSql.Clr.hash is created by an after-build script in the WDS.MsSql.Clr project file).

In order to add CLR functions in an SQL Server instance, the CLR feature should be enabled. To enable it, the WdsClrFunctions.sql script contains the following section:
``` SQL
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'clr enable', 1;
RECONFIGURE;
```

There is always the [latest](https://github.com/webdatasource/wds.mssql.clr/releases/tag/latest)

## Installation Steps
1. Download an Artifacts.zip archive from one of the releases or the latest one
2. Unarchive the Artifacts.zip and get into the unpacked folder
3. Run the Install.bat script and follow instructions
4. After the script completed successfully, the following message will be shown: ```All done. The MS SQL instance on "server_address" is ready to run WDS CLR functions.```

All [CLR functions and their UDT contracts](https://webdatasource.com/releases/latest/mssql/clr-functions/index.html) are installed to the `wds` namespace by default, and all examples use this namespace. This can be changed manually in `WdsClrFunctions.sql` if necessary.

### Install.bat configuration
The following environment variables are used to configure the Install.bat script:

| Environmetn Valiable | Default Value   | Description                        |
| -------------------- | --------------- | ---------------------------------- |
| SERVER               | localhost       | SQL Server instance address        |

#### Install.bat run examples with overrides
1. With a custom SQL Server address
``` BASH
cmd /c "SET SERVER=10.11.12.13 && Install.bat"
```


# ServerStatus

Checks whether the configured server's MSSQL-compatible API dependencies are ready and returns a status row for the check.

## Syntax

```sql
wds.ServerStatus(@serverConfig)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @serverConfig | [wds.ServerConfig](#serverconfig) | Supplies the server URI used to call the MSSQL-compatible readiness endpoint. |

## Returns

`TABLE (Name NVARCHAR(255), Value NVARCHAR(MAX), Description NVARCHAR(MAX))` — a table of status indicators. The `Ready` row reports whether the readiness request succeeded; `Description` contains the response text or the error.

## Data Contracts

### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.



# Start

Starts or restarts the job named in the supplied configuration after applying its crawler settings, then returns its initial download tasks.

## Syntax

```sql
wds.Start(@jobConfig)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @jobConfig | [wds.JobConfig](#jobconfig) | Supplies the server connection, job name, start URLs, and crawler settings. Its job name selects the MSSQL-compatible job to configure and start. |

## Returns

`TABLE (Task wds.DownloadTask)` — [wds.DownloadTask](#downloadtask) values, one for each configured start URL.

## Data Contracts

### JobConfig

Configures a WDS job and its request, crawl, retrieval, and host behavior.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | WDS API server connection parameters. |
| JobType | string | String wrapper for `Type`; excluded from XML serialization. |
| JobName | string | Optional job name; when not specified, a random value is generated. |
| StartUrls | array of string | Initial URLs that start the job. |
| Type | [JobTypes](#jobtypes) | Optional job type. |
| Headers | [HeadersConfig](#headersconfig) | Optional headers settings. |
| Restart | [RestartConfig](#restartconfig) | Optional job restart settings. |
| Https | [HttpsConfig](#httpsconfig) | Optional HTTPS settings. |
| Cookies | [CookiesConfig](#cookiesconfig) | Optional cookies settings. |
| Proxy | [ProxiesConfig](#proxiesconfig) | Optional proxy settings. |
| DownloadErrorHandling | [DownloadErrorHandling](#downloaderrorhandling) | Optional download-error handling settings. |
| CrawlersProtectionBypass | [CrawlersProtectionBypass](#crawlersprotectionbypass) | Optional crawler-protection countermeasure settings. |
| CrossDomainAccess | [CrossDomainAccess](#crossdomainaccess) | Optional cross-domain access settings. |
| Retrieval | [RetrievalConfig](#retrievalconfig) | Optional retrieval-index enrollment settings. |
| Host | [HostConfig](#hostconfig) | Optional host lifecycle settings. |

`AddStartUrl(url)` adds an initial URL. `Parse` accepts the semicolon-separated job configuration; validation requires server and start URLs, then validates configured nested contracts.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.


### JobTypes

Defines the environment in which a job crawls.

| Name | Description |
| --- | --- |
| Internet | Crawls internet sources using request gateways. |
| Intranet | Crawls intranet sources without those limits. |


### HeadersConfig

Configures HTTP headers applied to all job requests.

| Name | Type | Description |
| --- | --- | --- |
| DefaultRequestHeaders | array of [HttpHeader](#httpheader) | Optional headers sent with all requests. |

`AddHeader` adds a header; `AppendHeader` adds a value to a named header. `Parse` accepts the configured header fields.


### HttpHeader

HTTP header.

| Name | Type | Description |
| --- | --- | --- |
| Name | string | Header name. |
| Values | array of string | Header values. |

`Parse` accepts `Name: name; Values: value1, value2;`. Validation requires both fields.


### RestartConfig

Configures how an existing job is restarted.

| Name | Type | Description |
| --- | --- | --- |
| RestartMode | string | SQL-facing string representation of `JobRestartMode`. |
| JobRestartMode | [JobRestartModes](#jobrestartmodes) | Restart mode. |

`Parse` accepts `RestartMode: Continue|FromScratch`.


### JobRestartModes

Defines the behavior of a restarted job.

| Name | Description |
| --- | --- |
| Continue | Reuses cached data and continues crawling and parsing new data. |
| FromScratch | Clears cached data and starts from scratch. |


### HttpsConfig

Configures HTTPS certificate validation for target web resources.

| Name | Type | Description |
| --- | --- | --- |
| SuppressHttpsCertificateValidation | bool | When true, suppresses HTTPS certificate validation. |

`Parse` accepts `SuppressHttpsCertificateValidation: true|false`.


### CookiesConfig

Configures whether the downloader stores response cookies and sends them on later requests.

| Name | Type | Description |
| --- | --- | --- |
| UseCookies | bool | When true, persists and reuses cookies between requests for the job; false is the default. |

`Parse` accepts `UseCookies: true|false`.


### ProxiesConfig

Configures whether requests use proxies and the pool of available proxy endpoints.

| Name | Type | Description |
| --- | --- | --- |
| UseProxy | bool | Enables proxy use for requests. |
| SendOvertRequestsOnProxiesFailure | bool | Enables direct requests when all proxies fail. |
| IterateProxyResponseCodes | string | Optional comma-separated HTTP status codes that cause proxy rotation. |
| Proxies | array of [ProxyConfig](#proxyconfig) | Optional proxy configurations. |

`AddProxyConfig` adds a proxy configuration. `AddProxy` creates and adds one from its protocol, host, port, optional credentials, connection limit, and allowed hosts.


### ProxyConfig

Configures one proxy endpoint for download requests.

| Name | Type | Description |
| --- | --- | --- |
| Protocol | string | Proxy protocol (`http`, `https`, or `socks5`). |
| Host | string | Proxy host. |
| Port | int | Proxy port. |
| UserName | string | Optional proxy username. |
| Password | string | Optional proxy password. |
| ConnectionsLimit | int | Optional maximum concurrent connections through this proxy; null applies no limit. |
| AvailableHosts | array of string | Optional lowercase target hosts (or `host:port`) that may use this proxy; null or empty applies no restriction. |
| RenewalIntervalSec | int | Optional interval in seconds that caps the wait before this proxy can again be selected for a target host. |

`Parse` accepts the semicolon-separated fields shown above. Validation requires `Host` and a nonzero `Port`. `AddAvailableHost(host)` adds a distinct target host.


### DownloadErrorHandling

Configures the policy applied when a download fails.

| Name | Type | Description |
| --- | --- | --- |
| ErrorHandlingPolicy | string | SQL-facing string representation of `Policy`; excluded from XML serialization. |
| Policy | [DownloadErrorHandlingPolicies](#downloaderrorhandlingpolicies) | Error-handling policy. |
| RetryPolicyParams | [RetryPolicyParams](#retrypolicyparams) | Optional retry settings, used when the policy is `Retry`. |

`Parse` accepts `ErrorHandlingPolicy: Skip|Retry`.


### DownloadErrorHandlingPolicies

Defines how the downloader reacts to an error.

| Name | Description |
| --- | --- |
| Skip | Skips an error and continues crawling. |
| Retry | Retries according to the configured retry policy. |


### RetryPolicyParams

Retry settings.

| Name | Type | Description |
| --- | --- | --- |
| RetryDelayMs | int | Delay between retries in milliseconds. |
| RetriesLimit | int | Maximum number of retries. |

`Parse` accepts `RetryDelayMs: 1000; RetriesLimit: 3`.


### CrawlersProtectionBypass

Configures per-download response, redirect, timeout, and per-host pacing limits.

| Name | Type | Description |
| --- | --- | --- |
| MaxResponseSizeKb | int | Optional maximum response content buffered for a download; null uses 1000 KB. |
| MaxRedirectHops | int | Optional maximum redirects; null uses 10 and `0` disables redirects. |
| RequestTimeoutSec | int | Optional request timeout in seconds; null uses 30 seconds. |
| CrawlDelays | array of [CrawlDelay](#crawldelay) | Optional per-host delay overrides; null or empty uses robots.txt delay when available, otherwise no extra delay. |

`AddCrawlDelay` adds or replaces a host delay. `AddDelay` creates a delay from a host and delay string. Validation applies to all configured delays.


### CrawlDelay

Configures request pacing for one target host.

| Name | Type | Description |
| --- | --- | --- |
| Host | string | Lowercase target host, or `host:port` for a non-default port, to which the delay applies. |
| Delay | string | Per-host request wait: `0`, fixed seconds, a range such as `1-5`, or `robots` to use the robots.txt crawl-delay when available. |

`Parse` accepts `Host: host.com; Delay: 0|1-5|robots`. Validation requires both fields and a supported delay format.


### CrossDomainAccess

Configures cross-domain navigation behavior for a job.

| Name | Type | Description |
| --- | --- | --- |
| AccessPolicy | [CrossDomainAccessPolicies](#crossdomainaccesspolicies) | Cross-domain access policy. |

`Parse` accepts `AccessPolicy: None|Subdomains|CrossDomains`. Validation requires a defined access policy.


### CrossDomainAccessPolicies

Defines the permitted navigation scope for a crawl.

| Name | Description |
| --- | --- |
| None | Allows only the original domain. |
| Subdomains | Allows the original domain and its subdomains. |
| CrossDomains | Allows navigation to any domain. |


### RetrievalConfig

Configures optional enrollment of downloaded page content into the retrieval index.

| Name | Type | Description |
| --- | --- | --- |
| EnrollInIndex | bool | When true, converts successfully downloaded page content to Markdown chunks and enrolls it for retrieval. False is the default; cached pages are skipped unless `Force` is true. |
| Force | bool | When true, enrolls cached page content instead of skipping it; it has no effect when `EnrollInIndex` is false. |
| MaxTokensPerChunk | int | Optional maximum tokens per indexed chunk; null uses 512. |
| ContentScopes | array of [RetrievalContentScope](#retrievalcontentscope) | Optional ordered URL-path patterns and selectors; the first matching scope supplies the selector, otherwise the whole page is enrolled. |
| WaitForEnrolled | bool | When true, waits up to one minute per page for snippets to become queryable; false returns after submitting enrollment. |

`AddContentScope` appends a scope; the method has no XML summary in the source. Validation validates every configured scope.


### RetrievalContentScope

Indexed content scope.

| Name | Type | Description |
| --- | --- | --- |
| PathPattern | string | Source XML documentation is missing for this public member. |
| Selector | string | Source XML documentation is missing for this public member. |

Validation requires both fields. The source needs XML summaries for `PathPattern` and `Selector` before their behavior can be documented more specifically.


### HostConfig

Configures host lifecycle settings such as robots.txt and cookie refresh cadence.

| Name | Type | Description |
| --- | --- | --- |
| ReinitPeriodSec | int | Optional host reinitialization interval in seconds; null uses 604800 seconds (7 days). Reinitialization refreshes robots.txt content and renews cookies. |

`Parse` accepts `ReinitPeriodSec: seconds`.


### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.



# Crawl

Extracts links from the supplied task's downloaded page and creates child download tasks, automatically following pending server-side results.

## Syntax

```sql
wds.Crawl(@downloadTask, @selector, @attributeName)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @downloadTask | [wds.DownloadTask](#downloadtask) | Supplies the task ID and server connection. A null task returns no rows; a task with an error is returned unchanged. |
| @selector | string | Selects link-bearing elements. Use `CSS: <selector>`, `XPATH: <selector>`, or `*` for all anchor elements. |
| @attributeName | string | Optional attribute used to obtain a link. Use `val` for inner text; null or empty uses `href`. |

## Returns

`TABLE (Task wds.DownloadTask)` — subsequent [wds.DownloadTask](#downloadtask) values with URLs matched by the selector.

## Data Contracts

### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.



# ScrapeFirst

Extracts the first value matching the selector from the supplied task's downloaded page, automatically following pending server-side results.

## Syntax

```sql
wds.ScrapeFirst(@downloadTask, @selector, @attributeName)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @downloadTask | [wds.DownloadTask](#downloadtask) | Supplies the task ID and server connection. A null task produces no value; a task with an error produces that error as a value. |
| @selector | string | Selects matching elements. Use `CSS: <selector>`, `XPATH: <selector>`, or `*` for the whole page. |
| @attributeName | string | Optional matched-element attribute. Use `val`, null, or empty for inner text. |

## Returns

`NVARCHAR(MAX)` — the first scraped value. An unmatched selector returns `NULL`.

## Data Contracts

### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.



# ScrapeAll

Extracts every value matching the selector from the supplied task's downloaded page, automatically following pending server-side results.

## Syntax

```sql
wds.ScrapeAll(@downloadTask, @selector, @attributeName)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @downloadTask | [wds.DownloadTask](#downloadtask) | Supplies the task ID and server connection. A null task produces no values; a task with an error produces that error as a value. |
| @selector | string | Selects matching elements. Use `CSS: <selector>`, `XPATH: <selector>`, or `*` for the whole page. |
| @attributeName | string | Optional matched-element attribute. Use `val`, null, or empty for inner text. |

## Returns

`TABLE (Data NVARCHAR(MAX))` — one `Data` row for every matched page element.

## Data Contracts

### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.



# ScrapeMultiple

Creates a fluent batch-scrape builder bound to a download task. The server request is sent only when the builder retrieves values.

## Syntax

```sql
wds.ScrapeMultiple(@downloadTask)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @downloadTask | [wds.DownloadTask](#downloadtask) | Supplies the task used by the later batch request. A null task remains unbound and later retrieval produces no values. |

## Returns

[wds.ScrapeMultipleParams](#scrapemultipleparams) — use its `AddScrapeParams`, `GetAll`, and `GetFirst` methods to configure and retrieve the batch result.

See [Scrape Paged](https://webdatasource.com/releases/latest/mssql/examples/scrape-paged.html) for a complete `ScrapeMultiple(...).AddScrapeParams(...).GetFirst(...)` workflow.

## Data Contracts

### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.


### ScrapeMultipleParams

A special object for fluent configuration of a batch scrape request.

| Name | Type | Description |
| --- | --- | --- |

`BindDownloadTask(downloadTask)` binds the parent task. `AddScrapeParams(name, selector, attributeName)` adds an extraction rule. `GetAll(name)` returns [StringDataItems](#stringdataitems), and `GetFirst(name)` returns the first scraped string. `GetFirst(name)` returns `NULL` when no value is available. Validation requires a bound task and validates all configured scrape parameters.


### ScrapeParams

Defines one named field extracted from pages by selector, attribute, and optional conversion.

| Name | Type | Description |
| --- | --- | --- |
| Name | string | Output field name; names must be unique within a scrape request or Traversal level. |
| Selector | string | Selector for matching elements: `CSS: <selector>`, `XPATH: <selector>`, or `*` for the whole page. |
| AttributeName | string | Optional attribute; `val`, null, or empty returns inner text. |
| Convert | string | Optional conversion: `md()` for Markdown or `sr()` for main readable content. |

Validation requires `Name` and `Selector`.


### ScrapeResult

Represents values extracted for one named scrape field.

| Name | Type | Description |
| --- | --- | --- |
| Name | string | Optional field name copied from the matching `ScrapeParams` entry. |
| Values | array of string | Scraped values. |

Validation requires `Name` and `Values`.


### StringDataItems

A special object for the `ToStringsTable` function.

| Name | Type | Description |
| --- | --- | --- |
| DataItems | array of string | Source XML documentation is missing for this public field. |

`AddDataItems(items)` adds data items. `Parse` is unsupported.



# TaskStatus

Reads the supplied task's download state and HTTP attempt history from the MSSQL-compatible API.

## Syntax

```sql
wds.TaskStatus(@downloadTask)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @downloadTask | [wds.DownloadTask](#downloadtask) | Supplies the download task ID and server connection. A null task returns a null status; a task with an error returns that error without calling the server. |

## Returns

[wds.DownloadTaskStatus](#downloadtaskstatus) — a task-status object.

## Data Contracts

### DownloadTask

Represents a download task that can be passed to MSSQL CLR functions.

| Name | Type | Description |
| --- | --- | --- |
| Server | [ServerConfig](#serverconfig) | Server connection associated with the task. |
| Id | string | Download task identifier. |
| Url | string | Download task URL. |

Validation requires `Server`, `Id`, and `Url`.


### ServerConfig

API server configuration.

| Name | Type | Description |
| --- | --- | --- |
| Uri | Uri | API server URI. |

`Parse` accepts `wds://user:password@host:port?https=false`, requires the `wds` scheme, and produces an HTTP URI unless `https=true` is supplied. Validation requires a URI.


### DownloadTaskStatus

Reports the current state and request history for a download task.

| Name | Type | Description |
| --- | --- | --- |
| Url | string | Downloaded URL. |
| TaskState | string | String wrapper for `State`; it is excluded from XML serialization. |
| State | [DownloadTaskStates](#downloadtaskstates) | Task state. |
| Result | [DownloadInfo](#downloadinfo) | Optional final download-attempt details. |
| IntermedResults | array of [DownloadInfo](#downloadinfo) | Optional earlier attempt details, such as redirects or proxy retries. |


### DownloadTaskStates

Defines the lifecycle state reported for a download task.

| Name | Description |
| --- | --- |
| Handled | The downloader finished the task; inspect `Result.IsSuccess` and `Result.HttpStatusCode` when a result is present. |
| AccessDeniedForRobots | Access to the URL is denied by robots.txt. |
| AllRequestGatesExhausted | All request gateways were exhausted without receiving data. |
| Created | Task has not started. |
| InProgress | Task is in progress. |
| Deleted | Task has been deleted. |


### DownloadInfo

Download attempt information.

| Name | Type | Description |
| --- | --- | --- |
| Method | string | HTTP method. |
| Url | string | Request URL. |
| IsSuccess | bool | Indicates whether the request succeeded. |
| HttpStatusCode | int | HTTP status code. |
| ReasonPhrase | string | HTTP reason phrase. |
| RequestHeaders | array of [HttpHeader](#httpheader) | HTTP headers sent with the request. |
| ResponseHeaders | array of [HttpHeader](#httpheader) | HTTP headers received in the response. |
| RequestCookies | array of [Cookie](#cookie) | Cookies sent with the request. |
| ResponseCookies | array of [Cookie](#cookie) | Cookies received in the response. |
| RequestDateUtc | DateTime | Request date and time in UTC. |
| DownloadTimeSec | double | Download time in seconds. |
| ViaProxy | bool | Indicates whether the request was made through a proxy. |
| WaitTimeSec | double | Delay in seconds before the request was executed. |
| CrawlDelaySec | int | Delay in seconds applied to the request. |

Validation requires `Method` and `Url`.


### HttpHeader

HTTP header.

| Name | Type | Description |
| --- | --- | --- |
| Name | string | Header name. |
| Values | array of string | Header values. |

`Parse` accepts `Name: name; Values: value1, value2;`. Validation requires both fields.


### Cookie

Represents an HTTP cookie observed during a download request or response.

| Name | Type | Description |
| --- | --- | --- |
| Name | string | Cookie name. |
| Value | string | Optional cookie value. |
| Domain | string | Optional cookie domain attribute. |
| Path | string | Optional cookie path attribute. |
| HttpOnly | bool | `HttpOnly` cookie attribute. |
| Secure | bool | `Secure` cookie attribute. |
| Expires | DateTime | Optional expiration date and time; null for a session cookie or an unknown expiration. |

Validation requires `Name`.



# ToStringsTable

Projects supplied string items into one table row per value for use with `ScrapeMultipleParams.GetAll`.

## Syntax

```sql
wds.ToStringsTable(@items)
```

## Arguments

| Name | Type | Description |
| --- | --- | --- |
| @items | [wds.StringDataItems](#stringdataitems) | Supplies the string items to project into table rows. |

## Returns

`TABLE (Data NVARCHAR(MAX))` — the input-items table.

## Data Contracts

### StringDataItems

A special object for the `ToStringsTable` function.

| Name | Type | Description |
| --- | --- | --- |
| DataItems | array of string | Source XML documentation is missing for this public field. |

`AddDataItems(items)` adds data items. `Parse` is unsupported.



# Scrape Paged

> **_Minimal playground version:_** v1.0.0

Demonstrates iterating through paginated category pages to visit item pages and scrape fields; includes a ScrapeMultiple variant for fewer API calls.

``` SQL 
DECLARE @jobConfig wds.JobConfig = 'JobName: CrawlAllProducts; Server: wds://localhost:2807; StartUrls: http://playground';
DECLARE @pages TABLE (Task wds.DownloadTask);

-- Gathering categories' first pages
INSERT INTO @pages (Task)
SELECT nav.Task
FROM wds.Start(@jobConfig) root
    OUTER APPLY wds.Crawl(root.Task, 'css: ul.nav a:not([href="/"])', null) nav

-- Gathering the other pages of categories
WHILE @@ROWCOUNT > 0
BEGIN
    INSERT INTO @pages (Task)
    SELECT newPages.Task
    FROM @pages curPages
        CROSS APPLY wds.Crawl(curPages.Task, 'css: ul.pagination li:not(.disabled) a', null) newPages
    WHERE NOT EXISTS (SELECT NULL FROM @pages p WHERE p.Task = newPages.Task)
END

-- Iterating through pages, visiting product pages, and scraping data from them
SELECT 
    products.Task.Url ProductUrl,
    wds.ScrapeFirst(products.Task, 'css: h1', null) AS ProductName,
    wds.ScrapeFirst(products.Task, 'css: .price span', null) AS ProductPrice
FROM @pages pages
    CROSS APPLY wds.Crawl(pages.Task, 'css: .table a', null) products
```

Iterate over all pages and scrape data using the [ScrapeMultiple](https://webdatasource.com/releases/latest/mssql/clr-functions/scrape-multiple.html) function. This approach is a bit faster because fewer requests to the WDS API Server are required.

``` SQL
DECLARE @jobConfig wds.JobConfig = 'JobName: CrawlAllProductsBatch; Server: wds://localhost:2807; StartUrls: http://playground';
DECLARE @pages TABLE (Task wds.DownloadTask);

-- Gathering categories' first pages
INSERT INTO @pages
    (Task)
SELECT nav.Task
FROM wds.Start(@jobConfig) root
    OUTER APPLY wds.Crawl(root.Task, 'css: ul.nav a:not([href="/"])', null) nav

-- Gathering the other pages of categories
WHILE @@ROWCOUNT > 0
BEGIN
    INSERT INTO @pages
        (Task)
    SELECT newPages.Task
    FROM @pages curPages
        CROSS APPLY wds.Crawl(curPages.Task, 'css: ul.pagination li:not(.disabled) a', null) newPages
    WHERE NOT EXISTS (SELECT NULL FROM @pages p WHERE p.Task = newPages.Task)
END

-- Iterating through pages, visiting product pages, and scraping data from them
SELECT
    products.Task.Url ProductUrl,
    product.ScrapeResult.GetFirst('ProductName') AS ProductName,
    product.ScrapeResult.GetFirst('ProductPrice') AS ProductPrice
FROM @pages pages
    CROSS APPLY wds.Crawl(pages.Task, 'css: .table a', null) products
    CROSS APPLY (
        SELECT wds.ScrapeMultiple(products.Task)
                .AddScrapeParams('ProductName', 'css: h1', null)
                .AddScrapeParams('ProductPrice', 'css: .price span', null) AS ScrapeResult
    ) product
```


# Scrape Sitemap

> **_Minimal playground version:_** v1.0.1

Shows how to collect product pages from sitemap.xml and scrape fields from each page; includes a ScrapeMultiple variant to minimize round trips.

``` SQL
DECLARE @jobConfig wds.JobConfig = 'JobName: CrawlAllProductsSitemap; Server: wds://localhost:2807; StartUrls: http://playground/sitemap.xml';

SELECT 
    products.Task.Url ProductUrl,
    wds.ScrapeFirst(products.Task, 'css: h1', null) AS ProductName,
    wds.ScrapeFirst(products.Task, 'css: .price span', null) AS ProductPrice
FROM wds.Start(@jobConfig) root
    OUTER APPLY wds.Crawl(root.Task, 'xpath: //*[local-name()="url"]/*[local-name()="loc"]', 'val') products
```

Getting product pages from a sitemap.xml and scrape data from these pages using the [ScrapeMultiple](https://webdatasource.com/releases/latest/mssql/clr-functions/scrape-multiple.html) function. This approach is a bit faster because fewer requests to the WDS API Server are required.

``` SQL
DECLARE @jobConfig wds.JobConfig = 'JobName: CrawlAllProductsSitemap; Server: wds://localhost:2807; StartUrls: http://playground/sitemap.xml';

SELECT
    products.Task.Url ProductUrl,
    product.ScrapeResult.GetFirst('ProductName') AS ProductName,
    product.ScrapeResult.GetFirst('ProductPrice') AS ProductPrice
FROM wds.Start(@jobConfig) root
    OUTER APPLY wds.Crawl(root.Task, 'xpath: //*[local-name()="url"]/*[local-name()="loc"]', 'val') products
    CROSS APPLY (
        SELECT wds.ScrapeMultiple(products.Task)
                .AddScrapeParams('ProductName', 'css: h1', null)
                .AddScrapeParams('ProductPrice', 'css: .price span', null) AS ScrapeResult
    ) product
```
