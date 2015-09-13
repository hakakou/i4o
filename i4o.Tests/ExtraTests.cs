using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i4o;

namespace i4o.Tests
{
    [TestClass]
    public class ExtraTests
    {
        [TestMethod]
        public void TestElementClass()
        {
            var iComp = new IndexSet<TestClassB>(
                new TestClassB[] { 
                    new TestClassB() { T = 10 }, 
                    new TestClassB() { T = 20 }, 
                    new TestClassB() { T = 30 }, 
                    new TestClassB() { T = 40 } 
                },
                new IndexSpecification<TestClassB>().Add(q => q.T)); ;

            Assert.AreEqual(4, iComp.Where(c => c.T2 == 0).Count());

            bool failed = false;
            try
            {
                iComp.WhereIndexed(c => c.T2 == 0).Count();
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TestNonComparableIndex()
        {
            object o1 = "test";
            object o2 = 10;

            var iComp = new IndexSet<TestClassB>(
                new TestClassB[] { 
                    new TestClassB() { NonComparable = o1 },
                    new TestClassB() { NonComparable = o2 } 
                },
                new IndexSpecification<TestClassB>().Add(q => q.NonComparable));

            Assert.AreEqual(1, iComp.WhereIndexed(q => q.NonComparable == o1).Count());
            Assert.AreEqual(1, iComp.WhereIndexed(q => q.NonComparable == o2).Count());
        }

        [TestMethod]
        public void TestComparableIndex()
        {
            var iComp = new IndexSet<TestClassB>(
                new TestClassB[] { 
                    new TestClassB() { T = 10 },
                    new TestClassB() { T = 20 }, 
                    new TestClassB() { T = 30 }, 
                    new TestClassB() { T = 40 } 
                },
                new IndexSpecification<TestClassB>().Add(q => q.T));

            Assert.AreEqual(1, iComp.WhereIndexed(q => q.T == 20).Count());
            Assert.AreEqual(3, iComp.WhereIndexed(q => q.T >= 20).Count());
            Assert.AreEqual(2, iComp.WhereIndexed(q => q.T > 20).Count());
            Assert.AreEqual(2, iComp.WhereIndexed(q => q.T <= 20).Count());
            Assert.AreEqual(1, iComp.WhereIndexed(q => q.T < 20).Count());
        }
    }

    public class TestClassB
    {
        public int T { get; set; }
        public int T2 { get; set; }

        public Object NonComparable { get; set; }

        public int CompareTo(object obj)
        {
            return T - ((TestClassB)obj).T;
        }
    }
}
