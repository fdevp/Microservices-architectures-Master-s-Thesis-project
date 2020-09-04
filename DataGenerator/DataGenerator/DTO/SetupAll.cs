using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator.DTO
{
    public class SetupAll
    {
        public SetupAll Concat(SetupAll setup)
        {
            return new SetupAll
            {
                AccountsSetup = this.AccountsSetup.Concat(setup.AccountsSetup),
                CardsSetup = this.CardsSetup.Concat(setup.CardsSetup),
                UsersSetup = this.UsersSetup.Concat(setup.UsersSetup),
                LoansSetup = this.LoansSetup.Concat(setup.LoansSetup),
                PaymentsSetup = this.PaymentsSetup.Concat(setup.PaymentsSetup),
                TransactionsSetup = this.TransactionsSetup.Concat(setup.TransactionsSetup),
            };
        }

        public AccountsSetup AccountsSetup { get; set; }
        public CardsSetup CardsSetup { get; set; }
        public LoansSetup LoansSetup { get; set; }
        public PaymentsSetup PaymentsSetup { get; set; }
        public TransactionsSetup TransactionsSetup { get; set; }
        public UsersSetup UsersSetup { get; set; }
    }

    public class AccountsSetup
    {
        public AccountsSetup Concat(AccountsSetup setup)
        {
            return new AccountsSetup { Accounts = this.Accounts.Concat(setup.Accounts).ToArray() };
        }

        public AccountDTO[] Accounts { get; set; } = new AccountDTO[0];
    }

    public class CardsSetup
    {
        public CardsSetup Concat(CardsSetup setup)
        {
            return new CardsSetup { Cards = this.Cards.Concat(setup.Cards).ToArray() };
        }

        public CardDTO[] Cards { get; set; } = new CardDTO[0];
    }

    public class LoansSetup
    {
        public LoansSetup Concat(LoansSetup setup)
        {
            return new LoansSetup { Loans = this.Loans.Concat(setup.Loans).ToArray() };
        }

        public LoanDTO[] Loans { get; set; } = new LoanDTO[0];
    }

    public class PaymentsSetup
    {
        public PaymentsSetup Concat(PaymentsSetup setup)
        {
            return new PaymentsSetup { Payments = this.Payments.Concat(setup.Payments).ToArray() };
        }

        public PaymentDTO[] Payments { get; set; } = new PaymentDTO[0];
    }

    public class TransactionsSetup
    {
        public TransactionsSetup Concat(TransactionsSetup setup)
        {
            if (setup.Transactions == null)
                return this;

            return new TransactionsSetup { Transactions = this.Transactions.Concat(setup.Transactions).ToArray() };
        }

        public TransactionDTO[] Transactions { get; set; } = new TransactionDTO[0];
    }

    public class UsersSetup
    {
        public UsersSetup Concat(UsersSetup setup)
        {
            return new UsersSetup { Users = this.Users.Concat(setup.Users).ToArray() };
        }

        public UserDTO[] Users { get; set; } = new UserDTO[0];
    }

}
