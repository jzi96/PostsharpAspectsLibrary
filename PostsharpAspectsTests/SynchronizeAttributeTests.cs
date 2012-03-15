using System;
using System.Diagnostics;
using System.Threading;
using Zieschang.Net.Projects.PostsharpAspects.Aspects;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Zieschang.Net.Projects.PostsharpAspects.Tests
{
    [TestFixture]
    public class SynchronizeAttributeTests
    {
        private class SynchronizedFake
        {
            public ManualResetEvent CallCompleted { get; set; }
            public bool ChildCalled { get; set; }

            public SynchronizedFake()
            {
                CallCompleted = new ManualResetEvent(false);
            }
            [TypeSyncronize]
            public void Run(Action action)
            {
                if (action != null)
                {
                    Console.WriteLine("Invoking nested");
                    var asyncResult = action.BeginInvoke(ar => { action.EndInvoke(ar); }, null);
                    asyncResult.AsyncWaitHandle.WaitOne();
                }
                CallCompleted.Set();
            }
            [TypeSyncronize]
            public void Locked()
            {
                Console.WriteLine("Locked called");
                ChildCalled = true;
            }
            [TypeSyncronize(100)]
            public void LockedWithTimeout()
            {
                Console.WriteLine("LockedWithTimeout called");
                ChildCalled = true;
            }
            public void NoLock()
            {
                Console.WriteLine("NoLock called");
                ChildCalled = true;
            }
        }
        private class SynchronizedFake2
        {
            public bool ChildCalled { get; set; }
            [TypeSyncronize]
            public void Locked()
            {
                Console.WriteLine("SynchronizedFake2.Locked called");
                ChildCalled = true;
            }
        }
        [Test]
        public void SameType_Blocks()
        {
            var fake = new SynchronizedFake();
            var t = new Thread(new ThreadStart(() => fake.Run(() => fake.Locked())));
            t.Start();
            while (!t.IsAlive) { Thread.Sleep(10); }
            if (fake.CallCompleted.WaitOne(2000))
            {
                //normal end
                Assert.Fail("Should be blocked!");
            }
            t.Abort();
            Assert.That(fake.ChildCalled, new FalseConstraint());
        }
        [Test]
        public void SameTypeWithTimeout_ReturnsWithoutCall()
        {
            var fake = new SynchronizedFake();
            var t = new Thread(new ThreadStart(() => fake.Run(() => fake.LockedWithTimeout())));
            t.Start();
            while (!t.IsAlive) { Thread.Sleep(10); }
            if (!fake.CallCompleted.WaitOne(5000))
            {
                //normal end
                Assert.Fail("Should not be blocked!");
            }
            Assert.That(fake.ChildCalled, new FalseConstraint());
        }
        [Test]
        public void SameTypeNoLockInChild_ReturnsWithCall()
        {
            var fake = new SynchronizedFake();
            var t = new Thread(new ThreadStart(() => fake.Run(() => fake.NoLock())));
            t.Start();
            while (!t.IsAlive) { Thread.Sleep(10); }
            if (!fake.CallCompleted.WaitOne(5000))
            {
                //normal end
                Assert.Fail("Should not be blocked!");
            }
            Assert.That(fake.ChildCalled, new TrueConstraint());
        }
        [Test]
        public void DifferentTypes2Locks_ReturnsWithCall()
        {
            var fake = new SynchronizedFake();
            var fake2 = new SynchronizedFake2();
            var t = new Thread(new ThreadStart(() => fake.Run(() => fake2.Locked())));
            t.Start();
            while (!t.IsAlive) { Thread.Sleep(10); }
            if (!fake.CallCompleted.WaitOne(5000))
            {
                //normal end
                Assert.Fail("Should not be blocked!");
            }
            Assert.That(fake.ChildCalled, new FalseConstraint());
            Assert.That(fake2.ChildCalled, new TrueConstraint());
        }
    }
}