using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using BulderGoods.Models;
using IShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Controllers
{
    public class ShopController : Controller
    {
        public const string CartSessionKey = "CartId";
        public string ShoppingCartId { get; set; }
        public static double TotalCartSum;

        ShopContext _context;

        public ShopController(ShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.CartSum = TotalCartSum;
            ViewBag.Categories = _context.categories.Include(c => c.subcategories).ToList();
            //Goods with discount
            ViewBag.DiscountGoods = _context.goods.Where(g => g.Discount != 0).ToList();
            return View();
        }

        public ActionResult AddToCart(int id, int count)
        {
            if (count == 0)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                ShoppingCartId = GetCartId();
                var cartItem = _context.ShoppingCartItems.SingleOrDefault(
                    c => c.CartId == ShoppingCartId
                    && c.GoodId == id);

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        ItemId = Guid.NewGuid().ToString(),
                        GoodId = id,
                        CartId = ShoppingCartId,
                        Good = _context.goods.SingleOrDefault(
                                p => p.Id == id),
                        DateCreated = DateTime.Now,
                        Count = count
                    };
                    _context.ShoppingCartItems.Add(cartItem);

                    //Decrease the good count in a storage
                    Good goodAdded = _context.goods.FirstOrDefault(g => g.Id == id);
                    goodAdded.CountInStorage = goodAdded.CountInStorage - (int)count;
                }
                else
                {
                    cartItem.Count++;
                }
                _context.SaveChanges();
                TotalCartSum = GetTotalCartSum();
                return RedirectToAction("GetCartItems");
            }
        }

        public ActionResult RemoveFromCart(string id)
        {
            ShoppingCartId = GetCartId();
            var cartItem = _context.ShoppingCartItems.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.ItemId == id);
                _context.ShoppingCartItems.Remove(cartItem);
            //Increase the good count in a storage
            Good goodRemoved = _context.goods.FirstOrDefault(g => g.Id == cartItem.GoodId);
            goodRemoved.CountInStorage = goodRemoved.CountInStorage +1;

            _context.SaveChanges();
            TotalCartSum = GetTotalCartSum();
            return RedirectToAction("GetCartItems");
        }

        public ActionResult SendToEmail(string InOrder,
                                        string Surname, string Name, string SecondName, 
                                        string Tel, string Email, string Address, string Deliver)
        {
            string emailFrom = "";
            string emailTo = "";
            var senderEmail = new MailAddress(emailFrom);
            var receiverEmail = new MailAddress(emailTo);
            var emailPassword = "";

            var sub = "Новый заказ";
            var body = $"Оформлен заказ от {DateTime.Now} на имя: {Surname} {Name} {SecondName} \n" +
                       $"Состав заказа: \n";

            //Get all goods from the cart
            ShoppingCartId = GetCartId(); 
            var cart = _context.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Include(i => i.Good).ToList();

            double totalsum = 0;
            string[] ItemsToOrder = InOrder.Split('?');
            foreach (var x in cart)
            {
                for (int i = 0; i < ItemsToOrder.Length; i++)
                {
                    //if the good is selected to buy then include the good the the order
                    if (x.ItemId == ItemsToOrder[i])
                    {
                        _context.ShoppingCartItems.Remove(x);
                            _context.SaveChanges();
                            totalsum += Math.Round(x.Count * (x.Good.Price - ((x.Good.Price * x.Good.Discount) / 100)), 2);
                            body += $"Товар: {x.Good.Name} Кол-во: {x.Count} Цена: {x.Good.Price} Скидка: {x.Good.Discount} \n";
                    }
                }
            }

            Deliver = Deliver == "car" ? "доставка" : "самовывоз";
            body += $"Общая сумма заказа: {Math.Round(totalsum, 2)} \n" +
                    $"Контакты заказика: почта {Email}, телефон {Tel} \n" +
                    $"Место доставки {Address}. Тип доставки: {Deliver}";

            var smtp = new SmtpClient
            {
                Host = "smtp.mail.ru",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, emailPassword)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = sub,
                Body = body
            })
            {
                smtp.Send(mess);
            }

            TotalCartSum = GetTotalCartSum();
            ViewBag.CartSum = TotalCartSum;
            return View();
        }

        //To get selected goods from the cart
        public ActionResult MakeOrder(string InOrder)
        {
            ViewBag.Categories = _context.categories.Include(c => c.subcategories).ToList();
            ViewBag.InOrder = InOrder;
            ViewBag.CartSum = TotalCartSum;
            return View();
        }

        //Show the goods from the cart
        public ActionResult GetCartItems()
        {
            ViewBag.Categories = _context.categories.Include(c => c.subcategories).ToList();
            ShoppingCartId = GetCartId();
            ViewBag.CartSum = TotalCartSum;
            ViewBag.sumCart = TotalCartSum;

            //Getting goods from the cart
            ViewBag.CartItems = _context.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Include(i=>i.Good).ToList();
            return View();
        }

        public string GetCartId()
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

        public  double GetTotalCartSum()
        {
            ShoppingCartId = GetCartId();
            return Math.Round(_context.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Sum(i=>i.Count*Math.Round( i.Good.Price- (i.Good.Price* i.Good.Discount) /100 ,2)),2);
        }
    }
}
