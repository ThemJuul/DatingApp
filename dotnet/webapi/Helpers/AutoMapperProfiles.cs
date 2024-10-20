﻿using AutoMapper;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Extensions;

namespace WebApi.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<User, MemberDto>()
            .ForMember(d => d.Age, o => 
                o.MapFrom(s => 
                    s.DateOfBirth.CalculateAge()))
            .ForMember(d => d.PhotoUrl, o => 
                o.MapFrom(s => 
                    s.Photos.FirstOrDefault(x => 
                        x.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, User>();
    }
}
