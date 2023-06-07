namespace DataLibrary
{
    public class Operation
    {
        public Operation(int accountNumer, decimal credit, decimal debit)
        {
            AccountNumer = accountNumer;
            Credit = credit;
            Debit = debit;
        }

        public int AccountNumer { get;  set; }
        public decimal Credit { get;  set; }
        public decimal Debit { get;  set; }
    }
}