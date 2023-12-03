using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using APICodere.Controllers;
using APICodere.Mappings;
using APICodere.Models.Dtos;
using APICodere.Models.Models;
using APICodere.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace APICodere.Test
{
    [TestClass]
    public class ShowsControllerTests
    {
        private ShowsController _showsController;
        private ShowsRepository _repository;
        private IMapper _mapper;
        private HttpClient _httpClient;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ShowsRepository>()
                   .UseInMemoryDatabase(databaseName: "Tvshows.sqlite")
                   .Options;
            _repository = new ShowsRepository(options);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = new Mapper(mapperConfiguration);
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.tvmaze.com");

            // Crear el controlador con las dependencias configuradas
            _showsController = new ShowsController(_repository, _mapper);
        }

        [TestMethod]
        public async Task GetShowsMainInformationAndImport_ShouldCreateRecordsInDatabase()
        {           

            // Act
            var result = await _showsController.GetShowsMainInformationAndImport() as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

        }

        [TestMethod]
        public async Task GetShowById_ShouldReturnShowDto()
        {
            // Arrange
            var showId = 1;            

            var result = await _showsController.GetShowById(showId) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

        }

        [TestMethod]
        public async Task GetAllData_ShouldReturnAllData()
        {          

            // Act
            var result = await _showsController.GetAllData() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

        }

       
    }
}
