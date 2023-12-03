using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using APICodere.Models.Dtos;
using APICodere.Models.Models;
using APICodere.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICodere.Services
{
    public class ShowsService
    {
        private readonly ShowsRepository _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;

        public ShowsService(ShowsRepository dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.tvmaze.com")
            };
        }

        public async Task<bool> ImportShows(List<ShowDto> showInfoList)
        {
            using var transactionNetwork = _dbContext.Database.BeginTransaction();

            try
            {
                var showsOnBBDD = _dbContext.Shows.ToDictionary(s => s.Id);

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
                _dbContext.Countries.AddRange(uniqueCountries.Select(country => _mapper.Map<CountryDto>(country)));
                _dbContext.SaveChanges();

                // Persiste las redes únicas
                _dbContext.Networks.AddRange(uniqueNetworks.Select(network =>
                {
                    var networkDto = _mapper.Map<NetworkDto>(network);
                    var matchingCountry = uniqueCountries.FirstOrDefault(c => c.Code == network.Country.Code);
                    if (matchingCountry != null)
                        networkDto.idCountry = matchingCountry.Id;
                    return networkDto;
                }));
                _dbContext.SaveChanges();

                // Persiste Schedule, Externals, Image y Link únicos
                _dbContext.Schedules.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Schedule != null)
                    .Select(show => show.Schedule)
                    .GroupBy(schedule => new { schedule.Time, Days = string.Join(",", schedule.Days) })
                    .Select(group => group.First())
                    .Select(schedule => _mapper.Map<ScheduleDto>(schedule)));
                _dbContext.SaveChanges();

                _dbContext.Externals.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Externals != null)
                    .Select(show => show.Externals)
                    .GroupBy(externals => new { externals.Tvrage, externals.Thetvdb, externals.Imdb })
                    .Select(group => group.First())
                    .Select(externals => _mapper.Map<ExternalsDto>(externals)));
                _dbContext.SaveChanges();

                _dbContext.Images.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Image != null)
                    .Select(show => show.Image)
                    .GroupBy(image => new { image.Medium, image.Original })
                    .Select(group => group.First())
                    .Select(image => _mapper.Map<ImageDto>(image)));
                _dbContext.SaveChanges();

                _dbContext.Links.AddRange(showsToInsertOrUpdate
                    .Where(show => show.Link != null)
                    .Select(show => show.Link)
                    .GroupBy(link => new { link.SelfHref, link.PreviousepisodeHref })
                    .Select(group => group.First())
                    .Select(link => _mapper.Map<LinkDto>(link)));
                _dbContext.SaveChanges();

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
                        var matchingSchedule = _dbContext.Schedules
                            .AsEnumerable()
                            .FirstOrDefault(s => s.Time == show.Schedule.Time &&
                                                  s.Days.SequenceEqual(show.Schedule.Days));


                        if (matchingSchedule != null)
                            show.IdSchedule = matchingSchedule.Id;
                    }

                    if (show.Externals != null)
                    {
                        var matchingExternals = _dbContext.Externals
                            .FirstOrDefault(e =>
                                e.Tvrage == show.Externals.Tvrage && e.Thetvdb == show.Externals.Thetvdb &&
                                e.Imdb == show.Externals.Imdb);
                        if (matchingExternals != null)
                            show.idExternals = matchingExternals.Id;
                    }

                    if (show.Image != null)
                    {
                        var matchingImage = _dbContext.Images
                            .FirstOrDefault(i => i.Medium == show.Image.Medium && i.Original == show.Image.Original);
                        if (matchingImage != null)
                            show.idImage = matchingImage.Id;
                    }

                    if (show.Link != null)
                    {
                        var matchingLink = _dbContext.Links
                            .FirstOrDefault(l =>
                                l.SelfHref == show.Link.SelfHref && l.PreviousepisodeHref == show.Link.PreviousepisodeHref);
                        if (matchingLink != null)
                            show.idLink = matchingLink.Id;
                    }
                    show.Network = null;
                    _dbContext.Shows.AddRange(show);
                }

                // Guardar cambios una vez fuera del bucle
                _dbContext.SaveChanges();

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

        public async Task<List<ShowDto>> GetShowsMainInformation()
        {
            try
            {
                var response = await _httpClient.GetAsync("shows");
                if (response.IsSuccessStatusCode)
                {
                    var showInfoList = await response.Content.ReadFromJsonAsync<List<Show>>();
                    var listadoShows = _mapper.Map<List<ShowDto>>(showInfoList);

                    return listadoShows;
                }

                return null; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Show> GetShowById(int id)
        {
            var response = await _httpClient.GetAsync($"shows/{id}");

            if (response.IsSuccessStatusCode)
            {
                var show = await response.Content.ReadFromJsonAsync<Show>();
                return show;
            }

            return null;
        }

        public async Task<List<ShowDto>> GetAllData()
        {
            var allData = await _dbContext.Shows
                .Include(show => show.Network)
                    .ThenInclude(network => network.Country)
                .Include(show => show.Schedule)
                .Include(show => show.Externals)
                .Include(show => show.Image)
                .Include(show => show.Link)
                .ToListAsync();

            return allData.Select(_mapper.Map<ShowDto>).ToList();
        }

        public IActionResult GetDataByQuery(string sqlQuery)
        {
            try
            {
                if (_dbContext.Shows.Count() == 0)
                {
                    return new BadRequestObjectResult(new { error = "La base de datos no tiene Datos, por favor, ejecute antes el metodo de importación: ShowsMainInformationAndImport" });
                }
                // Ejecutar la consulta en la base de datos
                var result = _dbContext.Shows.FromSqlRaw(sqlQuery).ToList();
                return new OkObjectResult(result.Select(_mapper.Map<ShowDto>).ToList());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { error = $"Error ejecutando Query: {ex.Message}" });
            }
        }
    }
}
