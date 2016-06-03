namespace DbAsk
{
    using System.Configuration;

    internal class Search
    {
        internal Search(string arg)
        {
            if (arg == null)
            {
                return;
            }

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
    }
}
