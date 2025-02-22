using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Cortside.AmqpTools.WebApi.Tests {
    public class UnitTestFixture {
        private readonly List<Mock> mocks;
        protected Mock<IHttpContextAccessor> HttpContextAccessorMock { get; private set; } = new Mock<IHttpContextAccessor>();

        public UnitTestFixture() {
            mocks = new List<Mock>();
        }

        public Mock<T> Mock<T>() where T : class {
            var mock = new Mock<T>();
            this.mocks.Add(mock);
            return mock;
        }

        public void TearDown() {
            this.mocks.ForEach(m => m.VerifyAll());
        }
    }
}
