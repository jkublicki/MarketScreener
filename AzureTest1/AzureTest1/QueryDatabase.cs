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
        const bool Enabled = true;

        public static int ExecuteSQLStatement(string connectionString, string query, bool throwError, out DataTable? dataTable)
        {
            if(!Enabled)
            {
                dataTable = null;
                return 0;
            }                

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
                    if (Log.Enabled)
                        Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatement() failed with message: ", e.Message, ". Full exception: ", e.ToString()));

                    if (Log.DebugEnabled)
                        Log.Entry(String.Concat("Failed query: ", query));

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
            if (!Enabled)
            {
                success = true;
                return 0;
            }

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

                if (Log.Enabled)
                    Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatement() failed with message: ", e.Message, ". Full exception: ", e.ToString()));

                if (Log.DebugEnabled)
                    Log.Entry(String.Concat("Failed query: ", query));

                if (throwError)
                {
                    throw e;
                }
                return -1;

            }
        }

    }
}