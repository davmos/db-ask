﻿namespace DbAsk
{
    using static System.Console;

    internal class Program
    {
        private static void Main(string[] args)
        {
            SetWindow();

            if (args == null || args.Length < 1)
            {
                WriteLine("usage: db [<server_name>.][<database_name>.][<schema_name>.]<table_name>");
                return;
            }

            WriteLine(GetOutput(args) ?? "Nothing found.");

            ///*
            if (System.Diagnostics.Debugger.IsAttached)
            {
                WriteLine("\n[Press any key to exit]");
                ReadKey();
            }
            //*/
        }

        private static void SetWindow()
        {
            ConsoleWindow.AlmostMaximise();

            ConsoleWindow.EnableQuickEditMode();

            //var fonts = ConsoleWindow.ConsoleFonts;

            ConsoleWindow.SetConsoleFont(9);
        }

        private static string GetOutput(string[] args)
        {
            switch (args[0].ToUpper())
            {
                case "C":
                    return SearchColumns(args[1]);
                case "T":
                    return SearchTables(args[1]);
                default:
                    return DescribeTable(args[0]);
            }
        }

        private static string SearchColumns(string arg)
        {
            var search = new Search(arg, Search.Type.ColumnSearch);
            WriteLine($"Server={search.Host} Catalog={search.Catalog} Schema={search.Schema}");
            WriteLine($"Searching for columns with name containing '{search.Table}'...\n");
            return DataAccess.SearchColumns(arg);
        }

        private static string SearchTables(string arg)
        {
            var search = new Search(arg, Search.Type.TableSearch);
            WriteLine($"Server={search.Host} Catalog={search.Catalog} Schema={search.Schema}");
            WriteLine($"Searching for tables with name containing '{search.Table}'...\n");
            return DataAccess.SearchTables(arg);
        }

        private static string DescribeTable(string arg)
        {
            var search = new Search(arg, Search.Type.TableDescription);
            WriteLine($"Server={search.Host} Catalog={search.Catalog} Schema={search.Schema}");
            WriteLine($"Getting table definition of '{search.Table}'...\n");
            return DataAccess.DescribeTable(arg);
        }
    }
}