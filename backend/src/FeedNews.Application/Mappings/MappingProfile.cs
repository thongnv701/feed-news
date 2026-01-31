using AutoMapper;
using FeedNews.Application.UseCases.Accounts.Models;
using FeedNews.Domain.Entities;

namespace FeedNews.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountResponse>();
    }
}