using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Scheduler.Extensions;

namespace Scheduler.TESTS.ExtensionsTests
{
    public class Int32ExtensionTests
    {
        [Fact]
        public void IsBetween()
        {
            var x = 25;
            var t = x.Between(2, 54);
            var f = x.Between(49, 50);

            Assert.True(t);
            Assert.False(f);
        }

        [Fact]
        public void TestToDate()
        {
            var x = 20151001;
            var dt = x.ToDate();
            Assert.Equal(2015, dt.Year);
            Assert.Equal(10, dt.Month);
            Assert.Equal(1, dt.Day);
        }


    }
}
