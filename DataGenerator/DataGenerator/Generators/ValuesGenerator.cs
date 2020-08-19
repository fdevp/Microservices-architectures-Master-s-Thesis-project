using DataGenerator.Rnd;
using RandomNameGeneratorLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator
{
    public static class ValuesGenerator
    {
        private static PersonNameGenerator personGenerator = new PersonNameGenerator();
        private static Random random = new Random();

        public static IEnumerable<UserDTO> CreateUsers(int count)
        {
            var generated = new HashSet<string>();

            for (int i = 0; i < count; i++)
            {
                var name = $"{personGenerator.GenerateRandomFirstName().ToLower()}_{personGenerator.GenerateRandomLastName().ToLower()}";
                while (generated.Contains(name))
                    name = $"{personGenerator.GenerateRandomFirstName().ToLower()}_{personGenerator.GenerateRandomLastName().ToLower()}";
                generated.Add(name);
                yield return new UserDTO { Id = Guid.NewGuid().ToString(), Login = name, Password = "password" };
            }
        }

        public static IEnumerable<AccountDTO> CreateAccounts(UserDTO[] users, IRnd<int> countRnd, IRnd<float> balanceRnd)
        {
            foreach (var user in users)
            {
                var count = countRnd.Next();
                for (int j = 0; j <= count; j++)
                {
                    //var b = random.Next(balance.Min, balance.Max) + GetCents;
                    var balance = balanceRnd.Next();
                    yield return new AccountDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Balance = balance,
                        Number = CreateAccountNumber(),
                        UserId = user.Id
                    };
                }
            }
        }

        public static IEnumerable<CardDTO> CreateCards(AccountDTO[] accounts, IRnd<int> countRnd)
        {
            foreach (var account in accounts)
            {
                var count = countRnd.Next();
                for (int i = 0; i < count; i++)
                    yield return new CardDTO { Id = Guid.NewGuid().ToString(), AccountId = account.Id, Number = CreateCardNumber() };
            }
        }

        public static IEnumerable<PaymentDTO> CreatePayments(AccountDTO[] accounts, PaymentStatus status, IRnd<string> recipientRnd,
            IRnd<int> countRnd, IRnd<float> amountRnd, IRnd<DateTime> startDateRnd, Func<DateTime> datetimeNow, IRnd<TimeSpan> intervalRnd)
        {
            foreach (var account in accounts)
            {
                var count = countRnd.Next();
                for (int i = 0; i < count; i++)
                {
                    var interval = intervalRnd.Next();
                    var startTimestamp = startDateRnd.Next();
                    var repayDiff = (int)((datetimeNow() - startTimestamp) / interval);
                    var lastRepayTimestamp = startTimestamp + repayDiff * interval;
                    yield return new PaymentDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountId = account.Id,
                        Amount = amountRnd.Next(),
                        StartTimestamp = startTimestamp,
                        LatestProcessingTimestamp = lastRepayTimestamp,
                        Interval = interval,
                        Status = (int)status,
                        Recipient = recipientRnd.Next(account.Id)
                    };
                }
            }
        }

        public static IEnumerable<(LoanDTO, PaymentDTO)> CreateLoans(AccountDTO[] loansAccounts, IRnd<int> countRnd, IRnd<float> totalRnd,
            LoanInstalmentsRnd instalmentsRnd, Rnd<int> paidInstalmentsRnd, IRnd<TimeSpan> intervalRnd, Func<DateTime> datetimeNow, IRnd<string> recipientRnd)
        {
            foreach (var account in loansAccounts)
            {
                var count = countRnd.Next();
                for (int i = 0; i < count; i++)
                {
                    var totalAmount = totalRnd.Next();
                    var instalments = instalmentsRnd.Next(totalAmount);
                    paidInstalmentsRnd.Max = instalments;
                    var paidInstalments = paidInstalmentsRnd.Next();
                    var paidAmount = (float)paidInstalments / instalments * totalAmount;

                    var loan = new LoanDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Instalments = instalments,
                        TotalAmount = totalAmount,
                        PaidAmount = paidAmount,
                        AccountId = account.Id
                    };

                    var interval = intervalRnd.Next();
                    var start = datetimeNow() - interval * paidInstalments;
                    var recipient = recipientRnd.Next(account.Id);
                    var lastRepayTimestamp = start + interval * paidInstalments;
                    var payment = new PaymentDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountId = account.Id,
                        Amount = totalAmount / instalments,
                        Interval = interval,
                        StartTimestamp = start,
                        LatestProcessingTimestamp = lastRepayTimestamp,
                        Status = (int)PaymentStatus.ACTIVE,
                        Recipient = recipient
                    };

                    loan.PaymentId = payment.Id;
                    yield return (loan, payment);
                }
            }
        }

        public static IEnumerable<TransactionDTO> CreateTransactions(LoanDTO[] loans, Dictionary<string, PaymentDTO> payments)
        {
            var now = DateTime.Now;
            foreach (var loan in loans)
            {
                var payment = payments[loan.PaymentId];
                var date = payment.StartTimestamp;
                var paidInstalments = (loan.PaidAmount / loan.TotalAmount) * loan.Instalments;

                for (int i = 0; i < paidInstalments; i++)
                {
                    yield return new TransactionDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Amount = payment.Amount,
                        PaymentId = payment.Id,
                        Sender = payment.AccountId,
                        Recipient = payment.Recipient,
                        Timestamp = date,
                        Title = $"{i + 1} of {loan.Instalments} instalments. Payment {"nejm"}({payment.Id})"
                    };
                }

                date += payment.Interval;
            }
        }

        public static IEnumerable<TransactionDTO> CreateTransactions(PaymentDTO[] payments, DateTime? maxDate = null)
        {
            var now = DateTime.Now;
            foreach (var payment in payments)
            {
                var end = maxDate ?? DateTime.UtcNow;
                for (var date = payment.StartTimestamp; date < end; date += payment.Interval)
                {
                    yield return new TransactionDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Amount = payment.Amount,
                        PaymentId = payment.Id,
                        Sender = payment.AccountId,
                        Recipient = payment.Recipient,
                        Timestamp = date,
                        Title = $"Transaction of payment {"nejm"}({payment.Id})"
                    };

                    date += payment.Interval;
                }
            }
        }

        public static IEnumerable<TransactionDTO> CreateTransactions(CardDTO[] cards, IRnd<string> recipientRnd, IRnd<int> countRnd, IRnd<DateTime> timestampRnd, IRnd<float> amountRnd)
        {
            foreach (var card in cards)
            {
                int count = countRnd.Next();
                for (int i = 0; i < count; i++)
                {
                    var timestamp = timestampRnd.Next();
                    var amount = amountRnd.Next();
                    yield return new TransactionDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Amount = amount,
                        CardId = card.Id,
                        Sender = card.AccountId,
                        Timestamp = timestamp,
                        Recipient = recipientRnd.Next(card.AccountId),
                        Title = $"{timestamp} card usage for a transfer worth {amount} EUR."
                    };
                }
            }
        }

        public static IEnumerable<TransactionDTO> CreateTransactions(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<int> countRnd, IRnd<DateTime> timestampRnd, IRnd<float> amountRnd, IRnd<string> titleRnd)
        {
            foreach (var account in accounts)
            {
                int count = countRnd.Next();
                for (int i = 0; i < count; i++)
                {
                    var debit = random.Next(0, 1) == 1;
                    var sender = debit ? account.Id : recipientRnd.Next(account.Id);
                    var recipient = !debit ? recipientRnd.Next(account.Id) : account.Id;
                    yield return new TransactionDTO
                    {
                        Id = Guid.NewGuid().ToString(),
                        Amount = amountRnd.Next(),
                        Sender = sender,
                        Timestamp = timestampRnd.Next(),
                        Recipient = recipient,
                        Title = titleRnd.Next()
                    };
                }
            }
        }

        private static float GetCents() => (float)Math.Round(random.NextDouble(), 2);

        private static string CreateCardNumber() => $"{RandomWithPad(4)}{RandomWithPad(4)}{RandomWithPad(4)}{RandomWithPad(4)}";

        private static string CreateAccountNumber() => $"{82}{1020}{RandomWithPad(4)}{RandomWithPad(4)}{RandomWithPad(4)}{RandomWithPad(4)}{RandomWithPad(4)}";

        private static string RandomWithPad(int pad) => random.Next((int)Math.Pow(10, pad) - 1).ToString().PadLeft(pad);
    }
}
