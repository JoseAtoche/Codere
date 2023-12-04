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
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;

        public ShowsService(IMapper mapper)
        {
            _mapper = mapper;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.tvmaze.com")
            };
        }

        public async Task<bool> ImportShows(List<ShowDto> showInfoList)
        { 
            using var context= new ShowsRepository();
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
                context.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                // Algo salió mal, realiza un rollback
                Console.WriteLine($"Error: {ex.Message}");
                transactionNetwork.Rollback();
                context.Dispose();
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
            using var context = new ShowsRepository();

            var allData = await context.Shows
                .Include(show => show.Network)
                    .ThenInclude(network => network.Country)
                .Include(show => show.Schedule)
                .Include(show => show.Externals)
                .Include(show => show.Image)
                .Include(show => show.Link)
                .ToListAsync();
            context.Dispose();

            return allData.Select(_mapper.Map<ShowDto>).ToList();
        }

        public IActionResult GetDataByQuery(string sqlQuery)
        {
            using var context = new ShowsRepository();

            try
            {
                if (context.Shows.Count() == 0)
                {
                    return new BadRequestObjectResult(new { error = "La base de datos no tiene Datos, por favor, ejecute antes el metodo de importación: ShowsMainInformationAndImport" });
                }
                // Ejecutar la consulta en la base de datos
                var result = context.Shows.FromSqlRaw(sqlQuery).ToList();
                context.Dispose();

                return new OkObjectResult(result.Select(_mapper.Map<ShowDto>).ToList());
            }
            catch (Exception ex)
            {
                context.Dispose();

                return new BadRequestObjectResult(new { error = $"Error ejecutando Query: {ex.Message}" });
            }
        }
    }
}
