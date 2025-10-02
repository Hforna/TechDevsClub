using AutoMapper;
using Career.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.UnitTests.Mocks
{
    public static class MapperMock
    {
        public static IMapper GetMock()
        {
            return new MapperConfiguration(d => d.AddProfile(new ProjectMapperConfiguration())).CreateMapper();
        }
    }
}
