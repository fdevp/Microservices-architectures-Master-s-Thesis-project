using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerator.DTO
{
    public class SetupAll
    {
        public AccountsSetup AccountsSetup { get; set; }
        public CardsSetup CardsSetup { get; set; }
        public LoansSetup LoansSetup { get; set; }
        public PaymentsSetup PaymentsSetup { get; set; }
        public TransactionsSetup TransactionsSetup { get; set; }
        public UsersSetup UsersSetup { get; set; }
    }

    public class AccountsSetup
    {
        public AccountDTO[] Accounts { get; set; }
    }

    public class CardsSetup
    {
        public CardDTO[] Cards { get; set; }
    }

    public class LoansSetup
    {
        public LoanDTO[] Loans { get; set; }
    }

    public class PaymentsSetup
    {
        public PaymentDTO[] Payments { get; set; }
    }

    public class TransactionsSetup
    {
        public TransactionDTO[] Transactions { get; set; }
    }

    public class UsersSetup
    {
        public UserDTO[] Users { get; set; }
    }

}
