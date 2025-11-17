using AutoMapper;
using ClientsService.Models;
using Clients;

namespace ClientsService.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateClientRequest, Client>()
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => DateOnly.Parse(src.BirthDate)))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

        CreateMap<UpdateClientRequest, Client>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())

             // 1. Arreglo del Crash de Fecha: Solo parsear si NO es vacío
             .ForMember(dest => dest.BirthDate, opt =>
             {
                 opt.Condition(src => !string.IsNullOrEmpty(src.BirthDate));
                 opt.MapFrom(src => DateOnly.Parse(src.BirthDate));
             })

             // 2. Arreglo para no borrar datos: Solo mapear si tienen texto
             .ForMember(dest => dest.FirstName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.FirstName)))
             .ForMember(dest => dest.LastName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.LastName)))
             .ForMember(dest => dest.Email, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Email)))
             .ForMember(dest => dest.Username, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Username)))
             .ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Address)))
             .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PhoneNumber)));

        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate.ToString("o")))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
    }
}