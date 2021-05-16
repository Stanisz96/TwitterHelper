using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Data
{
    public class TwitterContext : DbContext
    {
        public TwitterContext(DbContextOptions<TwitterContext> options) : base(options)
        {
        }

        public DbSet<TwitterObject> TwitterObjects { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
    }
}
