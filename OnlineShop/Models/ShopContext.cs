
using BulderGoods.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IShop.Models
{
    public class ShopContext : DbContext
    {
        public DbSet<Category> categories { get; set; }
        public DbSet<Subcategory> subcategories { get; set; }
        public DbSet<Good> goods { get; set; }

        public DbSet<CartItem> ShoppingCartItems { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public ShopContext()
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Good>()
            .HasOne(p => p.Subcategory)
            .WithMany(t => t.goods);

            modelBuilder.Entity<Subcategory>()
            .HasOne(p => p.Category)
            .WithMany(t => t.subcategories);

            Category autogood = new Category { Id = 1, Name = "Автотовары" };
            Category wallpaper = new Category { Id = 2, Name = "Обои" };

            modelBuilder.Entity<Category>().HasData(
            new Category[]
            {
                autogood, wallpaper
            });

            Subcategory gps = new Subcategory { Id = 1, Name = "GPS навигаторы", CategoryId = autogood.Id };
            Subcategory autooils = new Subcategory { Id = 2, Name = "Автомасла", CategoryId = autogood.Id };
            Subcategory fluid_wallpaper = new Subcategory { Id = 3, Name = "Жидкие обои", CategoryId = wallpaper.Id };

            modelBuilder.Entity<Subcategory>().HasData(
            new Subcategory[]
            {
               gps, autooils, fluid_wallpaper
            });

            modelBuilder.Entity<Good>().HasData(
            new Good[]
            {
               new Good{Id=1, Name="НАВИГАТОР NAVITEL G550 MOTO", CountInStorage=4, Price=7.8, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/navigator.jpg"), Discount=0, SubcategoryId=1, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=2, Name="Масло компреc Gazpromneft 100л", CountInStorage=4, Price=1.3, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/oil100.jpg"), Discount=15, SubcategoryId=2, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=3, Name="Маслёнка рычажная 220мл. YATO", CountInStorage=4, Price=12.3, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/oilPackage220.jpg"), Discount=3, SubcategoryId=2, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=4, Name="Маслёнка рычажная 500мл. YATO", CountInStorage=4, Price=4.9, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/oilPackage500.jpg"), Discount=4, SubcategoryId=2, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=5, Name="Масло мот. 2-х тактное DIVINOL", CountInStorage=4, Price=33, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/oilDivinol.jpg"), Discount=3,  SubcategoryId=2, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=6, Name="Масло мот. 2-х тактное Лукойл", CountInStorage=4, Price=14.1, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/oil.jpg"), Discount=0, SubcategoryId=2, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=7, Name="ДЕКОРАТИВНАЯ ШТУКАТУРКА АРТ ДИ", CountInStorage=4, Price=5, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/dicorate.jpg"), Discount=0, SubcategoryId=3, Maker="Беларусь", Guarantee="1 мес."},
               new Good{Id=8, Name="ФОТООБОИ 4-006, 2700Х1940 ММ", CountInStorage=4, Price=4.3, Unit="шт", Image=ImgToBinnary("wwwroot/images/goods/wallpaper.jpg"),Discount=0, SubcategoryId=3, Maker="Беларусь", Guarantee="1 мес."},
            });
        }

        private byte[] ImgToBinnary(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
