using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class InternalFieldFinderWithStaticFieldTests
    {
        private class StaticFieldTest
        {
            public static InternalFieldFinder finder;

            public StaticFieldTest()
            {
                StaticFieldTest.finder = null;
            }
            public MethodBase GetMe()
            {
                return MethodBase.GetCurrentMethod();
            }
        }
        [Test]
        public void GetStaticField_FieldNull()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new StaticFieldTest();

            var result = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            Assert.That(result, new NullConstraint());
        }
        [Test]
        public void GetStaticField_CallTwice_FieldNull()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new StaticFieldTest();

            var result = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            var result2 = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            Assert.That(result, new NullConstraint());
            Assert.That(result2 , new EqualConstraint(result));
        }
        [Test]
        public void GetStaticField_FieldValue()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new StaticFieldTest();
            StaticFieldTest.finder = test;

            var result = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            Assert.That(result, new NotConstraint(new NullConstraint()));
            Assert.That(result, new NUnit.Framework.Constraints.InstanceOfTypeConstraint(typeof(InternalFieldFinder)));
        }
        [Test]
        public void GetStaticField_CallTwice_FieldValue()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new StaticFieldTest();
            StaticFieldTest.finder = test;

            var result = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            var result2 = test.GetInstance<InternalFieldFinder>(item.GetMe(), null);
            Assert.That(result, new NotConstraint(new NullConstraint()));
            Assert.That(result, new NUnit.Framework.Constraints.InstanceOfTypeConstraint(typeof(InternalFieldFinder)));
            Assert.That(result2, new EqualConstraint(result));
        }
    }

    [TestFixture]
    public class InternalFieldFinderWithInstanceTests
    {
        private class InstanceFieldTest
        {
            public InstanceFieldTest finder;

            public InstanceFieldTest()
            {
                finder = null;
            }
            public MethodBase GetMe()
            {
                return MethodBase.GetCurrentMethod();
            }
        }
        [Test]
        public void GetField_FieldNull()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new InstanceFieldTest();

            var result = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            Assert.That(result, new NullConstraint());
        }
        [Test]
        public void GetField_CallTwice_FieldNull()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new InstanceFieldTest();

            var result = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            var result2 = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            Assert.That(result, new NullConstraint());
            Assert.That(result2, new EqualConstraint(result));
        }
        [Test]
        public void GetField_FieldValue()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new InstanceFieldTest();
            item.finder = new InstanceFieldTest();

            var result = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            Assert.That(result, new NotConstraint(new NullConstraint()));
            Assert.That(result, new NUnit.Framework.Constraints.InstanceOfTypeConstraint(typeof(InstanceFieldTest)));
        }
        [Test]
        public void GetField_CallTwice_FieldValue()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new InstanceFieldTest();
            item.finder = new InstanceFieldTest();

            var result = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            var result2 = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            Assert.That(result, new NotConstraint(new NullConstraint()));
            Assert.That(result, new NUnit.Framework.Constraints.InstanceOfTypeConstraint(typeof(InstanceFieldTest)));
            Assert.That(result2, new EqualConstraint(result));
        }
        [Test]
        public void GetField_CallTwice_DifferentInstances_FieldValues()
        {
            var test = InternalFieldFinder.Instance;
            test.Reset();
            var item = new InstanceFieldTest();
            item.finder = new InstanceFieldTest();
            var item2 = new InstanceFieldTest();
            item2.finder = new InstanceFieldTest();

            var result = test.GetInstance<InstanceFieldTest>(item.GetMe(), item);
            var result2 = test.GetInstance<InstanceFieldTest>(item.GetMe(), item2);
            Assert.That(result, new NotConstraint(new NullConstraint()));
            Assert.That(result, new NUnit.Framework.Constraints.InstanceOfTypeConstraint(typeof(InstanceFieldTest)));
            Assert.That(result2,new NotConstraint( new EqualConstraint(result)));
        }
    }
}
