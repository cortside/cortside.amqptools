using System;
using System.Collections.Generic;
using Cortside.Common.Messages;
using Cortside.Common.Messages.Filters;
using Cortside.AmqpTools.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cortside.AmqpTools.WebApi.Tests.Filters {
    public class MessageExceptionResponseFilterTest {

        private readonly MessageExceptionResponseFilter filter;
        public MessageExceptionResponseFilterTest() {
            filter = new MessageExceptionResponseFilter(new Logger<MessageExceptionResponseFilter>(new LoggerFactory()));
        }

        [Theory]
        [MemberData(nameof(GetPassThroughScenarios))]
        public void ShouldPassThroughCertainScenarios(ActionExecutedContext context) {
            // act
            filter.OnActionExecuted(context);

            // assert
            Assert.False(context.ExceptionHandled);
            Assert.Null(context.Result);
        }

        [Theory]
        [MemberData(nameof(GetCommonMessageExceptionScenarios))]
        public void ShouldWriteResponseForCommonMessageExceptions(MessageException message, Func<IActionResult, bool> comparison) {
            // arrange
            ActionExecutedContext context = GetActionExecutedContext();
            context.Exception = message;

            // act
            filter.OnActionExecuted(context);

            // assert
            Assert.True(context.ExceptionHandled);
            Assert.NotNull(context.Result);
            Assert.True(comparison(context.Result));
        }

        public static IEnumerable<object[]> GetCommonMessageExceptionScenarios() {
            yield return new object[] { new ResourceNotFoundMessage(), (Func<IActionResult, bool>)((IActionResult result) => result is NotFoundObjectResult) };
            yield return new object[] { new InvalidStateChangeMessage(), (Func<IActionResult, bool>)((IActionResult result) => ((ObjectResult)result).StatusCode == StatusCodes.Status422UnprocessableEntity) };
        }

        public static IEnumerable<object[]> GetPassThroughScenarios() {
            var successContext = GetActionExecutedContext();
            yield return new object[] { successContext };

            var exceptionContext = GetActionExecutedContext();
            exceptionContext.Exception = new InvalidOperationException();
            yield return new object[] { exceptionContext };
        }

        private static ActionExecutedContext GetActionExecutedContext() {
            var controllerMock = new Mock<Controller>();
            var filters = new List<IFilterMetadata>();
            var actionContext = new ActionContext() {
                HttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>().Object,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor() {
                    ActionName = "index",
                    ControllerName = "home"
                }
            };
            return new ActionExecutedContext(actionContext, filters, controllerMock.Object);
        }
    }
}
