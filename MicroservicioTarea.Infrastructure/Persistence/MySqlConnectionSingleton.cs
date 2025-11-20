using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace MicroservicioTarea.Infrastructure.Persistence
{
    public class MySqlConnectionSingleton
    {
        private readonly string _connectionString;

        public MySqlConnectionSingleton(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("TareaDbConnection");
        }

        public MySqlConnection CreateConnection()
            => new MySqlConnection(_connectionString);
    }
}
