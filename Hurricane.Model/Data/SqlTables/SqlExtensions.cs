using System;
using System.Data.Common;
using System.Data.SQLite;

namespace Hurricane.Model.Data.SqlTables
{
    static class SqlExtensions
    {
        public static void AddGuid(this SQLiteParameterCollection parmeters, string parameterName, Guid? guid)
        {
            parmeters.AddWithValue(parameterName, guid?.ToString("D"));
        }

        public static Guid ReadGuid(this DbDataReader reader, int ordinal)
        {
            var line = reader.GetValue(ordinal)?.ToString();
            if(string.IsNullOrWhiteSpace(line))
                return Guid.Empty;

            return Guid.ParseExact(line, "D");
        }
    }
}
