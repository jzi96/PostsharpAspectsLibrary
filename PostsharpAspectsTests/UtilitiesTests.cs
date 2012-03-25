using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture][Explicit]
    public class UtilitiesTests
    {
        [Test]
        public void GetContractNamespace_LongWithVersion_ExpectFull()
        {
            //Setup
            var testType = typeof (UtilitiesTests);

            //act
            var result=Utilities.GetContractNamespace(testType);
            //assert
            Console.WriteLine(result);
            Assert.That(result, new EqualConstraint("contracts://net.zieschang/Projects/PostsharpAspects/Tests/1.0/"));
        }
        [Test]
        public void GetContractNamespace_SmallWithVersion_ExpectLessModified()
        {
            //Setup
            var testType = typeof (de.Small.ShortNamespaceClass);

            //act
            var result=Utilities.GetContractNamespace(testType);
            //assert
            Console.WriteLine(result);
            Assert.That(result, new EqualConstraint("de/Small/1.0/"));
        }
    }

}

namespace de.Small
{
    public class ShortNamespaceClass
    {
    }
}