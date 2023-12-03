using Microsoft.AspNetCore.Mvc;
using APICodere.Repository;
using System.Text.Json;
using APICodere.Models.Models;
using APICodere.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace APICodere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private ShowsRepository _dbContext;
        private readonly IMapper _mapper;

        public ShowsController(ShowsRepository dbContext, IMapper mapper)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.tvmaze.com");
            _mapper = mapper;
            _dbContext = dbContext;


        }

        [HttpGet("ShowsMainInformationAndImport")]
        public async Task<IActionResult> GetShowsMainInformationAndImport()
        {

            try
            {
                var response = await _httpClient.GetAsync("shows");
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Intenta deserializar el JSON a la clase Shows
                        var showInfoList = await response.Content.ReadFromJsonAsync<List<Show>>();

                        var listadoShows = _mapper.Map<List<ShowDto>>(showInfoList);
                        try { 
                            if (Importacion(listadoShows))
                            {
                                return Ok(showInfoList);
                            }
                            }
                        catch (Exception ex) {
                            StatusCode((int)response.StatusCode, "Error importando Shows");

                        }

                    }
                    catch (JsonException jsonEx)
                    {
                        // Manejar la excepción específica de deserialización JSON
                        return BadRequest($"Error deserializing JSON: {jsonEx.Message}");
                    }
                }
                return StatusCode((int)response.StatusCode, "Se produjo un error al recuperar datos de la API de TVMaze");
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception: {ex.Message}");
            }
        }

        [HttpGet("show/{id}")]
        public async Task<IActionResult> GetShowById(int id)
        {
            var response = await _httpClient.GetAsync($"shows/{id}");

            if (response.IsSuccessStatusCode)
            {
                var show = await response.Content.ReadFromJsonAsync<Show>();
                var showdto = _mapper.Map<ShowDto>(show);


                return Ok(showdto);
            }

            return StatusCode((int)response.StatusCode, "Se produjo un error al recuperar datos de la API de TVMaze");
        }

        [HttpGet("showAllData")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllData()
        {
            var allData = await _dbContext.Shows
                .Include(show => show.Network)
                    .ThenInclude(network => network.Country)
                .Include(show => show.Schedule)
                .Include(show => show.Externals)
                .Include(show => show.Image)
                .Include(show => show.Link)
                .ToListAsync();

            return Ok(allData);

        }

        [HttpGet("GetDataByQuery")]
        public IActionResult GetDataByQuery([FromBody] string sqlQuery)
        {
            try
            {   
                if (_dbContext.Shows.Count() == 0) {

                    return BadRequest(new { error = "La base de datos no tiene Datos, por favor, ejecute antes el metodo de importación: ShowsMainInformationAndImport" });
                }
                // Ejecutar la consulta en la base de datos
                var result = _dbContext.Shows.FromSqlRaw(sqlQuery).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error executing query: {ex.Message}" });
            }
        }



        private bool Importacion(List<ShowDto> showInfoList)
        {
            using var context = _dbContext;
            using var transactionNetwork = context.Database.BeginTransaction();
            try
            {
                var showsOnBBDD = context.Shows.ToDictionary(s => s.Id);

                // Identificar cambios y filtrar los que necesitan ser actualizados o insertados
                var showsToInsertOrUpdate = showInfoList
                    .Select(dto =>
                    {
                        if (showsOnBBDD.TryGetValue(dto.Id, out var existingShow))
                        {
                            if (!existingShow.Equals(dto))
                            {
                                _mapper.Map(dto, existingShow);
                            }
                            return null;
                        }
                        return dto;
                    })
                    .Where(dto => dto != null)
                    .ToList();

                if (showsToInsertOrUpdate.Count == 0)
                {
                    return true;
                }

                // Filtra las redes únicas
                var uniqueNetworks = showsToInsertOrUpdate
                    .Where(show => show.Network != null)
                    .Select(show => show.Network)
                    .GroupBy(network => network.Id)
                    .Select(group => group.First())
                    .ToList();

                var uniqueCountries = showsToInsertOrUpdate.Select(x => x.Network)
                    .Where(network => network != null && network.Country != null)
                    .Select(network => network.Country)
                    .GroupBy(country => country.Code)
                    .Select(group => group.First())
                    .ToList();

                // Persiste las countries únicas
                context.Countries.AddRange(uniqueCountries.Select(country => _mapper.Map<CountryDto>(country)));
                context.SaveChanges();

                // Persiste las redes únicas
                context.Networks.AddRange(uniqueNetworks.Select(network =>
                {
                    var networkDto = _mapper.Map<NetworkDto>(network);
                    var matchingCountry = uniqueCountries.FirstOrDefault(c => c.Code == network.Country.Code);
                    if (matchingCountry != null)
                        networkDto.idCountry = matchingCountry.Id;
                    return networkDto;
                }));
                context.SaveChanges();

                // Persiste Schedule, Externals, Image y Link únicos
                context.Schedules.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Schedule != null)
                    .Select(show => show.Schedule)
                    .GroupBy(schedule => new { schedule.Time, Days = string.Join(",", schedule.Days) })
                    .Select(group => group.First())
                    .Select(schedule => _mapper.Map<ScheduleDto>(schedule)));
                context.SaveChanges();

                context.Externals.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Externals != null)
                    .Select(show => show.Externals)
                    .GroupBy(externals => new { externals.Tvrage, externals.Thetvdb, externals.Imdb })
                    .Select(group => group.First())
                    .Select(externals => _mapper.Map<ExternalsDto>(externals)));
                context.SaveChanges();

                context.Images.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Image != null)
                    .Select(show => show.Image)
                    .GroupBy(image => new { image.Medium, image.Original })
                    .Select(group => group.First())
                    .Select(image => _mapper.Map<ImageDto>(image)));
                context.SaveChanges();

                context.Links.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Link != null)
                    .Select(show => show.Link)
                    .GroupBy(link => new { link.SelfHref, link.PreviousepisodeHref })
                    .Select(group => group.First())
                    .Select(link => _mapper.Map<LinkDto>(link)));
                context.SaveChanges();

                // Persiste cada Show
                foreach (var show in showsToInsertOrUpdate)
                {
                    if (show.Network != null)
                    {
                        var matchingNetwork = uniqueNetworks.FirstOrDefault(n => n.Id == show.Network.Id);
                        if (matchingNetwork != null)
                            show.idNetwork = matchingNetwork.Id;
                    }

                    if (show.Schedule != null)
                    {
                        var matchingSchedule = context.Schedules
                            .AsEnumerable()
                            .FirstOrDefault(s => s.Time == show.Schedule.Time &&
                                                  s.Days.SequenceEqual(show.Schedule.Days));


                        if (matchingSchedule != null)
                            show.IdSchedule = matchingSchedule.Id;
                    }

                    if (show.Externals != null)
                    {
                        var matchingExternals = context.Externals
                            .FirstOrDefault(e =>
                                e.Tvrage == show.Externals.Tvrage && e.Thetvdb == show.Externals.Thetvdb &&
                                e.Imdb == show.Externals.Imdb);
                        if (matchingExternals != null)
                            show.idExternals = matchingExternals.Id;
                    }

                    if (show.Image != null)
                    {
                        var matchingImage = context.Images
                            .FirstOrDefault(i => i.Medium == show.Image.Medium && i.Original == show.Image.Original);
                        if (matchingImage != null)
                            show.idImage = matchingImage.Id;
                    }

                    if (show.Link != null)
                    {
                        var matchingLink = context.Links
                            .FirstOrDefault(l =>
                                l.SelfHref == show.Link.SelfHref && l.PreviousepisodeHref == show.Link.PreviousepisodeHref);
                        if (matchingLink != null)
                            show.idLink = matchingLink.Id;
                    }
                    show.Network = null;
                    context.Shows.AddRange(show);

                }

                // Guardar cambios una vez fuera del bucle
                context.SaveChanges();

                // Confirma la transacción
                transactionNetwork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Algo salió mal, realiza un rollback
                Console.WriteLine($"Error: {ex.Message}");
                transactionNetwork.Rollback();

                return false;
            }
        }
    }
}
