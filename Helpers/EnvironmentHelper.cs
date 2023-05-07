using DotNetEnv;
using OnlineStoreAPI.Variables;

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
                string DB_PORT = Environment.GetEnvironmentVariable(EnvironmentVariables.DbPort);
                string DB_HOST = Environment.GetEnvironmentVariable(EnvironmentVariables.DbHost);
                string DB_DATABASE = Environment.GetEnvironmentVariable(EnvironmentVariables.DbDatabase);
                string DB_USERNAME = Environment.GetEnvironmentVariable(EnvironmentVariables.DbUsername);
                string DB_PASSWORD = Environment.GetEnvironmentVariable(EnvironmentVariables.DbPassword);

                string combinedVariable = $"Host={DB_HOST};Port={DB_PORT};Database={DB_DATABASE};Username={DB_USERNAME};Password={DB_PASSWORD}";

                string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                File.AppendAllText(envFilePath, $"\n{connectionStringName}={combinedVariable}");
            }
        }
    }
}