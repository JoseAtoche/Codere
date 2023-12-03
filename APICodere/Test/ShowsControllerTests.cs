using System.Net;
using APICodere.Controllers;
using APICodere.Models.Models;
using APICodere.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace APICodere.Test
{
    [TestClass]
    public class ShowsControllerTests
    {
        private ShowsController _controller;
        private Mock<HttpClient> _httpClientMock;


        [TestInitialize]
        public void Initialize()
        {
            var dbContextMock = new Mock<ShowsRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var mapperMock = new Mock<IMapper>();

            _httpClientMock = new Mock<HttpClient>();

            _controller = new ShowsController(dbContextMock.Object, mapperMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection()
                            .AddScoped(_ => _httpClientMock.Object)
                            .BuildServiceProvider()
                    }
                }
            };
        }
        [TestMethod]
        public async Task GetShowsMainInformation_ConnectsToApiSuccessfully()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent("[{ \"id\": 1, \"name\": \"Show 1\" }]");
            _httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessage);

            // Act
            var result = await _controller.GetShowsMainInformation();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(List<Show>));
            var showList = (List<Show>)okResult.Value;
            Assert.AreEqual(1, showList.Count);
            Assert.AreEqual(1, showList[0].Id);

            // Verify that the controller made a request to the TVMaze API
            _httpClientMock.Verify(c => c.GetAsync("shows"), Times.Once);
        }

        [TestCleanup]
        public void Cleanup()
        {
            //_dbContext.Database.EnsureDeleted();
            //_dbContext.Dispose();
        }
    }
}
