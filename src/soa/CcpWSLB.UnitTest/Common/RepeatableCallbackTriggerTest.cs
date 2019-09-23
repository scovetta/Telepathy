// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Common
{
    using System;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            this.callbackResult = string.Empty;
            RepeatableCallbackTrigger trigger = new RepeatableCallbackTrigger();
            trigger.RegisterCallback(TimeSpan.FromMilliseconds(100), this.CallbackMethod, "1");
            trigger.Start();
            Thread.Sleep(350);
            Assert.AreEqual("111", this.callbackResult);
        }

        /// <summary>
        /// Method to receive callback
        /// </summary>
        /// <param name="state">async state object</param>
        private void CallbackMethod(object state)
        {
            this.callbackResult += state.ToString();
        }
    }
}
