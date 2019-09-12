using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Hpc.ServiceBroker;

namespace Microsoft.Hpc.SvcBroker.UnitTest
{


    /// <summary>
    ///This is a test class for ThreadHelperTest and is intended
    ///to contain all ThreadHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ThreadHelperTest
    {


        private TestContext testContextInstance;

        private AutoResetEvent wait = new AutoResetEvent(false);

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void ThreadHelperTest1()
        {
            ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(this.WaitCallbackMethod)).CallbackRoot, "STATE");
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.WaitCallbackMethod), "STATE");
            if (!wait.WaitOne(1000))
            {
                Assert.Fail();
            }
        }

        private void WaitCallbackMethod(object state)
        {
            Assert.AreEqual(state, "STATE");
            wait.Set();
        }
    }
}
