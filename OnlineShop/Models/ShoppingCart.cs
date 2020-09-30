using BulderGoods.Models;
using IShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Models
{
    public class ShoppingCart
    {
        public const string CartSessionKey = "CartId";
        public string ShoppingCartId { get; set; }

        public ShopContext context = new ShopContext();

        public static ShoppingCart GetCart(HttpContext HttpContext)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(HttpContext);
            return cart;
        }
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public string GetCartId(HttpContext HttpContext)
        {
            if (HttpContext.Session.GetString("CartSessionKey") == null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    HttpContext.Session.SetString("CartSessionKey", HttpContext.User.Identity.Name);
                }
                else
                {
                    Guid tempCartId = Guid.NewGuid();
                    HttpContext.Session.SetString("CartSessionKey", tempCartId.ToString());
                }
            }
            return HttpContext.Session.GetString("CartSessionKey");
        }

        public double GetTotalCartSum()
        {
            return Math.Round(context.shoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Sum(i => i.Count * Math.Round(i.Good.Price - (i.Good.Price * i.Good.Discount) / 100, 2)), 2);
        }
    }
}
