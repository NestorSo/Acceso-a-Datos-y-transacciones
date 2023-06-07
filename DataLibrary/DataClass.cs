using System.Data.SqlClient;

namespace DataLibrary
{
    public class DataClass
    {

        public void writeDBAccount(string  connstr,Operation operation)
        {
            string sql = "INSET INTO Accounts(AccountNumber,Credit,Debit)  VALUES(@account,@credit,@debit)";

            //Operacion de escritura en la tabla accounts
            SqlConnection conn = new SqlConnection(connstr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@account", operation.AccountNumer);
            command.Parameters.AddWithValue("@credit", operation.Credit);
            command.Parameters.AddWithValue("@debit", operation.Debit);

            try
            {
                command.ExecuteNonQuery();
            }finally
            {
                conn.Close();
            }
        }

    }
}