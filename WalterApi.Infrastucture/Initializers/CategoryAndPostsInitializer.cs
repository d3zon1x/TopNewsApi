using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalterApi.Core.Entities.Category;

namespace WalterApi.Infrastucture.Initializers
{
    internal static  class CategoryAndPostsInitializer
    {
        public static void SeedCategories(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(new Category[]
            {
                new Category() { Id = 1, Name = "Sport"},
                new Category() { Id = 2, Name = "IT"},
            });
        }
    }
}
