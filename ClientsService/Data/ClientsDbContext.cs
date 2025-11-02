using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientsService.Models;
using Microsoft.EntityFrameworkCore;
namespace ClientsService.Data
{
    public class ClientsDbContext(DbContextOptions<ClientsDbContext> options) : DbContext(options)
    {
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var client = modelBuilder.Entity<Client>();
            client.Property(c => c.Id).ValueGeneratedOnAdd();
            client.HasIndex(c => c.Email).IsUnique();
            client.HasIndex(c => c.Username).IsUnique();
            // Global Filter for Soft Delete
            client.HasQueryFilter(c => c.Status == "active");
        }
    }
}