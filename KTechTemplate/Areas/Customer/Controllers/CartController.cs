using KTechTemplateDAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using KTechTemplate.ViewModels;
using Microsoft.EntityFrameworkCore;
using KTechTemplateDAL.Models;
using System.Collections;
using KTechTemplate.Helpers;
using Stripe.Checkout;
using Microsoft.Extensions.Options;
using Stripe;

namespace KTechTemplate.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; } //BindProperty: Will automatically bind property when Posting ('Summary' PostAction) - no need to pass as Parameter

        public int OrderTotal { get; set; }
        

        public CartController (ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            //retrieve current logged in UserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                ListCart = _db.ShoppingCarts.Include(x => x.Product).Where(u => u.ApplicationUserId == claim.Value),

                OrderHeader = new ()
            };

            //Determine Price based on Quantity/Count
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);

                //Calculate CartTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        //GET
        public IActionResult Summary()
        {
            //retrieve current logged in UserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                ListCart = _db.ShoppingCarts.Include(x => x.Product).Where(u => u.ApplicationUserId == claim.Value),

                OrderHeader = new()
            };

            //Populate details of Logged in User
            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.Province = ShoppingCartVM.OrderHeader.ApplicationUser.Province;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            //Determine Price based on Quantity/Count
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);

                //Calculate OrdertTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        //POST
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            //retrieve current logged in UserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _db.ShoppingCarts.Include(x => x.Product).Where(u => u.ApplicationUserId == claim.Value);
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            //Determine Price based on Quantity/Count
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);

                //Calculate OrderTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            //Check type of User in order to determine Order and Payment Status
            ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == claim.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //Set Status
                ShoppingCartVM.OrderHeader.PaymentStatus = AppUtilities.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = AppUtilities.StatusPending;
            }
            else
            {
                //Set Status: Company Users*
                ShoppingCartVM.OrderHeader.PaymentStatus = AppUtilities.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = AppUtilities.StatusApproved;

            }

            _db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);   
            _db.SaveChanges();


            //Generate Order Details per Item in the ShoppingCart
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };

                _db.OrderDetail.Add(orderDetail);
            }
            _db.SaveChanges();


            //Only allow Non-Company Users to proceed to Payment Portal
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //Set Status
                ShoppingCartVM.OrderHeader.PaymentStatus = AppUtilities.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = AppUtilities.StatusPending;


                //================================================
                //Ref.: https://stripe.com/docs/payments/accept-a-payment
                //Configure Stripe Payments for Checkout
                //Always use Package "checkout", not Billing*
                //================================================

                var domain = "https://localhost:44396/"; //alternetively configure in Pipeline
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/Index",
                };

                //Populate with Shopping Cart Items
                foreach (var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "zar",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                //Create Session for Payment
                var service = new SessionService();
                Session session = service.Create(options);

                //Retrieve SessionId and PaymentIntentId
                UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, ShoppingCartVM.OrderHeader.Id.ToString());

                _db.SaveChanges();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); //Status report '303' Redirects to Stripes Payment Portal

            }
            else
            {
                //pass 'id' in order to redirect to confirmation view with the particular placed order***
                return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            }

        }

        public IActionResult OrderConfirmation (int id)
        {
            OrderHeader orderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);

            if (orderHeader.PaymentStatus != AppUtilities.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                //check the Stripe Status - Insure Payment is made before Approving***
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    UpdateStatus(id, AppUtilities.StatusApproved, AppUtilities.PaymentStatusApproved);
                    _db.SaveChanges();
                }
            }
            
            
            //Display ShoppingCart Items on Stripe Payment Portal
            List<ShoppingCart> shoppingCarts = _db.ShoppingCarts.Include(x => x.Product)
                .Where(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();


            //Clear Shopping Cart after Payment
            _db.ShoppingCarts.RemoveRange(shoppingCarts);
            _db.SaveChanges();

            return View(id); //pass 'id' in order to redirect to confirmation view with the particular placed order***
        }

        public IActionResult Add (int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(x => x.Id == cartId);

            IncrementCount(cart, 1);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Subtract (int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(x => x.Id == cartId);

            //Avoids a negative value of Count
            if(cart.Count <= 1)
            {
                _db.ShoppingCarts.Remove(cart);
            }
            else
            {
                DecrementCount(cart, 1);
            }
            
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Remove (int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(x => x.Id == cartId);

            _db.ShoppingCarts.Remove(cart);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public int DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;
            return shoppingCart.Count;
        }

        public int IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return shoppingCart.Count;
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if(quantity <= 50)
            {
                return price;
            }
            else
            {
                if(quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }

        public void Update (OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        //Set paymentStatus to null, only need status on payment 'Once'
        public void UpdateStatus(int id, string orderStatus, string paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if(paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        //Update Status for Final Payment
        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);

            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymentIntentId = paymentIntentId; //"session.PaymentIntentId" no longer generated. Rather use SessionId for Refunding Clients
        }
    }
}
