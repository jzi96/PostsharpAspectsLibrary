using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using log4net.Appender;
using NUnit.Framework;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class TimeProfileTests
    {
        private MemoryAppender InitLog()
        {
            var appender =new log4net.Appender.MemoryAppender();
            var filter = new log4net.Filter.LoggerMatchFilter { LoggerToMatch = typeof(Profiler).ToString(), AcceptOnMatch = true };       
            appender.AddFilter(filter);
            log4net.Config.BasicConfigurator.Configure(appender);
            return appender;
        }

        [Test]
        public void ProfileMethodWithoutArgs_ReturnsOneMessage()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat();
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(1));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("<TestClass> <Profile> <Elapsed "));
        }

        [Test]
        public void ProfileMethodWithArgs_ReturnsOneMessage()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat2("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(1));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("<TestClass> <Profile2> <Elapsed "));
        }
        [Test]
        public void ProfileMethodWithArgsGeneric_ReturnsOneMessage()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat3<string>("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(1));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("<TestClass> <Profile3> <Elapsed "));
        }
        [Test]
        public void ProfileMethodWithArgsReturns_ReturnsOneMessage()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat4("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(1));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("<TestClass> <Profile4> <Elapsed "));
        }
        public class TestClass
        {
            [TimeProfile("Profile")]
            public void Flat()
            {
                Trace.WriteLine("Method called");
            }
            [TimeProfile("Profile2")]
            public void Flat2(string someArg)
            {
                Trace.WriteLine("Method called " + someArg);
            }
            [TimeProfile("Profile3")]
            public void Flat3<T>(T someArg)
            {
                Trace.WriteLine("Method called " + typeof(T).FullName);
            }

            [TimeProfile("Profile4")]
            public string Flat4(string someArg)
            {
                Trace.WriteLine("Method called " + someArg);
                return someArg;
            }
        }
    }
}
