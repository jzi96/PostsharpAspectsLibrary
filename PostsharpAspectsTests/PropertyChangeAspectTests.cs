using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using System.ComponentModel;
using NUnit.Framework.Constraints;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    public struct NotificationData
    {
        private string _PropertyName;
        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }
            set
            {
                _PropertyName = value;
            }
        }
        private object _PropertyValue;
        public object PropertyValue
        {
            get
            {
                return _PropertyValue;
            }
            set
            {
                _PropertyValue = value;
            }
        }
        private long _TimeOfCall;
        public long TimeOfCall
        {
            get
            {
                return _TimeOfCall;
            }
            set
            {
                _TimeOfCall = value;
            }
        }
        public NotificationData(string prop, object value)
        {
            this._PropertyName = prop;
            this._PropertyValue = value;
            this._TimeOfCall = DateTime.UtcNow.Ticks;
        }
    }

    /// <summary>
    /// in same assmebly you need to implement interface for some reason
    /// </summary>
    [NotifyPropertyChanged]
    [NotifyPropertyChanging]
    public class CheckNotificationInTest : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
    }

    [TestFixture]
    public class PropertyChangeAspectTests
    {
        private NotificationData _changedProperty;
        private NotificationData _changingProperty;

        private void ChangingName(object sender, PropertyChangingEventArgs e)
        {
            this._changingProperty = new NotificationData(e.PropertyName, ((CheckNotification)sender).Name);
        }
        private void ChangedName(object sender, PropertyChangedEventArgs e)
        {
            this._changedProperty = new NotificationData(e.PropertyName, ((CheckNotification)sender).Name);
        }
        private void ChangingNameLocal(object sender, PropertyChangingEventArgs e)
        {
            this._changingProperty = new NotificationData(e.PropertyName, ((CheckNotificationInTest)sender).Name);
        }
        private void ChangedNameLocal(object sender, PropertyChangedEventArgs e)
        {
            this._changedProperty = new NotificationData(e.PropertyName, ((CheckNotificationInTest)sender).Name);
        }
        [Test]
        public void PropertyValueChange_CallsBoth()
        {
            _changedProperty = new NotificationData();
            _changingProperty = new NotificationData();
            //setup
            CheckNotification not = new CheckNotification();
            not.PropertyChanged += new PropertyChangedEventHandler(ChangedName);
            not.PropertyChanging += new PropertyChangingEventHandler(ChangingName);
            //act
            not.Name = "Test";
            //assert
            Assert.That(_changingProperty.PropertyName, new EqualConstraint("Name"));
            Assert.That(_changedProperty.PropertyName, new EqualConstraint("Name"));
            Assert.That(_changingProperty.PropertyValue, new EqualConstraint(null));
            Assert.That(_changedProperty.PropertyValue, new EqualConstraint("Test"));
            Assert.That(_changingProperty.TimeOfCall, new LessThanConstraint(_changedProperty.TimeOfCall), "First changing must be called, afterwards changed");
        }
        [Test]
        public void InTest_PropertyValueChange_CallsBoth()
        {
            _changedProperty = new NotificationData();
            _changingProperty = new NotificationData();
            //setup
            CheckNotificationInTest not = new CheckNotificationInTest();
            not.PropertyChanged += new PropertyChangedEventHandler(ChangedNameLocal);
            not.PropertyChanging += new PropertyChangingEventHandler(ChangingNameLocal);
            //act
            not.Name = "Test";
            //assert
            Assert.That(_changingProperty.PropertyName, new EqualConstraint("Name"));
            Assert.That(_changedProperty.PropertyName, new EqualConstraint("Name"));
            Assert.That(_changingProperty.PropertyValue, new EqualConstraint(null));
            Assert.That(_changedProperty.PropertyValue, new EqualConstraint("Test"));
            Assert.That(_changingProperty.TimeOfCall, new LessThanConstraint(_changedProperty.TimeOfCall), "First changing must be called, afterwards changed");
        }
        [Test]
        public void PropertyValueMatchesPrev_NoCall()
        {
            _changedProperty = new NotificationData();
            _changingProperty = new NotificationData();
            //setup
            CheckNotification not = new CheckNotification();
            not.Name = "Test";
            not.PropertyChanging += new PropertyChangingEventHandler(ChangingName);
            not.PropertyChanged += new PropertyChangedEventHandler(ChangedName);
            //act
            not.Name = "Test";
            //assert
            Assert.That(_changingProperty.PropertyName, new EqualConstraint(null));
            Assert.That(_changedProperty.PropertyName, new EqualConstraint(null));
            Assert.That(_changingProperty.PropertyValue, new EqualConstraint(null));
            Assert.That(_changedProperty.PropertyValue, new EqualConstraint(null));
            Assert.That(_changedProperty.TimeOfCall, new EqualConstraint(0));
            Assert.That(_changingProperty.TimeOfCall, new EqualConstraint(0));
        }
    }
}
