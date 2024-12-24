using E_CommerceCoreMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace E_CommerceCoreMVC.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel mac = new CategoryModel { Name = "Macbool", Slug = "Macbool", Description = "Macbool is fucking delicious", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "Pc", Slug = "Pc", Description = "Pc is what the heck", Status = 1 };

                BrandModel apple = new BrandModel { Name = "Apple", Slug = "Apple", Description = "Apple is what the heck", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung is what the heck", Status = 1 };
                 
                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Slug = "Macbook", Description = "Macbook is the best", Image = "1.jpg", Category = mac,Brand=apple ,Price = 1233 },
                    new ProductModel { Name = "Samsung", Slug = "Samsung", Description = "Samsung is the Second", Image = "1.jpg", Category = pc,Brand=samsung ,Price = 1233 }
                );
            }
            _context.SaveChanges();
        }
    }
}
