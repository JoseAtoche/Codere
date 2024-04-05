using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models.ShowModels.Repository;
using Models.ShowModels.Service;
using Models.ShowModels.ExternalAPI;

using Repository;
using System.Net.Http.Json;
using Models.Enumerados;

namespace Services
{
    public class ShowsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IConfiguration configuration;
        private readonly bool BBDDModeSQLLite;



        public ShowsService(IMapper mapper)
        {
            configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            var ApiURI = configuration.GetConnectionString("ApiConnection");
            BBDDModeSQLLite = configuration.GetConnectionString("ConnectOption") == Enumerados.SQLConexion.SQLLite.ToString();
            _mapper = mapper;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiURI)
            };
        }

        public async Task<bool> ImportShows(List<ShowDto> showInfoList)
        {
            using var context = new ShowsRepository();
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
                PersistOrUpdateEntities(uniqueCountries, context.Countries, context, country => new { country.Name, country.Code, country.Timezone });

                // Persiste las redes únicas
                context.Networks.AddRange(uniqueNetworks.Select(network =>
                {
                    var networkDto = _mapper.Map<NetworkDto>(network);
                    var matchingCountry = uniqueCountries.FirstOrDefault(c => c.Code == network.Country.Code);
                    if (matchingCountry != null)
                        networkDto.idCountry = matchingCountry.Id;
                    return networkDto;
                }));

                SaveChanges(context, "Network");

                // Persiste cada Show
                var networkIds = uniqueNetworks.ToDictionary(network => network.Id);


                foreach (var show in showsToInsertOrUpdate)
                {
                    if (show.Network != null && networkIds.TryGetValue(show.Network.Id, out var networkId))
                    {
                        show.idNetwork = networkId.Id;
                     }

                    if (show.Schedule != null)
                    {
                        PersistAndAssignId<ScheduleDto>(show.Schedule, context.Schedules, dto => show.IdSchedule = dto.Id, context);
                    }

                    if (show.Externals != null)
                    {
                        PersistAndAssignId<ExternalsDto>(show.Externals, context.Externals, dto => show.idExternals = dto.Id, context);
                    }

                    if (show.Image != null)
                    {
                        PersistAndAssignId<ImageDto>(show.Image, context.Images, dto => show.idImage = dto.Id, context);
                    }

                    if (show.Link != null)
                    {
                        PersistAndAssignId<LinkDto>(show.Link, context.Links, dto => show.idLink = dto.Id, context);
                    }

                    show.Network = null;
                    context.Shows.Add(show);
                }

                // Guardar cambios una vez fuera del bucle

                SaveChanges(context, "Show");



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
                    var showInfoList = await response.Content.ReadFromJsonAsync<List<ExternalAPIShowModel>>();
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

        public async Task<ServiceShowModel> GetShowById(int id)
        {
            var response = await _httpClient.GetAsync($"shows/{id}");

            if (response.IsSuccessStatusCode)
            {
                var show = await response.Content.ReadFromJsonAsync<ServiceShowModel>();
                return show;
            }

            return null;
        }

        public async Task<List<ServiceShowModel>> GetAllData()
        {
            using var context = new ShowsRepository();
            if (context.Shows.Count() == 0)
            {
                return null;
            }
            var allData = await GetAllShows(context).ToListAsync();

            context.Dispose();

            return allData.Select(_mapper.Map<ServiceShowModel>).ToList();
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

                return new OkObjectResult(result.Select(_mapper.Map<ServiceShowModel>).ToList());
            }
            catch (Exception ex)
            {
                context.Dispose();

                return new BadRequestObjectResult(new { error = $"Error ejecutando Query: {ex.Message}" });
            }
        }

        private IQueryable<ShowDto> GetAllShows(ShowsRepository context)
        {
            IQueryable<ShowDto> shows = context.Shows
                    .Include(show => show.Network)
                        .ThenInclude(network => network.Country)
                     .Include(show => show.Schedule)
                    .Include(show => show.Externals)
                    .Include(show => show.Image)
                    .Include(show => show.Link);
            return shows;

        }
        private void PersistOrUpdateEntities<T>(IEnumerable<T> entities, DbSet<T> dbSet, ShowsRepository context, Func<T, object> keySelector = null) where T : class
        {
            if (keySelector == null)
            {
                // Si no se proporciona keySelector, simplemente agregamos todas las entidades sin filtrar duplicados
                dbSet.AddRange(entities);
            }
            else
            {
                var uniqueEntities = entities
                    .Where(entity => entity != null)
                    .GroupBy(keySelector)
                    .Select(group => group.First())
                    .ToList();

                var existingEntities = dbSet
                    .ToDictionary(keySelector, entity => entity);

                foreach (var uniqueEntity in uniqueEntities)
                {
                    var keyValue = keySelector(uniqueEntity);

                    if (existingEntities.TryGetValue(keyValue, out var existingEntity))
                    {
                        _mapper.Map(uniqueEntity, existingEntity);
                    }
                    else
                    {
                        dbSet.Add(_mapper.Map<T>(uniqueEntity));
                    }
                }
            }

            SaveChanges(context, "Show");

        }


        private void PersistAndAssignId<TDto>(object sourceEntity, DbSet<TDto> dbSet, Action<TDto> assignIdAction, ShowsRepository context) where TDto : class
        {
            var dto = _mapper.Map<TDto>(sourceEntity);
            dbSet.Add(dto);

            SaveChanges(context, "Show");

            assignIdAction(dto);
        }


        private void SaveChanges(ShowsRepository context,string table ) {
            if (BBDDModeSQLLite)
            {
                context.SaveChanges();
            }
            else {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT " + table + " ON");

                context.SaveChanges();

                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT  " + table + "  OFF");
            }
        }
    }
}
