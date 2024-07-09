using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cortside.AmqpTools.WebApi.Tests {
    public abstract class ControllerTest<T> : IDisposable where T : ControllerBase {

        protected T Controller { get; set; }
        protected UnitTestFixture TestFixture { get; set; }

        protected ControllerTest() {
            TestFixture = new UnitTestFixture();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            TestFixture.TearDown();
        }

        protected ControllerContext GetControllerContext() {
            var controllerContext = new ControllerContext();
            controllerContext.HttpContext = new DefaultHttpContext();
            return controllerContext;
        }
    }
}
