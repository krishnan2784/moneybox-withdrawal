﻿using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);

            from.EnsureSufficientFundsAreAvailable(amount);

            if (from.IsBreachingLowFundsAmount(amount))
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }

            from.Debit(amount);

            accountRepository.Update(from);
        }
    }
}
