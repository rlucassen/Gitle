namespace Gitle.Web.Helpers
{
    using System.Configuration;
    using System.Data.SqlClient;

    public class SqlConnectionHelper
    {
        private SqlConnection _connection;

        public SqlDataReader ExecuteSqlQuery(string connectionStringName, string sqlCommand)
        {
            _connection = new SqlConnection {ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString};
            _connection.Open();

            SqlCommand command = new SqlCommand(sqlCommand, _connection);

            return command.ExecuteReader();
        }

        public void CloseSqlConnection()
        {
            _connection?.Close();
        }
    }
}