using Moneybox.App.Tests.Builders;
using NUnit.Framework;
using System;

namespace Moneybox.App.Tests.Domain
{
    [TestFixture]
    public class AccountTests
    {
        public class EnsureSufficientFundsAreAvailable : AccountTests
        {
            [Test]
            public void ShouldThrowException_WhenInsufficientFundsAreAvailable()
            {
                Account sut = new AccountBuilder().WithBalance(10m);

                Assert.Throws<InvalidOperationException>(() => { sut.EnsureSufficientFundsAreAvailable(20m); });
            }

            [Test]
            public void ShouldNotThrowException_WhenSufficientFundsAreAvailable()
            {
                Account sut = new AccountBuilder().WithBalance(10m);

                Assert.DoesNotThrow(() => { sut.EnsureSufficientFundsAreAvailable(5m); });
            }
        }

        public class IsBreachingLowFundsAmount : AccountTests
        {
            [Test]
            public void ShouldReturnTrue_WhenWarningAmountIsBreached()
            {
                Account sut = new AccountBuilder().WithBalance(600m);

                Assert.IsTrue(sut.IsBreachingLowFundsAmount(200m));
            }

            [Test]
            public void ShouldReturnFalse_WhenWarningAmountIsNotBreached()
            {
                Account sut = new AccountBuilder().WithBalance(600m);

                Assert.IsFalse(sut.IsBreachingLowFundsAmount(50m));
            }
        }

        public class EnsurePayInLimitIsNotExceeded : AccountTests
        {
            [Test]
            public void ShouldThrowException_WhenPayInLimitIsExceeded()
            {
                Account sut = new AccountBuilder().WithPaidIn(2000m);

                Assert.Throws<InvalidOperationException>(() => { sut.EnsurePayInLimitIsNotExceeded(3000m); });
            }

            [Test]
            public void ShouldNotThrowException_WhenPayInLimitIsNotExceeded()
            {
                Account sut = new AccountBuilder().WithPaidIn(2000m);

                Assert.DoesNotThrow(() => { sut.EnsurePayInLimitIsNotExceeded(2000m); });
            }
        }

        public class IsApproachingPayInLimit : AccountTests
        {
            [Test]
            public void ShouldReturnTrue_WhenApproachingPayInLimit()
            {
                Account sut = new AccountBuilder().WithPaidIn(2000m);

                Assert.IsTrue(sut.IsApproachingPayInLimit(1750m));
            }

            [Test]
            public void ShouldReturnFalse_WhenNotApproachingPayInLimit()
            {
                Account sut = new AccountBuilder().WithPaidIn(2000m);

                Assert.IsFalse(sut.IsApproachingPayInLimit(1000m));
            }
        }

        public class Debit : AccountTests
        {
            [Test]
            public void ShouldReduceBalance()
            {
                Account sut = new AccountBuilder().WithBalance(1000m);

                sut.Debit(250m);

                var expected = 750m;
                Assert.AreEqual(expected, sut.Balance);
            }

            [Test]
            public void ShouldReduceWithdrawn()
            {
                Account sut = new AccountBuilder().WithWithdrawn(0m);

                sut.Debit(250m);

                var expected = -250m;
                Assert.AreEqual(expected, sut.Withdrawn);
            }
        }

        public class Credit : AccountTests
        {
            [Test]
            public void ShouldIncreaseBalance()
            {
                Account sut = new AccountBuilder().WithBalance(1000m);

                sut.Credit(250m);

                var expected = 1250m;
                Assert.AreEqual(expected, sut.Balance);
            }

            [Test]
            public void ShouldIncreasePaidIn()
            {
                Account sut = new AccountBuilder().WithPaidIn(1000m);

                sut.Credit(250m);

                var expected = 1250m;
                Assert.AreEqual(expected, sut.PaidIn);
            }
        }
    }
}
