using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;

namespace DotVVM.Samples.MiniProfiler.AspNetCore.Models
{
    public class SampleContext : DbContext
    {
        public SampleContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
    }
}
