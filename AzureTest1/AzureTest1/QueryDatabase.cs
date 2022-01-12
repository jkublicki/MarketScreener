using System;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Data;
using System.Diagnostics;



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
        public static int ExecuteSQLStatement(string connectionString, string query, bool throwError, out DataTable? dataTable)
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
                    Debug.WriteLine(e.ToString());

                    if (Log.Enabled)
                        Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatement() failed:\n  Query: ", query, "\n  Message:", e.Message));

                    if (Log.DebugEnabled)
                        Log.Entry(String.Concat("Full exception in QueryDatabase.ExecuteSQLStatement():\n", e.ToString()));

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

        public static List<int> ExecuteSQLStatementNQ(string connectionString, List<string> queries, bool throwError, out bool success)
        {
            if (!queries.Any())
            {
                string msg = "ExecuteSQLStatement(), NonQuery variant, called with empty list of queries. Run stopped.";
                success = false;
                Log.Entry(msg);
                throw new Exception(msg);
            }

            if (Log.DebugEnabled)
                Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatement(), NonQuery variant, about to execute queries:\n",
                    String.Join("\n", queries.Select(q => q[..Math.Min(200, q.Length)]))));                    

            List<int> rows = new ();
            string lastQuery = "";
            
            try
            {
                lastQuery = queries[0];

                SqlConnection connection = new(connectionString);
                connection.Open();

                using (connection)
                {
                    foreach (string query in queries)
                    {
                        lastQuery = query;
                        using SqlCommand command = new(query, connection);
                        
                        int r = command.ExecuteNonQuery();
                        rows.Add(r);                     
                        if (Log.DebugEnabled)
                            Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatementNQ() execution successful, ", r.ToString(), " row affected."));                        
                    }
                    if (rows.Any(r => r == -1))
                        success = false;
                    else
                        success = true;
                    return rows;
                }
            }
            catch (SqlException e)
            {
                Debug.WriteLine(e.ToString());

                success = false;

                if (Log.Enabled)
                    Log.Entry(String.Concat("QueryDatabase.ExecuteSQLStatementNQ() failed:\n  Query: ", lastQuery, "\n  Message:", e.Message));

                if (Log.DebugEnabled)
                    Log.Entry(String.Concat("Full exception in QueryDatabase.ExecuteSQLStatementNQ():\n", e.ToString()));

                if (throwError)
                {
                    throw e;
                }
                
                while (rows.Count() < queries.Count())
                {
                    rows.Add(-1);
                }
                return rows;
            }

            

        }

    }
}