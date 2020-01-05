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
    public class WithdrawMoneyTests
    {
        WithdrawMoney sut;
        Mock<IAccountRepository> mockAccountRepository;
        Mock<INotificationService> mockNotificationService;

        Guid fromAccountId = Guid.NewGuid();
        Guid fromUserId = Guid.NewGuid();

        User fromUser;

        AccountBuilder fromAccountBuilder;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            fromUser = new UserBuilder().WithId(fromUserId).WithEmail("from@user.com");
        }

        [SetUp]
        public void SetUp()
        {
            mockAccountRepository = new Mock<IAccountRepository>();
            mockNotificationService = new Mock<INotificationService>();

            fromAccountBuilder = new AccountBuilder().WithId(fromAccountId).WithUser(fromUser);
            mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccountBuilder);

            sut = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);
        }

        public class Execute : WithdrawMoneyTests
        {
            [Test]
            public void ShouldGetFromAccount()
            {
                sut.Execute(fromAccountId, 0m);

                mockAccountRepository.Verify(m => m.GetAccountById(fromAccountId), Times.Once());
            }

            [Test]
            public void ShouldThrowException_WhenSufficientFundsAreNotAvailable()
            {
                Assert.Throws<InvalidOperationException>(() => { sut.Execute(fromAccountId, 100m); });
            }

            [Test]
            public void ShouldNotify_WhenFundsAreLow()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(1000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, 750m);

                mockNotificationService.Verify(m => m.NotifyFundsLow(fromUser.Email), Times.Once());
            }

            [Test]
            public void ShouldNotNotify_WhenFundsAreNotLow()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(1000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, 100m);

                mockNotificationService.Verify(m => m.NotifyFundsLow(fromUser.Email), Times.Never());
            }

            [Test]
            public void ShouldDebitFromAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m).WithWithdrawn(0m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, 250m);

                Assert.AreEqual(4750m, fromAccount.Balance);
                Assert.AreEqual(-250m, fromAccount.Withdrawn);
            }

            [Test]
            public void ShouldUpdateFromAccount()
            {
                Account fromAccount = fromAccountBuilder.WithBalance(5000m);
                mockAccountRepository.Setup(m => m.GetAccountById(fromAccountId)).Returns(fromAccount);

                sut.Execute(fromAccountId, 250m);

                mockAccountRepository.Verify(m => m.Update(fromAccount), Times.Once());
            }
        }
    }
}
