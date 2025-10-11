using Microsoft.EntityFrameworkCore;
using Sistema_Gestion_Llaves.Models;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    }
}
