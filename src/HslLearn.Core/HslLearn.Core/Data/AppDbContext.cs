using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslLearn.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HslLearn.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<DeviceDataLog> DeviceLogs { get; set; }
    }
}
