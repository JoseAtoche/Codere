using AutoMapper;
using Models.ShowModels.ExternalAPI;
using Models.ShowModels.Repository;
using Models.ShowModels.Service;

namespace Utils
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<ServiceShowModel, ShowDto>()
                        .ForMember(dest => dest.Runtime, opt => opt.MapFrom(src => src.Runtime ?? 0))
                        .ForMember(dest => dest.AverageRuntime, opt => opt.MapFrom(src => src.AverageRuntime ?? 0))
                        .ForMember(dest => dest.Ended, opt => opt.MapFrom(src => src.EndedDate ?? DateTime.MinValue))
                        .ForMember(dest => dest.Rating, opt =>
                        {
                            opt.MapFrom(src => src.Rating != null ? src.Rating.Average : 0);
                        }).ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.Schedule))
                        .ForMember(dest => dest.idNetwork, opt => opt.MapFrom(src => src.Network.Id))
                        .ForMember(dest => dest.WebChannel, opt => opt.MapFrom(src => src.WebChannel ?? string.Empty))
                        .ForMember(dest => dest.DvdCountry, opt => opt.MapFrom(src => src.DvdCountry ?? string.Empty))
                        .ForMember(dest => dest.OfficialSite, opt => opt.MapFrom(src => src.OfficialSite ?? string.Empty))
                        .ForMember(dest => dest.Externals, opt => opt.MapFrom(src => src.Externals))
                        .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                        .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src._links));

            CreateMap<ServiceShowModel.Scheduleclass, ScheduleDto>();
            CreateMap<ServiceShowModel.Countryclass, CountryDto>();
            CreateMap<ServiceShowModel.Networkclass, NetworkDto>()
                .ForMember(dest => dest.OfficialSite, opt => opt.MapFrom(src => src.OfficialSite ?? string.Empty));
            CreateMap<ServiceShowModel.Externalsclass, ExternalsDto>()
                .ForMember(dest => dest.Imdb, opt => opt.MapFrom(src => src.Imdb ?? string.Empty)); ;
            CreateMap<ServiceShowModel.Imageclass, ImageDto>();
            CreateMap<ServiceShowModel.Linkclass, LinkDto>()
                .ForMember(dest => dest.SelfHref, opt => opt.MapFrom(src => src.Self.Href))
                .ForMember(dest => dest.PreviousepisodeHref, opt => opt.MapFrom(src => src.Previousepisode.Href));



            CreateMap<ExternalAPIShowModel, ShowDto>()
                        .ForMember(dest => dest.Runtime, opt => opt.MapFrom(src => src.Runtime ?? 0))
                        .ForMember(dest => dest.AverageRuntime, opt => opt.MapFrom(src => src.AverageRuntime ?? 0))
                        .ForMember(dest => dest.Ended, opt => opt.MapFrom(src => src.EndedDate ?? DateTime.MinValue))
                        .ForMember(dest => dest.Rating, opt =>
                        {
                            opt.MapFrom(src => src.Rating != null ? src.Rating.Average : 0);
                        }).ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.Schedule))
                        .ForMember(dest => dest.idNetwork, opt => opt.MapFrom(src => src.Network.Id))
                        .ForMember(dest => dest.WebChannel, opt => opt.MapFrom(src => src.WebChannel ?? string.Empty))
                        .ForMember(dest => dest.DvdCountry, opt => opt.MapFrom(src => src.DvdCountry ?? string.Empty))
                        .ForMember(dest => dest.OfficialSite, opt => opt.MapFrom(src => src.OfficialSite ?? string.Empty))
                        .ForMember(dest => dest.Externals, opt => opt.MapFrom(src => src.Externals))
                        .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                        .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src._links));

            CreateMap<ExternalAPIShowModel.Scheduleclass, ScheduleDto>();
            CreateMap<ExternalAPIShowModel.Countryclass, CountryDto>();
            CreateMap<ExternalAPIShowModel.Networkclass, NetworkDto>()
                .ForMember(dest => dest.OfficialSite, opt => opt.MapFrom(src => src.OfficialSite ?? string.Empty));
            CreateMap<ExternalAPIShowModel.Externalsclass, ExternalsDto>()
                .ForMember(dest => dest.Imdb, opt => opt.MapFrom(src => src.Imdb ?? string.Empty)); ;
            CreateMap<ExternalAPIShowModel.Imageclass, ImageDto>();
            CreateMap<ExternalAPIShowModel.Linkclass, LinkDto>()
                .ForMember(dest => dest.SelfHref, opt => opt.MapFrom(src => src.Self.Href))
                .ForMember(dest => dest.PreviousepisodeHref, opt => opt.MapFrom(src => src.Previousepisode.Href));


            CreateMap<ExternalAPIShowModel, ServiceShowModel>();



        }

    }
}
