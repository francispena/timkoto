using System.Diagnostics;
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.Attributes;
using Timkoto.Data.Repositories;

namespace Timkoto.WebSocket
{
    public class DbSession
    {
        private static Configuration _configuration;

        public DbSession()
        {
            _configuration = new Configuration();
            _configuration.DataBaseIntegration(c =>
            {
                c.Dialect<MySQL55InnoDBDialect>();
                c.ConnectionString = "server=timkotodb-dev.cpxv8mduindt.ap-southeast-1.rds.amazonaws.com;database=timkotodb;uid=admin;pwd=teamkoto";
                c.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                c.SchemaAction = SchemaAutoAction.Validate;
                c.LogFormattedSql = false;
                c.LogSqlInConsole = false;

                var serializer = new HbmSerializer();
                using (var s = serializer.Serialize(typeof(User).Assembly))
                {
                    _configuration.AddInputStream(s);
                }
            });
        }

        public ISessionFactory GetSessionFactory()
        {
            return _configuration.BuildSessionFactory();
        }
    }
}
