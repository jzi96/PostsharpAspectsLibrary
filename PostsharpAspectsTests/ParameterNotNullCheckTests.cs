using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PostSharp.Extensibility;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class ParameterNotNullCheckTests
    {
        private class MethodCheckSample
        {
            //[ParameterNotNullCheck("")]
            //public void Simple(){}
            [ParameterNotNullCheck("p")]
            [ParameterNotNullCheck("P")]
            [ParameterNotNullCheck(0)]
            public void SimpleOneParameterString(string p)
            {
                Console.WriteLine("Called!");
            }
            [ParameterNotNullCheck("p1")]
            [ParameterNotNullCheck("P1")]
            [ParameterNotNullCheck(0)]
            [ParameterNotNullCheck("p2")]
            [ParameterNotNullCheck("P2")]
            [ParameterNotNullCheck(1)]
            public void TwoParameterObjectString(object p1, string p2)
            {
                Console.WriteLine("Called");
            }
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParameterValidated_NullValue_ThrowsException()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.SimpleOneParameterString(null);
            //assert
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParameterValidated_FirstNullValue_ThrowsException()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.TwoParameterObjectString(null, string.Empty);
            //assert
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParameterValidated_SecondNullValue_ThrowsException()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.TwoParameterObjectString(new object()  , null);
            //assert
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParameterValidated_BothNullValue_ThrowsException()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.TwoParameterObjectString(null, null);
            //assert
        }
        [Test]
        public void ParameterValidated_BothValue_Calls()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.TwoParameterObjectString(new object(), string.Empty);
            //assert
        }

        [Test]
        public void ParameterValidated_EmptyValue_ThrowsException()
        {
            //setup
            var m = new MethodCheckSample();

            //act
            m.SimpleOneParameterString(string.Empty);
            //assert
        }
        [Test][ExpectedException(typeof(NullReferenceException),UserMessage="Exception ok in this case, cannot set message for validation response")]
        public void CompileTimeValidation_NoParameter_ReturnsFalseSetsMessage()
        {
            //setup
            var attr = new ParameterNotNullCheckAttribute(9);
            //act
            var ret = attr.CompileTimeValidate(this.GetType().GetMethod("CompileTimeValidation_NoParameter_ReturnsFalseSetsMessage"));

            //assert
            Assert.That(ret, new FalseConstraint());
        }
        [Test]
        [ExpectedException(typeof(NullReferenceException), UserMessage = "Exception ok in this case, cannot set message for validation response")]
        public void CompileTimeValidation_NothingSet_ReturnsFalseSetsMessage()
        {
            //setup
            var attr = new ParameterNotNullCheckAttribute(9);
            attr.ParameterName = null;
            //act
            var ret = attr.CompileTimeValidate(this.GetType().GetMethod("CompileTimeValidation_NothingSet_ReturnsFalseSetsMessage"));

            //assert
            Assert.That(ret, new FalseConstraint());
        }
        [Test]
        [ExpectedException(typeof(NullReferenceException), UserMessage = "Exception ok in this case, cannot set message for validation response")]
        public void CompileTimeValidation_ParameterLessIndex_ReturnsFalseSetsMessage()
        {
            //setup
            var attr = new ParameterNotNullCheckAttribute(9);
            //act
            var ret = attr.CompileTimeValidate(typeof(MethodCheckSample).GetMethod("SimpleOneParameterString"));

            //assert
            Assert.That(ret, new FalseConstraint());
        }
    }
}
