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
        private readonly IConfiguration configuration;


        public ShowsService(IMapper mapper)
        {
            configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            var ApiURI = configuration.GetConnectionString("ApiConnection");

            _mapper = mapper;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiURI)
            };
        }

        public async Task<bool> ImportShows(List<ShowDto> showInfoList)
        { 
            using var context= new ShowsRepository();
            using var transactionNetwork = context.Database.BeginTransaction();

            try
            {
                var showsOnBBDD = GetAllShows(context).ToDictionary(s => s.Id);

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

                //Aquí cogemos todas las countries unicas para persistirlas
                var uniqueCountries = showsToInsertOrUpdate.Select(x => x.Network)
                    .Where(network => network != null && network.Country != null)
                    .Select(network => network.Country)
                    .GroupBy(country => country.Code)
                    .Select(group => group.First())
                    .ToList();

                // Persiste las countries únicas que no estén en BBDD aún
                PersistOrUpdateEntities(uniqueCountries, context.Countries, country => new { country.Name, country.Code, country.Timezone }, context);

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
                PersistOrUpdateEntities(showsToInsertOrUpdate.Select(show => show.Schedule), context.Schedules, schedule => new { schedule.Time, Days = string.Join(",", schedule.Days) }, context);
                PersistOrUpdateEntities(showsToInsertOrUpdate.Select(show => show.Externals), context.Externals, externals => new { externals.Tvrage, externals.Thetvdb, externals.Imdb }, context);
                PersistOrUpdateEntities(showsToInsertOrUpdate.Select(show => show.Image), context.Images, image => new { image.Medium, image.Original }, context);
                PersistOrUpdateEntities(showsToInsertOrUpdate.Select(show => show.Link), context.Links, link => new { link.SelfHref, link.PreviousepisodeHref }, context);


                // Persiste cada Show
                var networkIds = uniqueNetworks.ToDictionary(network => network.Id);
                var scheduleIds = context.Schedules.ToDictionary(schedule => new { schedule.Time, Days = string.Join(",", schedule.Days) }, schedule => schedule.Id);
                var externalsIds = context.Externals.ToDictionary(externals => new { externals.Tvrage, externals.Thetvdb, externals.Imdb }, externals => externals.Id);
                var imagesIds = context.Images.ToDictionary(image => new { image.Medium, image.Original }, image => image.Id);
                var linksIds = context.Links.ToDictionary(link => new { link.SelfHref, link.PreviousepisodeHref }, link => link.Id);

                foreach (var show in showsToInsertOrUpdate)
                {
                    if (show.Network != null && networkIds.TryGetValue(show.Network.Id, out var networkId))
                    {
                        show.idNetwork = networkId.Id;
                    }

                    if (show.Schedule != null && scheduleIds.TryGetValue(new { show.Schedule.Time, Days = string.Join(",", show.Schedule.Days) }, out var scheduleId))
                    {
                        show.IdSchedule = scheduleId;
                    }

                    if (show.Externals != null && externalsIds.TryGetValue(new { show.Externals.Tvrage, show.Externals.Thetvdb, show.Externals.Imdb }, out var externalsId))
                    {
                        show.idExternals = externalsId;
                    }

                    if (show.Image != null && imagesIds.TryGetValue(new { show.Image.Medium, show.Image.Original }, out var imageId))
                    {
                        show.idImage = imageId;
                    }

                    if (show.Link != null && linksIds.TryGetValue(new { show.Link.SelfHref, show.Link.PreviousepisodeHref }, out var linkId))
                    {
                        show.idLink = linkId;
                    }

                    show.Network = null;
                    context.Shows.Add(show);
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
            if (context.Shows.Count() == 0)
            {
                return null;
            }
            var allData = await GetAllShows(context).ToListAsync();

            context.Dispose();

            return allData.Select(_mapper.Map<ShowDto>).ToList();
        }

        public async Task<IActionResult> GetDataByQuery(string sqlQuery)
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

        private IQueryable<ShowDto> GetAllShows(ShowsRepository context) {

            return context.Shows
                    .Include(show => show.Network)
                        .ThenInclude(network => network.Country)
                    .Include(show => show.Schedule)
                    .Include(show => show.Externals)
                    .Include(show => show.Image)
                    .Include(show => show.Link);

        }
        private void PersistOrUpdateEntities<T>(IEnumerable<T> entities, DbSet<T> dbSet, Func<T, object> keySelector, ShowsRepository context) where T : class
        {
            var uniqueEntities = entities.Where(entity => entity != null)
                .GroupBy(keySelector)
                .Select(group => group.First())
                .ToList();

            var existingEntities = dbSet
                .ToDictionary(keySelector, entity => entity);

            foreach (var uniqueEntity in uniqueEntities)
            {
                if (existingEntities.TryGetValue(keySelector(uniqueEntity), out var existingEntity))
                {
                    _mapper.Map(uniqueEntity, existingEntity);
                }
                else
                {
                    dbSet.Add(_mapper.Map<T>(uniqueEntity));
                }
            }

            context.SaveChanges();
        }
    }
}
