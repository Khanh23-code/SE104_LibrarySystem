using System.Configuration;
using Microsoft.Data.SqlClient;

namespace THUVIENZ.DAL
{
    public class DataProvider
    {
        private static DataProvider? _instance;
        public static DataProvider Instance => _instance ??= new DataProvider();

        private readonly string? _connectionString;

        private DataProvider()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["QL_ThuVien"]?.ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new ConfigurationErrorsException("Connection string 'QL_ThuVien' was not found in App.config");

            return new SqlConnection(_connectionString);
        }
    }
}
