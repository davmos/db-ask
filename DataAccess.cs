namespace DbAsk
{
    using System.Collections.Generic;
    using System.Data.SqlClient;

    internal class DataAccess
    {
        internal static string SearchTables(string namePart)
        {
            var search = new Search(namePart, Search.Type.TableSearch);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommandTableSearch(connection, search))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    lines.AddRange(search.GetColumnHeaders());
                }

                while (reader.Read())
                {
                    lines.Add(search.GetColumnValues(reader));
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        internal static string SearchColumns(string namePart)
        {
            var search = new Search(namePart, Search.Type.ColumnSearch);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommandColumnSearch(connection, search))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    lines.AddRange(search.GetColumnHeaders());
                }

                while (reader.Read())
                {
                    lines.Add(search.GetColumnValues(reader));
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        internal static string DescribeTable(string tableName)
        {
            var search = new Search(tableName, Search.Type.TableDescription);

            var lines = new List<string[]>();

            using (var connection = NewSqlConnection(search))
            using (var command = NewSqlCommand(connection, search))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    lines.AddRange(search.GetColumnHeaders());
                }

                while (reader.Read())
                {
                    lines.Add(search.GetColumnValues(reader));
                }
            }

            return Formatter.PadElementsInLines(lines);
        }

        private static SqlCommand NewSqlCommand(SqlConnection connection, Search search)
        {
            string sql
                = @";WITH PK_COLUMNS AS
                    (
	                    SELECT tc.table_schema, tc.table_name, ccu.column_name
	                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
	                    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu 
                            ON tc.constraint_name = ccu.constraint_name
	                    WHERE tc.constraint_type = 'Primary Key'
                    )
                    SELECT
                        c.table_schema, 
                        c.column_name, 
                        c.is_nullable,
                        c.data_type,
                        c.column_default,
                        c.character_maximum_length,
                        c.numeric_precision,
                        c.numeric_precision_radix,
	                    IIF(pkc.column_name IS NULL, 0, 1) AS is_primary_key 
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    LEFT JOIN PK_COLUMNS pkc ON pkc.column_name = c.column_name
						                    AND pkc.table_schema = c.table_schema
						                    AND pkc.table_name = c.table_name 
                    WHERE c.table_name = @tableName ";

            if (!search.IsAllSchemas)
            {
                sql += "AND c.table_schema = @schemaName ";
            }

            sql += "ORDER BY c.table_schema, c.ordinal_position";

            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("tableName", search.Table);

            if (!search.IsAllSchemas)
            {
                command.Parameters.AddWithValue("schemaName", search.Schema);
            }

            return command;
        }

        private static SqlCommand NewSqlCommandColumnSearch(SqlConnection connection, Search search)
        {
            string sql
                = @"SELECT
                        table_schema, 
                        table_name,
                        column_name, 
                        is_nullable,
                        data_type,
                        column_default,
                        character_maximum_length,
                        numeric_precision,
                        numeric_precision_radix  
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE column_name LIKE @nameContains ";

            if (!search.IsAllSchemas)
            {
                sql += "AND table_schema = @schemaName ";
            }

            sql += "ORDER BY table_schema";

            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("nameContains", "%" + search.Table + "%");

            if (!search.IsAllSchemas)
            {
                command.Parameters.AddWithValue("schemaName", search.Schema);
            }

            return command;
        }

        private static SqlCommand NewSqlCommandTableSearch(
            SqlConnection connection, 
            Search search)
        {
            string sql
                = @"SELECT
                        table_schema, 
                        table_name 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE table_name LIKE @nameContains ";

            if (!search.IsAllSchemas)
            {
                sql += "AND table_schema = @schema ";
            }

            sql += "ORDER BY table_schema";

            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("nameContains", "%" + search.Table + "%");

            if (!search.IsAllSchemas)
            {
                command.Parameters.AddWithValue("schema", search.Schema);
            }

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
