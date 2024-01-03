using Application.Contract.Level.Commands;
using Application.Contract.Level.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Levels;

public class LevelMapperProfile : Profile
{
    public LevelMapperProfile()
    {
        CreateMap<CreateLevelCommand, Level>();
        CreateMap<Level, LevelResponse>();
    }
}