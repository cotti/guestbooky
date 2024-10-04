using Guestbooky.API.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.API.Validations
{
    public class InvalidModelStateResponseFactoryTests
    {
        [Fact]
        public async Task DefaultInvalidModelStateResponse_InvalidActionContext_ReturnsBadRequest()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();
            var headers = new HeaderDictionary();

            httpResponseMock.SetupProperty(r => r.StatusCode);
            httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
            httpContextMock.SetupGet(h => h.Request).Returns(httpRequestMock.Object);
            httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);

            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            modelState.AddModelError("test", "test error");

            var actionContext = new ActionContext(httpContextMock.Object, 
                new Microsoft.AspNetCore.Routing.RouteData(), 
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(), 
                modelState);

            // Act
            var result = InvalidModelStateResponseFactory.DefaultInvalidModelStateResponse(actionContext);

            // Assert
            Assert.NotNull(result);
            
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("application/problem+json", objectResult.ContentTypes[0]);
            
            var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.NotNull(problemDetails);
            Assert.Equal("test error", problemDetails.Errors["test"][0]);
        }

        [Fact]
        public async Task DefaultInvalidModelStateResponse_BadRangeValue_ReturnsRequestedRangeNotSatisfiable()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();
            var headers = new HeaderDictionary();

            httpResponseMock.SetupProperty(r => r.StatusCode);
            httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
            httpContextMock.SetupGet(h => h.Request).Returns(httpRequestMock.Object);
            httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);

            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            modelState.AddModelError("Range.Range", "A test value is not valid for Range.");

            var actionContext = new ActionContext(httpContextMock.Object,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                modelState);

            // Act
            var result = InvalidModelStateResponseFactory.DefaultInvalidModelStateResponse(actionContext);

            // Assert
            Assert.NotNull(result);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(416, objectResult.StatusCode);
            Assert.Equal("application/problem+json", objectResult.ContentTypes[0]);

            var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.NotNull(problemDetails);
            Assert.Equal("A test value is not valid for Range.", problemDetails.Errors["Range.Range"][0]);
        }

        [Fact]
        public async Task DefaultInvalidModelStateResponse_NullRangeError_ReturnsNull()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();
            var headers = new HeaderDictionary();

            httpResponseMock.SetupProperty(r => r.StatusCode);
            httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
            httpContextMock.SetupGet(h => h.Request).Returns(httpRequestMock.Object);
            httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);

            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            modelState.AddModelError("Range.Range", "Range here would have been delivered as something invalid.");

            var actionContext = new ActionContext(httpContextMock.Object,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                modelState);

            // Act
            var result = InvalidModelStateResponseFactory.DefaultInvalidModelStateResponse(actionContext);

            // Assert
            Assert.Null(result);
        }
    }
}
