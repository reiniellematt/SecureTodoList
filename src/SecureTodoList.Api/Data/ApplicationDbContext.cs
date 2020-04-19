using Microsoft.EntityFrameworkCore;
using SecureTodoList.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTodoList.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
