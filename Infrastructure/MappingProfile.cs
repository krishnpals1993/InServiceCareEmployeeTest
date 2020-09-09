using EmployeeTest.Models;
using AutoMapper;

namespace EmployeeTest.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EmployeeViewModel_datatable, EmployeeViewModel_datatable>()
                .ReverseMap();
         
        }
    }
}
