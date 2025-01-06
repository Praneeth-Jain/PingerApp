using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PingerApp.Data.Entity;
using PingerApp.Model;

namespace PingerApp.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }
        public DbSet<PingRecord> PingRecords { get; set; } 
        public DbSet<IPAdresses> IPadresses { get; set; }
    }
}
