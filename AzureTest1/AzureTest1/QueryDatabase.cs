using System;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Data;



/** UWAGI 
 * uwaga na nulle https://stackoverflow.com/questions/1772025/sql-data-reader-handling-null-column-values/1772037
 * używając SqlDataReader dla każdej kolumny użyć get właściwego dla typu (GetString, GetInt32...)
 * rola "using": pominięcie dispose (dobra praktyka: zawsze robić dispose obiektów IDisposable); defines a scope at the end of which an object will be disposed
 * w przypadku simple using statement dispose będzie na końcu otaczającej metody
 * podobno ok jest otwieranie i zamykanie połączenia do bazy dla każdego zapytania
 * w public static SqlDataReader? QueryDB znak zapytania oznacza, że tej metodzie wolno zwrócić null-a
 */


namespace MarketScreener
{
    class QueryDatabase
    {
        public static int ExecuteSQLStatement(string connectionString, string query, bool throwError, out DataTable dataTable)
        {
            dataTable = new DataTable();
            SqlConnection connection = new(connectionString);
            int rows = -1;

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqlCommand cmd = new()
                    {
                        CommandText = query,
                        CommandType = CommandType.Text,
                        Connection = connection
                    };

                    SqlDataAdapter da = new(cmd);

                    using (da)
                    {
                        rows = da.Fill(dataTable);
                    }

                    connection.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                    if (throwError)
                    {
                        throw e;                        
                    }
                    return -1;
                }
            }

            if (dataTable.Columns.Count == 0) //surogat na wypadek insert, update, delete
            {
                dataTable.Columns.Add("c_NO_RESULT_TABLE");
                DataRow r = dataTable.NewRow();
                r[0] = "NO_RESULT_TABLE";
                dataTable.Rows.Add(r);
            }

            return rows;
        }

        public static int ExecuteSQLStatement(string connectionString, string query, bool throwError, out bool success)
        {
            try
            {
                SqlConnection connection = new(connectionString);
                using (connection)
                {
                    using SqlCommand command = new(query, connection);
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    success = true;
                    return rows;
                }
            }
            catch (SqlException e)
            {
                success = false;
                Console.WriteLine(e);
                if (throwError)
                {
                    throw e;
                }
                return -1;
            }
        }

        public static void Test()
        {
            string cs = "Server = tcp:jkublickiserver1.database.windows.net,1433; Database=DB1; Initial Catalog = DB1; Persist Security Info = False; User ID = <...>; Password = <...>; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
            string q = String.Concat("INSERT INTO ENU_TICKER (TickerGoogleFinance) VALUES ('test_", Guid.NewGuid().ToString().AsSpan(0, 7), "')");
            int rows = ExecuteSQLStatement(cs, q, true, out bool _);

            if (rows != -1)
            {
                Console.WriteLine(rows.ToString() + " rows affected");
            }
            else
            {
                throw new Exception("Błąd w QueryDatabase.Test() - non query");
            }


            q = "SELECT TOP 3 TickerGoogleFinance FROM ENU_TICKER";
            rows = ExecuteSQLStatement(cs, q, true, out DataTable d);

            if (rows != -1 && d != null && d.Rows.Count > 0 && d.Rows[0].ItemArray.Count() > 0)
            {
                Console.WriteLine(rows.ToString() + " rows affected");
                Console.WriteLine(d.Rows[0].ItemArray[0].ToString());
            }
            else
            {
                throw new Exception("Błąd w QueryDatabase.Test() - query");
            }






        }
    }
}
