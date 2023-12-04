using System;
using System.Net.Http;
using System.Threading.Tasks;
using APICodere.Controllers;
using APICodere.Mappings;
using APICodere.Repository;
using APICodere.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace APICodere.Test
{
    [TestFixture]
    public class ShowsControllerTests
    {
        private ShowsController _showsController;
        private ShowsRepository _repository;
        private IMapper _mapper;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = new Mapper(mapperConfiguration);
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.tvmaze.com");
            _repository = new ShowsRepository();
            _repository.Database.EnsureCreated();
            ShowsService showsService = new ShowsService(_repository, _mapper);
            _showsController = new ShowsController(showsService);
        }

        [Test]
        public async Task GetShowsMainInformationAndImport_ShouldCreateRecordsInDatabase()
        {
            var result = await _showsController.GetShowsMainInformationAndImport() as ObjectResult;
            var jsonResult = JsonConvert.SerializeObject(result?.Value);

            Console.WriteLine("Result.Value: " + jsonResult);
            Assert.That(result.Value, Is.Not.Null);
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
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        //[TearDown]
        //public void TearDown()
        //{
        //    // Este método se ejecutará al finalizar todas las pruebas en la clase

        //    string directorio = Environment.CurrentDirectory;
        //    string nombreArchivo = "Tvshows.sqlite";
        //    string rutaCompleta = Path.Combine(directorio, nombreArchivo);

        //    if (File.Exists(rutaCompleta))
        //    {
        //        try
        //        {
        //            File.Delete(rutaCompleta);
        //            Console.WriteLine($"Archivo '{nombreArchivo}' eliminado al finalizar las pruebas.");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error al eliminar el archivo: {ex.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine($"El archivo '{nombreArchivo}' no existe en el directorio '{directorio}'.");
        //    }
        //}
    }
}
