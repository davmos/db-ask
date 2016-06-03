namespace DbAsk
{
    using static System.Console;

    class Program
    {
        static void Main(string[] args)
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
            var search = new Search(arg);
            WriteLine("Server={0} Catalog={1} Schema={2}", search.Host, search.Catalog, search.Schema);
            WriteLine("Searching for columns with name containing '{0}'...\n", search.Table);
            return DataAccess.SearchColumns(arg);
        }

        private static string SearchTables(string arg)
        {
            var search = new Search(arg);
            WriteLine("Server={0} Catalog={1} Schema={2}", search.Host, search.Catalog, search.Schema);
            WriteLine("Searching for tables with name containing '{0}'...\n", search.Table);
            return DataAccess.SearchTables(arg);
        }

        private static string DescribeTable(string arg)
        {
            var search = new Search(arg);
            WriteLine("Server={0} Catalog={1} Schema={2}", search.Host, search.Catalog, search.Schema);
            WriteLine("Getting table definition of '{0}'...\n", search.Table);
            return DataAccess.DescribeTable(arg);
        }
    }
}