using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal PayInLimitApproachWarningAmount = 500m;
        public const decimal LowFundsWarningAmount = 500m;

        public Account(Guid id, User user, decimal balance, decimal withdrawn, decimal paidIn)
        {
            Id = id;
            User = user;
            Balance = balance;
            Withdrawn = withdrawn;
            PaidIn = paidIn;
        }

        public Guid Id { get; }

        public User User { get; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }

        public void EnsureSufficientFundsAreAvailable(decimal amount)
        {
            if (Balance - amount < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }
        }

        public bool IsBreachingLowFundsAmount(decimal amount)
        {
            return Balance - amount < LowFundsWarningAmount;
        }

        public void EnsurePayInLimitIsNotExceeded(decimal amount)
        {
            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }
        }

        public bool IsApproachingPayInLimit(decimal amount)
        {
            return PayInLimit - PaidIn - amount < PayInLimitApproachWarningAmount;
        }

        public void Debit(decimal amount)
        {
            Balance -= amount;
            Withdrawn -= amount;
        }

        public void Credit(decimal amount)
        {
            Balance += amount;
            PaidIn += amount;
        }
    }
}
