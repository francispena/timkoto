using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.Attributes;
using NHibernate.Tool.hbm2ddl;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Timkoto.Data.Repositories;

namespace Timkoto.UsersApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Adds the n hibernate.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddNHibernate(this IServiceCollection services, string connectionString)
        {
            var configuration = new Configuration();
            configuration.DataBaseIntegration(c =>
            {
                c.Dialect<MySQL55InnoDBDialect>();
                c.ConnectionString = connectionString;
                c.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                c.SchemaAction = SchemaAutoAction.Validate;
                c.LogFormattedSql = false;
                c.LogSqlInConsole = false;
            });

            var serializer = new HbmSerializer();
            using (var s = serializer.Serialize(typeof(User).Assembly))
            {
#if DEBUG
                var rdr = new StreamReader(s);
                var xmlMapping = rdr.ReadToEnd();
                s.Position = 0;
                Debug.WriteLine(xmlMapping);
#endif
                configuration.AddInputStream(s);
            }

            //UpdateSchema(configuration);

            var sessionFactory = configuration.BuildSessionFactory();

            services.AddSingleton<ISessionFactory>(sessionFactory);
            services.AddScoped<ISession>(factory => sessionFactory.OpenSession());
            
            return services;
        }

        /// <summary>
        /// Updates the schema.
        /// </summary>
        /// <param name="nhConfiguration">The nh configuration.</param>
        private static void UpdateSchema(Configuration nhConfiguration)
        {
            var schemaUpdate = new SchemaUpdate(nhConfiguration);
            schemaUpdate.Execute(true, true);

            if (schemaUpdate.Exceptions != null && schemaUpdate.Exceptions.Count > 0)
            {
                throw schemaUpdate.Exceptions.First();
            }
        }
    }
}
