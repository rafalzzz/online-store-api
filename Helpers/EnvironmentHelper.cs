using DotNetEnv;

namespace OnlineStoreAPI.Helpers
{
    public class EnvironmentHelper
    {
        public static void EnsureConnectionStringVariableExists(string connectionStringName)
        {
            Env.Load();

            string isConnectionString = Environment.GetEnvironmentVariable(connectionStringName);

            if (isConnectionString is null)
            {
                string DB_PORT = Environment.GetEnvironmentVariable("DB_PORT");
                string DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
                string DB_DATABASE = Environment.GetEnvironmentVariable("DB_DATABASE");
                string DB_USERNAME = Environment.GetEnvironmentVariable("DB_USERNAME");
                string DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");

                string combinedVariable = $"Host={DB_HOST};Port={DB_PORT};Database={DB_DATABASE};Username={DB_USERNAME};Password={DB_PASSWORD}";

                string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                File.AppendAllText(envFilePath, $"\n{connectionStringName}={combinedVariable}");
            }
        }
    }
}