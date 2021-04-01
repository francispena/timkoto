namespace Timkoto.UsersApi.BaseClasses
{
    public class AppConfig : IAppConfig
    {
        public bool IsProduction { get; }

        public AppConfig(bool isProd)
        {
            IsProduction = isProd;
        }
    }
}
