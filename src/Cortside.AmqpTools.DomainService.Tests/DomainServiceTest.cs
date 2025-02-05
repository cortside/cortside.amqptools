using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Cortside.AmqpTools.DomainService.Tests {
    public abstract class DomainServiceTest<T> : IDisposable {

        protected T Service { get; set; }
        protected UnitTestFixture TestFixture { get; set; }
        protected Mock<IHttpContextAccessor> HttpContextAccessorMock { get; private set; } = new Mock<IHttpContextAccessor>();
        protected DomainServiceTest() {
            TestFixture = new UnitTestFixture();

        }

        public void SetupHttpUser(Claim claim) {
            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            Mock<ClaimsPrincipal> user = new Mock<ClaimsPrincipal>();
            if (claim != null) {
                httpContext.SetupGet(x => x.User).Returns(user.Object);
                this.HttpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext.Object);
                user.Setup(x => x.FindFirst(claim.Type)).Returns(claim);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            TestFixture.TearDown();
        }
    }
}
