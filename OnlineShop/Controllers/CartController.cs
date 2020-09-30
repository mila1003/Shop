using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BulderGoods.Models;
using IShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        public const string CartSessionKey = "CartId";
        public string ShoppingCartId { get; set; }
        //public static double TotalCartSum;

        ShopContext _context;

        public CartController(ShopContext context)
        {
            _context = context;
        }

        //Show the goods from the cart
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
            ShoppingCartId = GetCartId();
            ViewBag.CartSum = GetTotalCartSum();

            //Getting goods from the cart
            ViewBag.CartItems = await _context.shoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Include(i => i.Good).ToListAsync();
            return View();
        }

        public async Task<ActionResult> Add(int id, int count)
        {
            if (count == 0)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                ShoppingCartId = GetCartId();
                var cartItem = await _context.shoppingCartItems.SingleOrDefaultAsync(
                    c => c.CartId == ShoppingCartId
                    && c.GoodId == id);

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        ItemId = Guid.NewGuid().ToString(),
                        GoodId = id,
                        CartId = ShoppingCartId,
                        Good = await _context.goods.SingleOrDefaultAsync(
                                p => p.Id == id),
                        DateCreated = DateTime.Now,
                        Count = count
                    };
                    await _context.shoppingCartItems.AddAsync(cartItem);

                    //Decrease the good count in a storage
                    Good goodAdded = await _context.goods.FirstOrDefaultAsync(g => g.Id == id);
                    goodAdded.CountInStorage = goodAdded.CountInStorage - (int)count;
                }
                else
                {
                    cartItem.Count++;
                }
                await _context.SaveChangesAsync();

                //TotalCartSum = GetTotalCartSum();
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Remove(string id)
        {
            ShoppingCartId = GetCartId();
            var cartItem =  await _context.shoppingCartItems.SingleOrDefaultAsync(
                c => c.CartId == ShoppingCartId
                && c.ItemId == id);
                _context.shoppingCartItems.Remove(cartItem);
            //Increase the good count in a storage
            Good goodRemoved = await _context.goods.FirstOrDefaultAsync(g => g.Id == cartItem.GoodId);
            goodRemoved.CountInStorage = goodRemoved.CountInStorage +1;

            await _context.SaveChangesAsync();
            //TotalCartSum = GetTotalCartSum();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> SendToEmail(string InOrder,
                                        string Surname, string Name, string SecondName, 
                                        string Tel, string Email, string Address, string Deliver)
        {
            //string emailFrom = "";
            //string emailTo = "";
            //var senderEmail = new MailAddress(emailFrom);
            //var receiverEmail = new MailAddress(emailTo);
            //var emailPassword = "";

            //var sub = "Новый заказ";
            //var body = $"Оформлен заказ от {DateTime.Now} на имя: {Surname} {Name} {SecondName} \n" +
            //           $"Состав заказа: \n";

            ////Get all goods from the cart
            //ShoppingCartId = GetCartId();
            //var cart = await _context.shoppingCartItems.Where(
            //    c => c.CartId == ShoppingCartId).Include(i => i.Good).ToListAsync();

            //double totalsum = 0;
            //string[] ItemsToOrder = InOrder.Split('?');
            //foreach (var x in cart)
            //{
            //    for (int i = 0; i < ItemsToOrder.Length; i++)
            //    {
            //        //if the good is selected to buy then include the good the the order
            //        if (x.ItemId == ItemsToOrder[i])
            //        {
            //            _context.shoppingCartItems.Remove(x);
            //            await _context.SaveChangesAsync();
            //            totalsum += Math.Round(x.Count * (x.Good.Price - ((x.Good.Price * x.Good.Discount) / 100)), 2);
            //            body += $"Товар: {x.Good.Name} Кол-во: {x.Count} Цена: {x.Good.Price} Скидка: {x.Good.Discount} \n";
            //        }
            //    }
            //}

            //Deliver = Deliver == "car" ? "доставка" : "самовывоз";
            //body += $"Общая сумма заказа: {Math.Round(totalsum, 2)} \n" +
            //        $"Контакты заказика: почта {Email}, телефон {Tel} \n" +
            //        $"Место доставки {Address}. Тип доставки: {Deliver}";

            //var smtp = new SmtpClient
            //{
            //    Host = "smtp.mail.ru",
            //    Port = 587,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    Credentials = new NetworkCredential(senderEmail.Address, emailPassword)
            //};
            //using (var mess = new MailMessage(senderEmail, receiverEmail)
            //{
            //    Subject = sub,
            //    Body = body
            //})
            //{
            //    smtp.Send(mess);
            //}

            ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
            ViewBag.CartSum = GetTotalCartSum();
            return View();
        }

        //To get selected goods from the cart
        public async Task<ActionResult> MakeOrder(string InOrder)
        {
            string[] ItemsToOrder = InOrder.Split('?');
            double sumInOrder = 0;
            ShoppingCartId = GetCartId();
            var cart = await _context.shoppingCartItems
                .Where(c => c.CartId == ShoppingCartId)
                .Include(i => i.Good).ToListAsync();
            foreach (var x in cart)
            {
                for (int i = 0; i < ItemsToOrder.Length; i++)
                {
                    //if the good is selected to buy then include the good the the order
                    if (x.ItemId == ItemsToOrder[i])
                    {
                        await _context.SaveChangesAsync();
                        sumInOrder += Math.Round(x.Count * (x.Good.Price - ((x.Good.Price * x.Good.Discount) / 100)), 2);
                    }
                }
            }

            ViewBag.Categories = await _context.categories.Include(c => c.subcategories).ToListAsync();
            ViewBag.InOrder = InOrder;
            ViewBag.InOrderSum = sumInOrder;
            ViewBag.CartSum = GetTotalCartSum();
            return View();
        }

        public string GetCartId()
        {
            //DateTime date = DateTime.Now.Subtract(new TimeSpan(0, 1, 0));
            //List <CartItem> cartItems = await _context.shoppingCartItems
            //                          .Include(i => i.Good).Where(item =>
            //                            item.DateCreated < date)
            //                          .ToListAsync();
            //foreach (CartItem cartItem in cartItems)
            //{
            //    var good = _context.goods.Find(cartItem.GoodId);
            //    good.CountInStorage += cartItem.Count;
            //    _context.shoppingCartItems.Remove(cartItem);
            //    _context.SaveChanges();
            //}
            //TotalCartSum = GetTotalCartSum();

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
            ShoppingCartId = GetCartId();
            return Math.Round(_context.shoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).Sum(i=>i.Count*Math.Round( i.Good.Price- (i.Good.Price* i.Good.Discount) /100 ,2)),2);
        }
    }
}
