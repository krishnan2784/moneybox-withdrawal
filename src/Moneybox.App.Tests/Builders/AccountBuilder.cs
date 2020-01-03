using System;

namespace Moneybox.App.Tests.Builders
{
    public class AccountBuilder
    {
        Guid id = Guid.NewGuid();
        User user = new User(Guid.NewGuid(), "", "");
        decimal balance = 0;
        decimal withdrawn = 0;
        decimal paidIn = 0;

        public static implicit operator Account(AccountBuilder instance)
        {
            return instance.Build();
        }

        public Account Build()
        {
            return new Account(id, user, balance, withdrawn, paidIn);
        }

        public AccountBuilder WithId(Guid id)
        {
            this.id = id;
            return this;
        }

        public AccountBuilder WithUser(User user)
        {
            this.user = user;
            return this;
        }

        public AccountBuilder WithBalance(decimal balance)
        {
            this.balance = balance;
            return this;
        }

        public AccountBuilder WithWithdrawn(decimal withdrawn)
        {
            this.withdrawn = withdrawn;
            return this;
        }

        public AccountBuilder WithPaidIn(decimal paidIn)
        {
            this.paidIn = paidIn;
            return this;
        }
    }
}
