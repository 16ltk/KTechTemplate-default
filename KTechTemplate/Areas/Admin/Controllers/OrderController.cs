using KTechTemplateDAL;
using KTechTemplateDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.Security.Claims;
using KTechTemplate.Helpers;
using KTechTemplate.ViewModels;

namespace KTechTemplate.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public OrderViewModel OrderViewModel { get; set; }

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(AppUtilities.Role_Admin) || User.IsInRole(AppUtilities.Role_Staff))
            {
                //Admin and Staff Access
                IEnumerable<OrderHeader> orderHeaders = _db.OrderHeaders; //retrieve all Orders and convert to List
                //Eager loading: Populate ApplicationUser Model Properties
                var appuser = _db.ApplicationUsers
                        .Include(c => c.Email)
                        .ToList();

                return View(orderHeaders);
            }
            else
            {
                //Company and Student Access
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                IEnumerable<OrderHeader> orderHeaders = _db.OrderHeaders.Where(u => u.ApplicationUserId == claim.Value); //retrieve all Orders and convert to List
                //Eager loading: Populate ApplicationUser Model Properties
                var appuser = _db.ApplicationUsers
                        .Include(c => c.Email)
                        .ToList();

                return View(orderHeaders);
            }
           
        }

        public IActionResult Details(int orderId)
        {
            OrderViewModel = new OrderViewModel()
            {
                orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId),
                orderDetail = _db.OrderDetail.Include(u => u.Product),

            };

            var appuser = _db.ApplicationUsers
                       .Include(c => c.Email)
                       .ToList();

            return View(OrderViewModel);
        }
    }
}
