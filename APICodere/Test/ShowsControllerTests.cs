using System;
using System.Net.Http;
using System.Threading.Tasks;
using APICodere.Controllers;
using APICodere.Mappings;
using APICodere.Models.Dtos;
using APICodere.Repository;
using APICodere.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;

namespace APICodere.Test
{
    [TestFixture]
    public class ShowsControllerTests
    {
        private ShowsController _showsController;
        private IMapper _mapper;
        private HttpClient _httpClient;
        IConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            var ApiURI = configuration.GetConnectionString("ApiConnection");

            _mapper = new Mapper(mapperConfiguration);
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ApiURI);
            ShowsService showsService = new ShowsService(_mapper);
            _showsController = new ShowsController(showsService);
            
        }

        [Test]
        public async Task GetShowsMainInformationAndImport_ShouldCreateRecordsInDatabase()
        {
            var result = await _showsController.GetShowsMainInformationAndImport() as ObjectResult;
            var jsonResult = JsonConvert.SerializeObject(result?.Value);

            Console.WriteLine("Result.Value: " + jsonResult);

            var numeroResultados = ((List<ShowDto>)result.Value).Count;

            Assert.That(numeroResultados, Is.GreaterThan(0));
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetShowById_ShouldReturnShowDto()
        {
            var showId = 1;
            var result = await _showsController.GetShowById(showId) as ObjectResult;
            var jsonResult = JsonConvert.SerializeObject(result?.Value);

            Console.WriteLine("Result.Value: " + jsonResult);

            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetAllData_ShouldReturnAllData()
        {
            // Debo hacer un import de los datos en caso de no tenerlo hecho
            await _showsController.GetShowsMainInformationAndImport();
            var result = await _showsController.GetAllData() as ObjectResult;
            var jsonResult = JsonConvert.SerializeObject(result?.Value);

            Console.WriteLine("Result.Value: " + jsonResult);

            var numeroResultados = ((List<ShowDto>)result.Value).Count;

            Assert.That(numeroResultados, Is.GreaterThan(0));
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [TearDown]
        public void TearDown()
        {
            // Este método se ejecutará al finalizar todas las pruebas en la clase


            string nombreArchivo = configuration.GetConnectionString("ShowsDatabase");
            string directorio = Environment.CurrentDirectory;
            string rutaCompleta = Path.Combine(directorio, nombreArchivo);

            if (File.Exists(rutaCompleta))
            {
                try
                {
                    using (var context = new ShowsRepository())
                    {
                        context.Database.EnsureDeleted();
                    }

                    Console.WriteLine($"Base de datos '{nombreArchivo}' eliminada al finalizar las pruebas.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar la base de datos: {ex.Message}");
                }
            }
        }
    }
}
