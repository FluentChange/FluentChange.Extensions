
using FluentChange.Extensions.System.Helper;
using NUnit.Framework;
using System;

namespace Tests
{
    public class DateTimeHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TotalMonthsUntil()
        {
            var from = new DateTime(2021, 2, 1);
            var to = new DateTime(2022, 4, 1);

            var result = from.TotalMonthsUntil(to);
            Assert.AreEqual(14, result);

            var result2 = to.TotalMonthsUntil(from);
            Assert.AreEqual(-14, result2);
        }

        [Test]
        public void CountMonthsBetween()
        {
            var from = new DateTime(2021, 2, 1);
            var to = new DateTime(2022, 4, 1);

            var result = from.CountMonthsBetween(to);
            Assert.AreEqual(15, result);

            Assert.Throws<ArgumentException>(() => { to.CountMonthsBetween(from); });

        }
    }
}