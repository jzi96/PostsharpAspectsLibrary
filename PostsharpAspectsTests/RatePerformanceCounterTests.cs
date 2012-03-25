using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class RatePerformanceCounterTests
    {
        public class RateTest
        {
            [RatePerformanceCounter("NUnit", "RateTests")]
            public void Run(){}
        }
        [Test]
        public void Test()
        {
            //setup
            RateTest test = new RateTest();

            //act
            test.Run();

            //assert
        }
    }
}
