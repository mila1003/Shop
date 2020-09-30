using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class GoodsController : Controller
    {
        const int pageSize = 16;
        const int pageSizeBanner = 4;
        ShopContext _context;
        public GoodsController(ShopContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index(int page, string search)
        {
            ViewBag.DiscountGoods = await _context.goods.Where(g => g.Discount != 0).ToListAsync();
            ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();

            var cart = ShoppingCart.GetCart(HttpContext);
            cart.context = _context;
            ViewBag.CartSum = cart.GetTotalCartSum();

            ViewBag.Page = page;
            if (search != "" && search != null)
            {
                List<Good> goods = await _context.goods.Where(g => g.Name.Contains(search)).ToListAsync();
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(goods.Count()) / pageSizeBanner);
                ViewBag.Goods = goods.Skip((page - 1) * pageSizeBanner)
                                                     .Take(pageSizeBanner).ToList();
                ViewBag.Search = search;
            }
            else 
            {
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.goods.CountAsync()) / pageSizeBanner);
                ViewBag.Goods = await _context.goods.Skip((page - 1) * pageSizeBanner)
                                                     .Take(pageSizeBanner)
                                                     .ToListAsync(); 
            }

            return View();
        }

        public async Task<ActionResult> Show(int page, int? subcategoryId, string search,
                                      string hasDiscount, int? fromDiscount, int? toDiscount,
                                      string sort, double? fromPrice, double? toPrice,
                                      string Filter)
        {
            if (Filter == "no")
            {
                var cart = ShoppingCart.GetCart(HttpContext);
                cart.context = _context;
                ViewBag.CartSum = cart.GetTotalCartSum();
                ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
                ViewBag.SubcategoryId = subcategoryId;

                //Search
                ViewBag.Search = "";
                //Only with discount
                ViewBag.HasDiscount = "";
                ViewBag.fromDiscount = "";
                ViewBag.toDiscount = "";
                //Sort
                ViewBag.Sort = "";
                //By price
                ViewBag.fromPrice = "";
                ViewBag.toPrice = "";

                ViewBag.Goods = await _context.goods.Where(g => g.SubcategoryId == subcategoryId)
                                                     .Skip((page - 1) * pageSize)
                                                     .Take(pageSize).ToListAsync();
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(await _context.goods.Where(g => g.SubcategoryId == subcategoryId).CountAsync()) / pageSize);
                return View();
            }
            else
            {
                List<Good> goods = new List<Good>();
                var cart = ShoppingCart.GetCart(HttpContext);
                cart.context = _context;
                ViewBag.CartSum = cart.GetTotalCartSum();
                ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
                ViewBag.SubcategoryId = subcategoryId;
                Subcategory subcategory = await _context.subcategories.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == subcategoryId);
                ViewBag.Category = subcategory.Category.Name;
                ViewBag.Subcategory = subcategory.Name;

                if (subcategoryId != null)
                {
                    goods = await _context.goods.Where(g => g.SubcategoryId == subcategoryId).ToListAsync();
                }
                if (search != "" && search != null)
                    goods = await _context.goods.Where(g => g.Name.Contains(search)).ToListAsync();
                //Search
                ViewBag.Search = search;
                //Only with discount
                ViewBag.HasDiscount = hasDiscount;
                ViewBag.fromDiscount = fromDiscount;
                ViewBag.toDiscount = toDiscount;
                if (hasDiscount == "true")
                    goods = goods.Where(g => g.Discount != 0).ToList();

                if (fromDiscount != null && toDiscount != null)
                    goods = goods.Where(g => g.Discount >= fromDiscount).Where(g => g.Discount <= toDiscount).ToList();
                //Sort
                ViewBag.Sort = sort;
                if (sort != null)
                {
                    if (sort == "down")
                        goods = goods.OrderBy(g => g.Price).ToList();
                    else
                        goods = goods.OrderByDescending(g => g.Price).ToList();
                }
                //Sort by price
                ViewBag.fromPrice = fromPrice;
                ViewBag.toPrice = toPrice;
                if (fromPrice != null && toPrice != null)
                    goods = goods.Where(g => g.Price >= fromPrice).Where(g => g.Price <= toPrice).ToList();
                //For paging 
                ViewBag.Goods = goods.Skip((page - 1) * pageSize)
                                                     .Take(pageSize).ToList();
                ViewBag.Page = page;
                ViewBag.countPages = Math.Ceiling(Convert.ToDouble(goods.Count()) / pageSize);
                return View();
            }
        }

        public async Task<ActionResult> MoreInfo(int id)
        {
            //TotalSumCart
            var cart = ShoppingCart.GetCart(HttpContext);
            cart.context = _context;
            ViewBag.CartSum = cart.GetTotalCartSum();
            //For showing the categories
            ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
            //The certain good by id
            Good good = await _context.goods.FirstOrDefaultAsync(g => g.Id == id);
            ViewBag.Good = await _context.goods.FirstOrDefaultAsync(g=>g.Id==id);
            return View();
        }
    }
}
