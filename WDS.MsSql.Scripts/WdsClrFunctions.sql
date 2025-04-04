-- ENABLING CLR FUNCTIONS
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'clr enable', 1;
RECONFIGURE;

-- UPSERTING SCHEMA
DECLARE @SchemaName NVARCHAR(128) = 'wds';
IF NOT EXISTS (SELECT *
               FROM sys.schemas
               WHERE name = @SchemaName)
    BEGIN
        EXEC ('CREATE SCHEMA ' + @SchemaName);
    END

-- CLEANING UP
DECLARE @AssemblyName NVARCHAR(128) = 'WdsSqlClrFunctions';
DECLARE @sql NVARCHAR(MAX) = '';
-- Drop functions
SELECT
    @sql += 'DROP ' + CASE WHEN o.type = 'PC' THEN 'PROCEDURE ' ELSE 'FUNCTION ' END + @SchemaName + '.' + o.Name + '; '
FROM sys.assemblies asm
         INNER JOIN sys.ASSEMBLY_MODULES m ON asm.assembly_id = m.assembly_id
         INNER JOIN sys.OBJECTS o ON m.object_id = o.object_id
WHERE asm.name = @AssemblyName
-- Drop types
SELECT @sql += 'DROP TYPE ' + @SchemaName + '.' + t.Name + '; '
FROM sys.assembly_types t
         INNER JOIN sys.assemblies asm on asm.assembly_id = t.assembly_id
WHERE asm.name = @AssemblyName
-- Drop the assembly
SET @sql += 'DROP ASSEMBLY IF EXISTS ' + @AssemblyName
EXEC (@sql);

-- ADDING THE ASSEMBLY TO TRUSTED ASSEMBLIES
DECLARE @BasePath NVARCHAR(255) = N'$(BasePath)';
DECLARE @HashFilePath NVARCHAR(500) = @BasePath + '\WDS.MsSql.Clr.hash';
DECLARE @DllFilePath NVARCHAR(500) = @BasePath + '\WDS.MsSql.Clr.dll';
SET @sql = N'
DECLARE @AssemblyHash VARBINARY(64) = (SELECT * FROM OPENROWSET(BULK ''' + @HashFilePath + N''', SINGLE_BLOB) AS BinaryFile)
IF EXISTS (SELECT 1 FROM sys.trusted_assemblies WHERE hash = @AssemblyHash)
BEGIN
    EXEC sp_drop_trusted_assembly @Hash = @AssemblyHash;
END
EXEC sp_add_trusted_assembly @Hash = @AssemblyHash;';
EXEC sp_executesql @sql;

SET @sql = N'
CREATE ASSEMBLY WdsSqlClrFunctions
    FROM ''' + @DllFilePath + N'''
    WITH PERMISSION_SET = EXTERNAL_ACCESS;';
EXEC sp_executesql @sql;

-- ADDING UDTs
CREATE TYPE wds.HttpHeader
    EXTERNAL NAME WdsSqlClrFunctions.HttpHeader;

CREATE TYPE wds.ProxyConfig
    EXTERNAL NAME WdsSqlClrFunctions.ProxyConfig;

CREATE TYPE wds.ProxiesConfig
    EXTERNAL NAME WdsSqlClrFunctions.ProxiesConfig;

CREATE TYPE wds.RetryPolicyParams
    EXTERNAL NAME WdsSqlClrFunctions.RetryPolicyParams;

CREATE TYPE wds.DownloadErrorHandlingPolicy
    EXTERNAL NAME WdsSqlClrFunctions.DownloadErrorHandlingPolicy;

CREATE TYPE wds.CrawlDelay
    EXTERNAL NAME WdsSqlClrFunctions.CrawlDelay;

CREATE TYPE wds.CrawlersProtectionBypass
    EXTERNAL NAME WdsSqlClrFunctions.CrawlersProtectionBypass;

CREATE TYPE wds.HeadersConfig
    EXTERNAL NAME WdsSqlClrFunctions.HeadersConfig;

CREATE TYPE wds.JobRestartConfig
    EXTERNAL NAME WdsSqlClrFunctions.JobRestartConfig;

CREATE TYPE wds.HttpsConfig
    EXTERNAL NAME WdsSqlClrFunctions.HttpsConfig;

CREATE TYPE wds.CookiesConfig
    EXTERNAL NAME WdsSqlClrFunctions.CookiesConfig;

CREATE TYPE wds.ServerConfig
    EXTERNAL NAME WdsSqlClrFunctions.ServerConfig;

CREATE TYPE wds.JobConfig
    EXTERNAL NAME WdsSqlClrFunctions.JobConfig;
GO

CREATE TYPE wds.DownloadTask
    EXTERNAL NAME WdsSqlClrFunctions.DownloadTask;
GO

CREATE TYPE wds.Cookie
    EXTERNAL NAME WdsSqlClrFunctions.Cookie;
GO

CREATE TYPE wds.DownloadInfo
    EXTERNAL NAME WdsSqlClrFunctions.DownloadInfo;
GO

CREATE TYPE wds.DownloadTaskStatus
    EXTERNAL NAME WdsSqlClrFunctions.DownloadTaskStatus;
GO

CREATE TYPE wds.ScrapeMultipleParams
    EXTERNAL NAME WdsSqlClrFunctions.ScrapeMultipleParams;
GO

CREATE TYPE wds.StringDataItems
    EXTERNAL NAME WdsSqlClrFunctions.StringDataItems;
GO

-- ADDING FUNCTIONS
CREATE FUNCTION wds.ServerStatus(@serverConfig wds.ServerConfig)
    RETURNS TABLE
            (
                Name        NVARCHAR(255),
                Value       NVARCHAR(MAX),
                Description NVARCHAR(MAX)
            )
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.ServerStatus;
GO

CREATE FUNCTION wds.TaskStatus(@downloadTask wds.DownloadTask)
    RETURNS wds.DownloadTaskStatus
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.TaskStatus;
GO

CREATE FUNCTION wds.Start(@jobConfig wds.JobConfig)
    RETURNS TABLE
            (
                Task wds.DownloadTask
            )
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.Start;
GO

CREATE FUNCTION wds.Crawl(@downloadTask wds.DownloadTask, @selector NVARCHAR(MAX), @attributeName NVARCHAR(MAX))
    RETURNS TABLE
            (
                Task wds.DownloadTask
            )
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.Crawl;
GO

CREATE FUNCTION wds.ScrapeAll(@downloadTask wds.DownloadTask, @selector NVARCHAR(MAX), @attributeName NVARCHAR(MAX))
    RETURNS TABLE
            (
                Data NVARCHAR(MAX)
            )
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.ScrapeAll;
GO

CREATE FUNCTION wds.ScrapeFirst(@downloadTask wds.DownloadTask, @selector NVARCHAR(MAX), @attributeName NVARCHAR(MAX))
    RETURNS NVARCHAR(MAX)
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.ScrapeFirst;
GO

CREATE FUNCTION wds.ScrapeMultiple(@downloadTask wds.DownloadTask)
    RETURNS wds.ScrapeMultipleParams
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.ScrapeMultiple;
GO

CREATE FUNCTION wds.ToStringsTable(@items wds.StringDataItems)
    RETURNS TABLE
            (
                Data NVARCHAR(MAX)
            )
    EXTERNAL NAME WdsSqlClrFunctions.ClrFunctions.ToStringsTable;
GO