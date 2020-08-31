using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Controllers
{
    public class GoodsController : Controller
    {
        const int pageSize = 4;
        ShopContext _context;
        public GoodsController(ShopContext context)
        {
            _context = context;
        }

        private List<Good> PagedGoods(int startPage, int pageSize, List<Good> goods, out int CountPages)
        {
            CountPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(goods.Count()) / Convert.ToDouble(pageSize)));
            List<Good> result = new List<Good>();
            if (startPage <= CountPages && CountPages != 0)
            {
                for (int i = 1; i < pageSize + 1; i++)
                {
                    if (startPage * pageSize - i >= goods.Count) continue;
                    else
                        result.Add(goods[startPage * pageSize - i]);
                }
                return result;
            }
            else return result;

        }

        public ActionResult Show(int page,int? subcategoryId, string search,
                                      string hasDiscount, int? fromDiscount, int? toDiscount,
                                      string sort, double? fromPrice, double? toPrice)
        {
            List<Good> goods = new List<Good>();
            ViewBag.CartSum = ShopController.TotalCartSum;
            ViewBag.Categories = _context.categories.Include(c => c.subcategories).ToList();
            ViewBag.SubcategoryId = subcategoryId;
            if (subcategoryId != null)
            {
                goods = _context.goods.Where(g => g.SubcategoryId == subcategoryId).ToList();
            }
            if (search != "" && search != null)
                goods = _context.goods.Where(g => g.Name.Contains(search)).ToList();
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
            int pageCount = 0;
            ViewBag.Goods = PagedGoods(page, pageSize, goods, out pageCount);
            ViewBag.Page = page;
            ViewBag.countPages = pageCount;
            return View();
        }

        public ActionResult MoreInfo(int id)
        {
            //TotalSumCart
            ViewBag.CartSum = ShopController.TotalCartSum;
            //For showing the categories
            ViewBag.Categories = _context.categories.Include(c => c.subcategories).ToList();
            //The certain good by id
            ViewBag.Good = _context.goods.Find(id);
            return View();
        }
    }
}
