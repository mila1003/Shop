using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using IShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;


namespace OnlineShop.Controllers
{
    [Authorize]
    public class AdministrationController : Controller
    {
        const int pageSize = 10;
        ShopContext _context;
        public AdministrationController(ShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string message)
        {
            Admin admin =  await _context.admins.FirstOrDefaultAsync();
            ViewBag.Email = admin.Email;
            ViewBag.Message = message != null ? message : "";
            return View();
        }

        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            Admin admin = await _context.admins.FirstOrDefaultAsync(a=>a.Password==oldPassword);
            if(admin == null)
            {
                return Redirect("Index?message=Пароль не совпадает");
            }
            else
            {
                if (newPassword==null)
                {
                    BadRequest();
                }
                admin.Password = newPassword;
                await _context.SaveChangesAsync();
                return Redirect("Index?message=Пароль изменен");
            }
        }

        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            Admin admin = await _context.admins.FirstOrDefaultAsync();
            if (admin == null || newEmail == null)
            {
                return Redirect("Index?message=Ошибка");
            }
            else
            {
                admin.Email = newEmail;
                await _context.SaveChangesAsync();
                return Redirect("Index?message=Почта изменена");
            }
        }


        public async Task<ActionResult<IEnumerable<Category>>> Categories(int page, string search)
        {
            if (search != "" && search != null)
            {
                //Search
                ViewBag.Search = search;
                //Paging
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.categories.CountAsync()) / pageSize);
                return View(await _context.categories.Where(g => g.Name.Contains(search))
                                                     .Skip((page - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToListAsync());
            }
            
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.categories.CountAsync()) / pageSize);
                return View(await _context.categories.Skip((page - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToListAsync());
        }

        public async Task<IActionResult> UpdateCategory(long id, string Name)
        {
            Category category = await _context.categories
                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (category==null)
            {
                return NotFound();
            }
            if (category.Name != Name && Name != null)
            {
                category.Name = Name;
                await _context.SaveChangesAsync();
                return Redirect("Categories?page=1");
            }
            else
            return View(category);
        }

        public async Task<IActionResult> DeleteCategory(long id)
        {
            Category category = await _context.categories
                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                BadRequest();
            }
            _context.categories.Remove(category);
            await _context.SaveChangesAsync();
            return Redirect("Categories?page=1");
        }

        public async Task<IActionResult> CreateCategory(string Name)
        {
            if (Name != null)
            {
                await _context.categories.AddAsync(new Category { Name = Name });
                await _context.SaveChangesAsync();

                return Redirect("Categories?page=1");
            }
            else return View();
        }


        public async Task<ActionResult<IEnumerable<Subcategory>>> Subcategories(int page, string search)
        {
            if (search != "" && search != null)
            {
                List<Subcategory> subcategories = await (from Subcategory in _context.subcategories
                                                         where Subcategory.Name.Contains(search)
                                                               || Subcategory.Category.Name.Contains(search)
                                                         select Subcategory)
                                                         .Include(s => s.Category)
                                                         .ToListAsync();
                //Search
                ViewBag.Search = search;
                //Paging
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(subcategories.Count()) / pageSize);
                return View(subcategories.Skip((page - 1) * pageSize)
                                             .Take(pageSize));
            }
            
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.subcategories.CountAsync()) / pageSize);
                return View(await _context.subcategories.Include(s => s.Category)
                                                     .Skip((page - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToListAsync());
        }

        public async Task<IActionResult> UpdateSubcategory(long id, string Name, long categoryId)
        {
            Subcategory subcategory = await _context.subcategories
                                      .FirstOrDefaultAsync(s => s.Id == id);
            if (subcategory == null)
            {
                return NotFound();
            }
            if (Name != null)
            {
                if(subcategory.Name != Name)
                subcategory.Name = Name;
                if (subcategory.CategoryId != categoryId)
                    subcategory.Category = await _context.categories
                                                 .FirstOrDefaultAsync(c => c.Id == categoryId);
                await _context.SaveChangesAsync();
                return Redirect("Subcategories?page=1");
            }
            else
            {
                ViewBag.Subcategory = subcategory;
                return View(await _context.categories.ToListAsync());
            }
        }

        public async Task<IActionResult> DeleteSubcategory(long id)
        {
            Subcategory subcategory = await _context.subcategories
                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (subcategory == null)
            {
                BadRequest();
            }
            _context.subcategories.Remove(subcategory);
            await _context.SaveChangesAsync();
            return Redirect("Subcategories?page=1");
        }

        public async Task<IActionResult> CreateSubcategory(string Name, long categoryId)
        {
            if (Name != null)
            {
                Category checkedCategory = await _context.categories
                                           .FirstOrDefaultAsync(c => c.Id == categoryId);
                await _context.subcategories.AddAsync(
                      new Subcategory { Name = Name, Category = checkedCategory }
                      );
                await _context.SaveChangesAsync();

                return Redirect("Subcategories?page=1");
            }
            else 
                return View(await _context.categories.ToListAsync());
        }


        public async Task<ActionResult<IEnumerable<Good>>> Goods(int page, string search)
        {
            if (search != "" && search != null)
            {
                List<Good> goods = await (from Good in _context.goods
                                                         where Good.Name.Contains(search)
                                                               || Good.Subcategory.Name.Contains(search)
                                                               || Good.Subcategory.Category.Name.Contains(search)
                                                         select Good)
                                                         .Include(g => g.Subcategory)
                                                         .ThenInclude(s=> s.Category)
                                                         .ToListAsync();
                //Search
                ViewBag.Search = search;
                //Paging
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(goods.Count()) / pageSize);
                return View(goods.Skip((page - 1) * pageSize)
                                             .Take(pageSize));
            }

            ViewBag.Page = page;
            ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.goods.CountAsync()) / pageSize);
            return View(await _context.goods.Include(g => g.Subcategory)
                                            .ThenInclude(s=>s.Category)
                                            .Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync());
        }

        public async Task<IActionResult> UpdateGood(long id,
                                                    string Name, long subcategoryId,
                                                    int? Discount, string Unit, 
                                                    string Price, int? CountInStorage,
                                                    string Maker, string Guarantee,
                                                    string Note, IFormFile Image)
        {
            Good good = await _context.goods
                                      .FirstOrDefaultAsync(g => g.Id == id);
            if (good == null)
            {
                return NotFound();
            }
            if (Name != null)
            {
                if (good.Name != Name) good.Name = Name;
                if (good.SubcategoryId != subcategoryId)
                    good.Subcategory = await _context.subcategories
                                                 .FirstOrDefaultAsync(s => s.Id == subcategoryId);
                if (good.Unit != Unit) good.Unit = Unit;
                if (good.Price != Math.Round(Convert.ToDouble(Price.Replace(".", ",")), 2))
                    good.Price = Math.Round(Convert.ToDouble(Price.Replace(".", ",")), 2);
                if (good.Discount != Discount)
                    good.Discount = Convert.ToInt32(Discount);
                if (good.CountInStorage != CountInStorage)
                    good.CountInStorage = Convert.ToInt32(CountInStorage);
                if (good.Guarantee != Guarantee) good.Guarantee = Guarantee;
                if (good.Maker != Maker) good.Maker = Maker;
                if (good.Note != Note) good.Note = Note;
                if (Image != null)
                {
                    byte[] imageData = null;
                    // считываем переданный файл в массив байтов
                    using (var binaryReader = new BinaryReader(Image.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)Image.Length);
                    }
                    good.Image = imageData;
                }
                await _context.SaveChangesAsync();
                return Redirect("Goods?page=1");
            }
            else
            {
                ViewBag.Good = good;
                return View(await _context.categories.Include(c=>c.subcategories).ToListAsync());
            }
        }

        public async Task<IActionResult> DeletGood(long id)
        {
            Good good = await _context.goods
                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (good == null)
            {
                BadRequest();
            }
            _context.goods.Remove(good);
            await _context.SaveChangesAsync();
            return Redirect("Goods?page=1");
        }

        public async Task<IActionResult> CreateGood(string Name, long subcategoryId,
                                                    int? Discount, string Unit, 
                                                    string Price, int? CountInStorage, 
                                                    string Maker, string Guarantee,
                                                    string Note, IFormFile Image)
        {
            if (Name != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(Image.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)Image.Length);
                }

                Subcategory checkedSubategory = await _context.subcategories
                                           .Include(s=>s.Category)
                                           .FirstOrDefaultAsync(s => s.Id == subcategoryId);
                await _context.goods.AddAsync(
                      new Good
                      {
                          Name = Name,
                          Subcategory = checkedSubategory,
                          Unit = Unit,
                          Price = Math.Round(Convert.ToDouble(Price.Replace(".", ",")), 2),
                          Discount = Convert.ToInt32(Discount),
                          CountInStorage = Convert.ToInt32(CountInStorage),
                          Maker = Maker,
                          Guarantee = Guarantee,
                          Image = imageData,
                          Note = Note
                      }
                      );
                ;
                await _context.SaveChangesAsync();

                return Redirect("Goods?page=1");
            }
            else
                return View(await _context.categories.Include(c=>c.subcategories).ToListAsync());
        }
    }
}
