using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalterApi.Core.DTO_s.User;
using WalterApi.Core.Entities.User;

namespace WalterApi.Core.AutoMapper.User
{
    public class AutoMapperUserProfile : Profile
    {
        public AutoMapperUserProfile()
        {
            CreateMap<UsersDto, AppUser>().ReverseMap();
            CreateMap<EditUserDTO, AppUser>().ForMember(dest => dest.Id, opt => opt.Ignore()).ReverseMap();
            CreateMap<CreateUserDTO, AppUser>().ForMember(dst => dst.UserName, act => act.MapFrom(src => src.Email));
            CreateMap<AppUser, CreateUserDTO>();
        }
    }
}
