using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SO_NAIJI
{
    public class SQL
    {
        private static string _connectionString;

        static SQL()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = @"IP\Server";
            builder.InitialCatalog = "Schema";
            builder.IntegratedSecurity = false;
            builder.UserID = "User";
            builder.Password = "Password";
            _connectionString = builder.ToString();
        }

        public static DataTable GetCustSoNo(string CustCode)
        {
            var sql = @"SELECT Column FROM Table WITH(NOLOCK) WHERE Date >= '" + DateTime.Today.AddYears(-3).ToString("yyyy/MM/dd") + "' AND Code = '" + CustCode + "'";

            DataTable outputDt = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(sql, connection))
            {
                connection.Open();
                adapter.Fill(outputDt);
            }

            return outputDt;
        }
    }
}
