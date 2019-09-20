// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.SvcBroker.UnitTest
{
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.Common;

    /// <summary>
    ///This is a test class for RepeatableCallbackTrigger and is intended
    ///to contain all RepeatableCallbackTrigger Unit Tests
    ///</summary>
    [TestClass()]
    public class RepeatableCallbackTriggerTest
    {
        /// <summary>
        /// Stores the callback result
        /// </summary>
        private string callbackResult;

        /// <summary>
        /// Unit test for repeatable callback
        /// </summary>
        [TestMethod]
        public void RepeatableCallbackTest()
        {
            callbackResult = string.Empty;
            RepeatableCallbackTrigger trigger = new RepeatableCallbackTrigger();
            trigger.RegisterCallback(TimeSpan.FromMilliseconds(100), this.CallbackMethod, "1");
            trigger.Start();
            Thread.Sleep(350);
            Assert.AreEqual("111", callbackResult);
        }

        /// <summary>
        /// Method to receive callback
        /// </summary>
        /// <param name="state">async state object</param>
        private void CallbackMethod(object state)
        {
            callbackResult += state.ToString();
        }
    }
}
