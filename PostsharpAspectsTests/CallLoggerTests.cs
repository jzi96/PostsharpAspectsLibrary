using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using log4net.Appender;
using NUnit.Framework;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class CallLoggerTestsWithoutLogger
    {
        private MemoryAppender InitLog()
        {
            var appender = new log4net.Appender.MemoryAppender();
            var filter = new log4net.Filter.LoggerMatchFilter { LoggerToMatch = "Calls", AcceptOnMatch = true };
            appender.AddFilter(filter);
            log4net.Config.BasicConfigurator.Configure(appender);
            return appender;
        }

        [Test]
        public void ProfileMethodWithoutArgs_Logs2Messages()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat();
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(2));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Entering TestClass.Flat"));
            Assert.That(result[1].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Leaving TestClass.Flat"));
        }

        [Test]
        public void ProfileMethodWithArgs_Logs2Messages()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat2("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(2));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Entering TestClass.Flat2"));
            Assert.That(result[1].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Leaving TestClass.Flat2"));
        }
        [Test]
        public void ProfileMethodWithArgsGeneric_Logs2Messages()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat3<string>("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(2));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Entering TestClass.Flat3"));
            Assert.That(result[1].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Leaving TestClass.Flat3"));
        }
        [Test]
        public void ProfileMethodWithArgsReturns_Logs2Messages()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            item.Flat4("args passed");
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(2));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Entering TestClass.Flat4\r\nEntering with args 'args passed'"));
            Assert.That(result[1].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Leaving TestClass.Flat4"));
        }
        [Test]
        public void ProfileMethodWithArgs_RaisedEx_Logs4Messages()
        {
            MemoryAppender appender = InitLog();
            var item = new TestClass();
            try
            {
                item.Flat5Ex("args passed");
                Assert.Fail("Ex should be raised!");
            }
            catch   {}
            var result = appender.GetEvents();
            Assert.That(result.Length, new NUnit.Framework.Constraints.EqualConstraint(4));
            Assert.That(result[0].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Entering TestClass.Flat5Ex"));
            Assert.That(result[1].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Error executing TestClass.Flat5Ex"));
            Assert.That(result[2].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("args passed"));
            Assert.That(result[3].RenderedMessage, new NUnit.Framework.Constraints.StartsWithConstraint("Leaving TestClass.Flat5Ex with Exception: Failed call"));
        }
        public class TestClass
        {
            [LogCalls()]
            public void Flat()
            {
                Trace.WriteLine("Method called");
            }
            [LogCalls()]
            public void Flat2(string someArg)
            {
                Trace.WriteLine("Method called " + someArg);
            }
            [LogCalls()]
            public void Flat3<T>(T someArg)
            {
                Trace.WriteLine("Method called " + typeof(T).FullName);
            }

            [LogCalls("Entering with args '{0}'")]
            public string Flat4(string someArg)
            {
                Trace.WriteLine("Method called " + someArg);
                return someArg;
            }
            [LogCalls()]
            public void Flat5Ex(string args)
            {
                throw new Exception("Failed call");
            }
        }
    }

}
