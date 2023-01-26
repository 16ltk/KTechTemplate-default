using KTechTemplateDAL;
using KTechTemplateDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using KTechTemplate.ViewModels;

namespace KTechTemplate.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        /// <summary>
        /// LeeuwTK:
        /// Register Logger with Dependancy Injection
        /// </summary>
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _db.Products; //retrieve all Products and convert to List
            return View(objProductList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _db.Products.Include(x => x.Category).FirstOrDefault(m => m.Id == productId),
            };

            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //retrieve current logged in UserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            //Shopping Cart: Allow multiple records of a Product to be added for a User
            ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if(cartFromDb == null)
            {
                _db.ShoppingCarts.Add(shoppingCart);
            }
            else
            {
                IncrementCount(cartFromDb, shoppingCart.Count);
            }

            _db.SaveChanges();


            //return Redirect("Index"); magic strings
            return Redirect(nameof(Index)); //redirect to current Index
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


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}