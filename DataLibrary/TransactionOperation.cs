using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Transactions;

namespace DataLibrary
{
    public class TransactionOperation
    {
        private readonly int _originAccount;
        private readonly int _DestinationAccount;
        private readonly decimal _amount;


        // Transferir fondos de una cuenta a otra (acreditar 200 de la cuenta 10 a la 20)
        decimal quantityToTransfer = 200;
        private SqlTransaction transaction;

        // Preparar consultas
        // Obtener si la cuenta 10 realmente tiene el dinero
        const string sqlAccount1Fund = "SELECT (Sum(Credit) - Sum(Debit)) as Saldo FROM Accounts WHERE AccountNumber = @OriginAccount";

        // Consulta para extraer el valor de la cuenta de origen
        const string sqlWithdrawCredit = "INSERT INTO Accounts(AccountNumber, Debit, Credit) VALUES (@OriginAccount, @Debit, 0)";

        // Consulta para depositar el dinero en destino
        const string sqlFundCredit = "INSERT INTO Accounts(AccountNumber, Debit, Credit) VALUES(@DestinationAccount, 0, @Credit)";


        public TransactionOperation(int origin, int destination, decimal amount)
        {
            _originAccount = origin;
            _DestinationAccount = destination;
            _amount = amount;
        }


        ///<sumary>
        ///esta ioperacion obtiene la cantidad de dinero enla cuenta de origen y si esta es mas que el monto a ser extraido 
        ///permite la operacion, extrae la cantidad de origne como decito y deposito en destion com credito 
        ///la ooperacion puede ser ejecutada asincrona 
        ///<sumary>
        ///<param name= "commit"> si la transaccion es verdadera commit, si no el rollback</param>
        ///<param name="delayEndTransaction">demora artificial para terminar la tranzaccion </param>
        ///<param name="level">el nivel de aislamiento </param>
        ///<param name="TransactionName">Un nombre arbitrario para la transaccion </param>
        ///<retturn>La tarea. Cuando la tarea es finalizada devuelve el valor vberdadero si esta bien</return>


        public Task<bool> ExecuteTransaction(bool commit, int delayTransaction, System.Data.IsolationLevel level, string TransactionName)
        {
            return Task<bool>.Run(() =>
           {
               //Obtener la cadena de conexion
               string connStr1 = ConfigurationManager.ConnectionStrings["Bank1"].ConnectionString;

               //Crear la conexion
               var conn = new SqlConnection(connStr1);


               //Prepara para leer la disponibilidad de dinero en origen 
               var command = new SqlCommand(sqlAccount1Fund, conn);
               command.Parameters.AddWithValue("@OriginAccount", _originAccount);


               //abre la conexion el la Db
               conn.Open();

               //crea la transaccion y se alista con eso el comando
               var transaction = conn.BeginTransaction(level, TransactionName);
               command.Transaction = transaction;
              
               try
               {
                   var funds = Convert.ToDecimal(command.ExecuteScalar());
                   Console.WriteLine($"{TransactionName} => fund: {funds} amount: {_amount}");

                   if (funds > _amount)
                   {
                            //Extrae el dinero
                       command = new SqlCommand(sqlWithdrawCredit, conn)
                       {
                           Transaction = transaction
                       };
                       command.Parameters.AddWithValue("@OriginAccount", _originAccount);
                       command.Parameters.AddWithValue("@Debit", _amount);
                       command.ExecuteNonQuery();
                            //Deposita el dinero
                       command = new SqlCommand(sqlWithdrawCredit, conn)
                       {
                           Transaction = transaction
                       };

                       command.Parameters.AddWithValue("@DestinationAccount", _DestinationAccount);
                       command.Parameters.AddWithValue("@Credit", _amount);
                       command.ExecuteNonQuery();
                       return true;
                   }
                   return false;
               }
               catch (Exception ex)
               {
                   transaction.Rollback();
                   Console.WriteLine(ex.Message);
                   return false;
               }
               finally
               {
                   // Si commit, commit no es rollback
                   if (commit)
                   {
                       transaction.Commit();
                   }
                     
                   else
                   {
                       Thread.Sleep(delayTransaction);
                       transaction.Rollback();
                   }
               }
         
        });

        }

    }
    }

