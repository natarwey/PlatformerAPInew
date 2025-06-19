using PlatformerAPI.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PlatformerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
