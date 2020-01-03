using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App.Tests.Builders;
using Moq;
using NUnit.Framework;
using System;

namespace Moneybox.App.Tests.Features
{
    [TestFixture]
    public class TransferMoneyTests
    {
        TransferMoney sut;
        Mock<IAccountRepository> mockAccountRepository;
        Mock<INotificationService> mockNotificationService;

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        Guid fromUserId = Guid.NewGuid();
        Guid toUserId = Guid.NewGuid();

        User fromUser;
        User toUser;

        AccountBuilder fromAccountBuilder;
        AccountBuilder toAccountBuilder;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            fromUser = new UserBuilder().WithId(fromUserId).WithEmail("from@user.com");
            toUser = new UserBuilder().WithId(toUserId).WithEmail("to@user.com");
        }

        [SetUp]
        public void SetUp()
        {
            mockAccountRepository = new Mock<IAccountRepository>();
            mockNotificationService = new Mock<INotificationService>();

            fromAccountBuilder = new AccountBuilder().WithId(fromAccountId).WithUser(fromUser);
            toAccountBuilder = new AccountBuilder().WithId(toAccountId).WithUser(toUser);

            mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccountBuilder);
            mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccountBuilder);

            sut = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
        }

        public class Execute : TransferMoneyTests
        {
            [Test]
            public void ShouldGetFromAccount()
            {
                sut.Execute(fromAccountId, toAccountId, 0m);

                mockAccountRepository.Verify(m => m.GetAccountById(fromAccountId), Times.Once());
            }

            [Test]
            public void ShouldGetToAccount()
            {
                sut.Execute(fromAccountId, toAccountId, 0m);

                mockAccountRepository.Verify(m => m.GetAccountById(toAccountId), Times.Once());
            }

            [Test]
            public void ShouldThrowException_WhenSufficientFundsAreNotAvailable()
            {
                Assert.Throws<InvalidOperationException>(() => { sut.Execute(fromAccountId, toAccountId, 100m); });
            }

            [Test]
            public void ShouldNotify_WhenFundsAreLow()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(1000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, toAccountId, 750m);

                mockNotificationService.Verify(m => m.NotifyFundsLow(fromUser.Email), Times.Once());
            }

            [Test]
            public void ShouldNotNotify_WhenFundsAreNotLow()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(1000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, toAccountId, 100m);

                mockNotificationService.Verify(m => m.NotifyFundsLow(fromUser.Email), Times.Never());
            }

            [Test]
            public void ShouldThrowException_WhenPayInLimitIsExceeded()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                Account toAccount = toAccountBuilder.WithPaidIn(3000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);
                mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccount);

                Assert.Throws<InvalidOperationException>(() => { sut.Execute(fromAccountId, toAccountId, 1500m); });
            }

            [Test]
            public void ShouldNotify_WhenApproachingPayInLimit()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                Account toAccount = toAccountBuilder.WithPaidIn(3000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);
                mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccount);

                sut.Execute(fromAccountId, toAccountId, 750m);

                mockNotificationService.Verify(m => m.NotifyApproachingPayInLimit(toUser.Email), Times.Once());
            }

            [Test]
            public void ShouldNotNotify_WhenNotApproachingPayInLimit()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                Account toAccount = toAccountBuilder.WithPaidIn(3000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);
                mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccount);

                sut.Execute(fromAccountId, toAccountId, 250m);

                mockNotificationService.Verify(m => m.NotifyApproachingPayInLimit(toUser.Email), Times.Never());
            }

            [Test]
            public void ShouldDebitFromAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, toAccountId, 250m);

                Assert.AreEqual(fromAccount.Balance, 4750m);
                Assert.AreEqual(fromAccount.Withdrawn, -250m);
            }

            [Test]
            public void ShouldCreditToAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                Account toAccount = toAccountBuilder.WithBalance(3000m).WithPaidIn(3000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);
                mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccount);

                sut.Execute(fromAccountId, toAccountId, 250m);

                Assert.AreEqual(toAccount.Balance, 3250m);
                Assert.AreEqual(toAccount.PaidIn, 3250m);
            }

            [Test]
            public void ShouldUpdateFromAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, toAccountId, 250m);

                mockAccountRepository.Verify(m => m.Update(fromAccount), Times.Once());
            }

            [Test]
            public void ShouldUpdateToAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                Account toAccount = toAccountBuilder;
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);
                mockAccountRepository.Setup(m => m.GetAccountById(toAccountId)).Returns(toAccount);

                sut.Execute(fromAccountId, toAccountId, 250m);

                mockAccountRepository.Verify(m => m.Update(toAccount), Times.Once());
            }
        }
    }
}
