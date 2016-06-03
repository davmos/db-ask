namespace DbAsk
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    internal class DataAccess
    {
        internal static string SearchTables(string namePart)
        {
            var search = new Search(namePart);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommandTableSearch(connection, search))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    lines.Add(
                        new[]
                        {
                            reader["table_name"].ToString()
                        });
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        internal static string SearchColumns(string namePart)
        {
            var search = new Search(namePart);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommandColumnSearch(connection, search))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    lines.AddRange(
                        new[]
                        {
                            new[] { "Table", "Column", "Nullable", "DataType", "Default" },
                            new[] { "*****", "******", "********", "********", "*******" }
                        });
                }

                while (reader.Read())
                {
                    lines.Add(
                        new[]
                        {
                            reader["table_name"].ToString(),
                            reader["column_name"].ToString(),
                            FormatIsNullable(reader),
                            FormatDataType(reader),
                            reader["column_default"].ToString()
                        });
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        private static string FormatDataType(SqlDataReader reader)
        {
            string dataType = reader["data_type"].ToString();

            string characterMaxLength = reader["character_maximum_length"].ToString();

            if (!string.IsNullOrWhiteSpace(characterMaxLength))
            {
                return $"{dataType}({characterMaxLength})";
            }

            string numericPrecision = reader["numeric_precision"].ToString();
            string numericPrecisionRadix = reader["numeric_precision_radix"].ToString();

            if (!string.IsNullOrWhiteSpace(numericPrecision))
            {
                return $"{dataType}({numericPrecision}, {numericPrecisionRadix})";
            }

            return dataType;
        }

        internal static string DescribeTable(string tableName)
        {
            var search = new Search(tableName);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommand(connection, search))
            using (var reader = command.ExecuteReader())
            {
                // TODO: primary key info
                if (reader.HasRows)
                {
                    lines.AddRange(
                        new[]
                        {
                            new[] { "Column", "Nullable", "DataType", "Default" },
                            new[] { "******", "********", "********", "*******" }
                        });
                }

                while (reader.Read())
                {
                    lines.Add(
                        new[]
                        {
                            reader["column_name"].ToString(),
                            FormatIsNullable(reader),
                            FormatDataType(reader),
                            reader["column_default"].ToString()
                        });
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        private static string FormatIsNullable(IDataRecord reader)
        {
            return reader["is_nullable"].ToString().Equals("NO", StringComparison.OrdinalIgnoreCase)
                ? "NOT NULL"
                : string.Empty;
        }

        private static SqlCommand NewSqlCommand(SqlConnection connection, Search search)
        {
            const string Sql
                = @"SELECT 
                        column_name, 
                        is_nullable,
                        data_type,
                        column_default,
                        character_maximum_length,
                        numeric_precision,
                        numeric_precision_radix 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE table_name = @tableName
                    AND table_schema = @schemaName
                    ORDER BY ordinal_position";

            var command = new SqlCommand(Sql, connection);
            command.Parameters.AddWithValue("tableName", search.Table);
            command.Parameters.AddWithValue("schemaName", search.Schema);
            return command;
        }

        private static SqlCommand NewSqlCommandColumnSearch(SqlConnection connection, Search search)
        {
            const string Sql
                = @"SELECT 
                        table_name,
                        column_name, 
                        is_nullable,
                        data_type,
                        column_default,
                        character_maximum_length,
                        numeric_precision,
                        numeric_precision_radix  
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE column_name LIKE @nameContains
                    AND table_schema = @schemaName";

            var command = new SqlCommand(Sql, connection);
            command.Parameters.AddWithValue("nameContains", "%" + search.Table + "%");
            command.Parameters.AddWithValue("schemaName", search.Schema);
            return command;
        }

        private static SqlCommand NewSqlCommandTableSearch(
            SqlConnection connection, 
            Search search)
        {
            const string Sql
                = @"SELECT 
                        table_name 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE table_name LIKE @nameContains
                    AND table_schema = @schema";

            var command = new SqlCommand(Sql, connection);
            command.Parameters.AddWithValue("nameContains", "%" + search.Table + "%");
            command.Parameters.AddWithValue("schema", search.Schema);
            return command;
        }

        private static SqlConnection NewSqlConnection(Search search)
        {
            var connection = new SqlConnection(
                $@"data source={search.Host};initial catalog={search.Catalog};integrated Security=SSPI");

            connection.Open();

            return connection;
        }
    }
}
