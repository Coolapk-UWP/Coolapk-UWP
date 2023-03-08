// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation.Languages
{
    public class Sql : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Sql; }
        }

        public string Name
        {
            get { return "SQL"; }
        }

        public string CssClassName
        {
            get { return "sql"; }
        }

        public string FirstLinePattern
        {
            get
            {
                return null;
            }
        }

        public IList<LanguageRule> Rules
        {
            get
            {
                return new List<LanguageRule>
                           {
                               new LanguageRule(
                                   @"(?s)/\*.*?\*/",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"(--.*?)\r?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"'(?>[^\']*)'",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"""(?>[^\""]*)""",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"\[(?>[^\]]*)]",
                                   new Dictionary<int, string>
                                       {
                                           { 0, null },
                                       }),
                               new LanguageRule(
                                   @"(?i)\b(?<![.@""])(ADA|ADD|AFTER|ALL|ALTER|AND|ANY|AS|ASC|AT|AUTHORIZATION|AUTO|BACKUP|BEGIN|BETWEEN|BINARY|BIT|BIT_LENGTH|BREAK|BROWSE|BULK|BY|CASCADE|CASE|CHAR|CHARACTER|CHARACTER_LENGTH|CHAR_LENGTH|CHECK|CHECKPOINT|CHECKSUM_AGG|CLOSE|CLUSTERED|COLLATE|COL_LENGTH|COL_NAME|COLUMN|COLUMNPROPERTY|COMMIT|COMPUTE|CONNECT|CONNECTION|CONSTRAINT|CONTAINS|CONTAINSTABLE|CONTINUE|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DATABASEPROPERTY|DATE|DATETIME|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRED|DELETE|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DROP|DUMMY|DUMP|ELSE|ENCRYPTION|END-EXEC|END|ERRLVL|ESCAPE|EXCEPT|EXEC|EXECUTE|EXISTS|EXIT|EXTERNAL|EXTRACT|FALSE|FETCH|FILE|FILE_ID|FILE_NAME|FILEGROUP_ID|FILEGROUP_NAME|FILEGROUPPROPERTY|FILEPROPERTY|FILLFACTOR|FIRST|FLOAT|FOR|FOREIGN|FORTRAN|FREE|FREETEXT|FREETEXTTABLE|FROM|FULL|FULLTEXTCATALOGPROPERTY|FULLTEXTSERVICEPROPERTY|FUNCTION|GLOBAL|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|HOUR|IDENTITY|IDENTITYCOL|IDENTITY_INSERT|IF|IGNORE|IMAGE|IMMEDIATE|IN|INCLUDE|INDEX|INDEX_COL|INDEXKEY_PROPERTY|INDEXPROPERTY|INNER|INSENSITIVE|INSERT|INSTEAD|INT|INTEGER|INTERSECT|INTO|IS|ISOLATION|JOIN|KEY|KILL|LANGUAGE|LAST|LEFT|LEVEL|LIKE|LINENO|LOAD|LOCAL|MINUTE|MODIFY|MONEY|NAME|NATIONAL|NCHAR|NEXT|NOCHECK|NONCLUSTERED|NOCOUNT|NONE|NOT|NULL|NUMERIC|NVARCHAR|OBJECT_ID|OBJECT_NAME|OBJECTPROPERTY|OCTET_LENGTH|OF|OFF|OFFSETS|ON|ONLY|OPEN|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|OPTION|OR|ORDER|OUTER|OUTPUT|OVER|OVERLAPS|PARTIAL|PASCAL|PERCENT|PLAN|POSITION|PRECISION|PREPARE|PRIMARY|PRINT|PRIOR|PRIVILEGES|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|REAL|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|RETURNS|REVERT|REVOKE|RIGHT|ROLLBACK|ROLLUP|ROWCOUNT|ROWGUIDCOL|ROWS|RULE|SAVE|SCHEMA|SCROLL|SECOND|SECTION|SELECT|SEQUENCE|SET|SETUSER|SHUTDOWN|SIZE|SMALLINT|SMALLMONEY|SOME|SQLCA|SQLERROR|SQUARE|STATISTICS|TABLE|TEMPORARY|TEXT|TEXTPTR|TEXTSIZE|TEXTVALID|THEN|TIME|TIMESTAMP|TO|TOP|TRAN|TRANSACTION|TRANSLATION|TRIGGER|TRUE|TRUNCATE|TSEQUAL|TYPEPROPERTY|UNION|UNIQUE|UPDATE|UPDATETEXT|USE|VALUES|VARCHAR|VARYING|VIEW|WAITFOR|WHEN|WHERE|WHILE|WITH|WORK|WRITETEXT|IS_MEMBER|IS_SRVROLEMEMBER|PERMISSIONS|SUSER_SID|SUSER_SNAME|SYSNAME|UNIQUEIDENTIFIER|USER_ID|VARBINARY|ABSOLUTE|DATEPART|DATEDIFF|WEEK|WEEKDAY|MILLISECOND|GETUTCDATE|DATENAME|DATEADD|BIGINT|TINYINT|SMALLDATETIME|NTEXT|XML|ASSEMBLY|AGGREGATE|TYPE)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                               new LanguageRule(
                                   @"(?i)\b(?<![.@""])(ABS|ACOS|APP_NAME|ASCII|ASIN|ATAN|ATN2|AVG|BINARY_CHECKSUM|CAST|CEILING|CHARINDEX|CHECKSUM|CONVERT|COALESCE|COLLATIONPROPERTY|COLUMNS_UPDATED|COUNT|COS|COT|COUNT_BIG|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR_STATUS|DATALENGTH|DAY|DB_ID|DB_NAME|DEGREES|DIFFERENCE|ERROR_LINE|ERROR_MESSAGE|ERROR_NUMBER|ERROR_PROCEDURE|ERROR_SEVERITY|ERROR_STATE|EXP|FLOOR|FORMATMESSAGE|GETANSINULL|GETDATE|GROUPING|HOST_ID|HOST_NAME|IDENT_CURRENT|IDENT_INCR|IDENT_SEED|ISDATE|ISNULL|ISNUMERIC|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NEWID|NULLIF|PARSENAME|PATINDEX|PI|POWER|ORIGINAL_LOGIN|QUOTENAME|UNICODE|ROW_NUMBER|RADIANS|RAND|ROUND|RTRIM|REPLACE|REPLICATE|REVERSE|SCOPE_IDENTITY|SOUNDEX|STR|STUFF|SERVERPROPERTY|SESSIONPROPERTY|SESSION_USER|SIGN|SIN|SPACE|STATS_DATE|STDEV|STDEVP|SQRT|SUBSTRING|SUM|SUSER_NAME|SQL_VARIANT|SQL_VARIANT_PROPERTY|SYSTEM_USER|TAN|UPPER|USER|USER_NAME|VAR|VARP|XACT_STATE|YEAR)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.SqlSystemFunction }
                                       }
                                   ),
                               new LanguageRule(
                                   @"(?i)\B(@@(?:ERROR|IDENTITY|ROWCOUNT|TRANCOUNT|FETCH_STATUS))\b",
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.SqlSystemFunction}
                                       }
                                   ),
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}