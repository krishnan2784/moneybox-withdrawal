using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            from.EnsureSufficientFundsAreAvailable(amount);

            if (from.IsBreachingLowFundsAmount(amount))
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }

            to.EnsurePayInLimitIsNotExceeded(amount);

            if (to.IsApproachingPayInLimit(amount))
            {
                notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }

            from.Debit(amount);
            to.Credit(amount);

            accountRepository.Update(from);
            accountRepository.Update(to);
        }
    }
}
