using EmployeeTest.Contracts;
using EmployeeTest.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Infrastructure;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeTest.Services
{
    public class DefaultDemoService : IDemoService
    {
        private readonly DbContext _context;
        private readonly IConfigurationProvider _mappingConfiguration;

        public DefaultDemoService(DbContext context, IConfigurationProvider mappingConfiguration)
        {
            _context = context;
            _mappingConfiguration = mappingConfiguration;
        }

    }
}
