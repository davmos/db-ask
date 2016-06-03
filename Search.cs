namespace DbAsk
{
    using System.Configuration;

    public class Search
    {
        public Search(string arg)
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

            if (parts.Length == 3)
            {
                this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                this.Catalog = parts[0];
                this.Schema = parts[1];
                this.Table = parts[2];
                return;
            }

            if (parts.Length == 2)
            {
                this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                this.Catalog = ConfigurationManager.AppSettings["DefaultCatalog"];
                this.Schema = parts[0];
                this.Table = parts[1];
                return;
            }

            if (parts.Length == 1)
            {
                this.Host = ConfigurationManager.AppSettings["DefaultHost"];
                this.Catalog = ConfigurationManager.AppSettings["DefaultCatalog"];
                this.Schema = ConfigurationManager.AppSettings["DefaultSchema"];
                this.Table = parts[0];
            }
        }

        public string Host { get; set; }
        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
    }
}
