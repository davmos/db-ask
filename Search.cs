namespace DbAsk
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;

    internal class Search
    {
        internal enum Type
        {
            TableDescription,
            TableSearch,
            ColumnSearch
        }

        internal Search(string arg, Type type)
        {
            if (arg == null)
            {
                return;
            }

            this.SearchType = type;

            var parts = arg.Split('.');

            if (parts.Length > 3)
            {
                this.Host = parts[0];
                this.Catalog = parts[1];
                this.Schema = parts[2];
                this.Table = parts[3];
                return;
            }

            switch (parts.Length)
            {
                case 3:
                    this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                    this.Catalog = parts[0];
                    this.Schema = parts[1];
                    this.Table = parts[2];
                    return;
                case 2:
                    this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                    this.Catalog = ConfigurationManager.AppSettings["DefaultCatalog"];
                    this.Schema = parts[0];
                    this.Table = parts[1];
                    return;
                case 1:
                    this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                    this.Catalog = ConfigurationManager.AppSettings["DefaultCatalog"];
                    this.Schema = ConfigurationManager.AppSettings["DefaultSchema"];
                    this.Table = parts[0];
                    break;
            }
        }

        internal string Host { get; set; }
        internal string Catalog { get; set; }
        internal string Schema { get; set; }
        internal string Table { get; set; }

        internal Type SearchType { get; }

        internal bool IsAllSchemas 
            => string.IsNullOrWhiteSpace(this.Schema) || this.Schema == "*";

        internal string[][] GetColumnHeaders()
        {
            var columnHeaders = new List<string>();

            if (this.IsAllSchemas)
            {
                columnHeaders.Add("Schema");
            }

            switch (this.SearchType)
            {
                case Type.TableDescription:
                    columnHeaders.Add("Column");
                    columnHeaders.Add("Nullable");
                    columnHeaders.Add("DataType");
                    columnHeaders.Add("Default");
                    columnHeaders.Add("PK");
                    break;

                case Type.TableSearch:
                    columnHeaders.Add("Table");
                    break;

                case Type.ColumnSearch:
                    columnHeaders.Add("Table");
                    columnHeaders.Add("Column");
                    columnHeaders.Add("Nullable");
                    columnHeaders.Add("DataType");
                    columnHeaders.Add("Default");
                    break;

                default:
                    throw new Exception("Unknown search type: " + this.SearchType);
            }

            return new[]
            {
                columnHeaders.ToArray(),
                columnHeaders.Select(h => new string('*', h.Length)).ToArray()
            };
        }

        internal string[] GetColumnValues(IDataRecord reader)
        {
            switch (this.SearchType)
            {
                case Type.TableDescription:

                    if (this.IsAllSchemas)
                    {
                        return new[]
                        {
                            reader["table_schema"].ToString(),
                            reader["column_name"].ToString(),
                            FormatIsNullable(reader),
                            FormatDataType(reader),
                            reader["column_default"].ToString(),
                            FormatIsPrimaryKey(reader)
                        };
                    }

                    return new[]
                    {
                        reader["column_name"].ToString(),
                        FormatIsNullable(reader),
                        FormatDataType(reader),
                        reader["column_default"].ToString(),
                        FormatIsPrimaryKey(reader)
                    };

                case Type.TableSearch:

                    if (this.IsAllSchemas)
                    {
                        return new[]
                        {
                            reader["table_schema"].ToString(),
                            reader["table_name"].ToString()
                        };
                    }

                    return new[]
                    {
                        reader["table_name"].ToString()
                    };

                case Type.ColumnSearch:

                    if (this.IsAllSchemas)
                    {
                        return new[]
                        {
                            reader["table_schema"].ToString(),
                            reader["table_name"].ToString(),
                            reader["column_name"].ToString(),
                            FormatIsNullable(reader),
                            FormatDataType(reader),
                            reader["column_default"].ToString()
                        };
                    }

                    return new[]
                    {
                        reader["table_name"].ToString(),
                        reader["column_name"].ToString(),
                        FormatIsNullable(reader),
                        FormatDataType(reader),
                        reader["column_default"].ToString()
                    };

                default:
                    throw new Exception("Unknown search type: " + this.SearchType);
            }
        }

        private static string FormatIsNullable(IDataRecord reader)
        {
            return reader["is_nullable"].ToString().Equals("NO", StringComparison.OrdinalIgnoreCase)
                ? "NOT NULL"
                : string.Empty;
        }

        private static string FormatIsPrimaryKey(IDataRecord reader)
        {
            return (int)reader["is_primary_key"] == 1 ? "PK" : string.Empty;
        }

        private static string FormatDataType(IDataRecord reader)
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
    }
}