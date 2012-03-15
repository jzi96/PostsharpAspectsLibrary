using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class ApplyDataContractAttributeTests
    {
        //[Test]
        //public void Test()
        //{
        //    var t = typeof(DataContractAll);
        //    var dc = t.GetCustomAttributes(typeof(DataContractAttribute), true);
        //    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);//.GetFields(BindingFlags.SetField | BindingFlags.Instance);
        //    foreach (var field in fields)
        //    {
        //        Console.WriteLine(field.Name);
        //    }
        //}
        [AddDataContract]
        public class DataContractAll
        {
            public string AutoProp { get; set; }
            private string _withField;
            public string WithField
            {
                get { return _withField; }
                set { _withField = value; }
            }

            [NoDataMember]
            public object NoDataMember { get; set; }

            private static long _nomemberfield ;
        }
        [Test]
        public void AttributeAppliedToBackingFieldAndNormalField()
        {
            //setup, act run in compiler
            //assert
            var t = typeof(DataContractAll);
            var dc = t.GetCustomAttributes(typeof(DataContractAttribute), true);
            Assert.That(dc, new NotConstraint(new NullConstraint()));

            var fields = t.GetFields();
            foreach (var field in fields)
            {
                Console.WriteLine("Checking field " + field.Name);
                dc = field.GetCustomAttributes(typeof(DataMemberAttribute), true);
                if (field.IsStatic || field.IsDefined(typeof(NoDataMemberAttribute), true))
                    Assert.That(dc, new NullConstraint());
                else
                    Assert.That(dc, new NotConstraint(new NullConstraint()));
            }
        }
    }
}
