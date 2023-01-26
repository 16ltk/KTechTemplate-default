using Microsoft.AspNetCore.Mvc;
using KTechTemplateDAL;
using KTechTemplateDAL.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using KTechTemplate.ViewModels;
//using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using System.Linq.Expressions;
using NPOI.SS.Formula.Functions;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;

namespace KTechTemplate.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly IWebHostEnvironment _hostEnvironment; //Access 'wwwroot' folder

        public ProductController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db; //Retrive data from DB

           // _db.Products.Include(u => u.Category); //Lambda expression: Allow API to get Category Name
            _hostEnvironment = hostEnvironment; //Depandancy Injection
            
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _db.Products; //retrieve all Products and convert to List

            //Eager loading: Populate Category Model Properties
            var category = _db.Categories
                    .Include(c => c.Name)
                    .ToList();

            return View(objProductList);

        }

        public IActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Product Not Found";
                return RedirectToAction("Index");
            }

            try
            {
                var productFromDb = _db.Products.Find(id);
                if (productFromDb == null)
                {
                    TempData["error"] = "Product Not Found";
                    return RedirectToAction("Index");
                }

                //Eager loading: Populate Category Model Properties
                var category = _db.Categories
                        .Include(c => c.Name)
                        .ToList();

                return View(productFromDb);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //GET
        public IActionResult Create()
        {
            try
            {
                ProductViewModel productVM = new()
                {
                    Product = new(),
                    CategoryList = _db.Categories.Select(
                        u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString()
                        }),
                };
                return View(productVM);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductViewModel obj, IFormFile file)
        {
            string ImageName = "";
            string wwwRootPath = _hostEnvironment.WebRootPath;
            try
            {
                //Validate Image Upload
                if (file != null)
                {
                    string FileName = file.FileName.ToString();
                    var uploads = Path.Combine(wwwRootPath, "images", "products"); //store image
                    var extension = Path.GetExtension(file.FileName);

                    if (extension == ".jpg" || extension == ".JPG" || extension == ".jpeg" || extension == ".JPEG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF" || extension == ".jfif" || extension == ".JFIF")
                    {
                        ImageName = "image_" + file.FileName.ToLower(); //rename file name
                    }
                    else
                    {
                        ModelState.AddModelError("ImageUrl", "Invalid Image.");
                        return View();
                    }

                    string path = Path.Combine(wwwRootPath, "images", "products");
                    string filepath = Path.Combine(path, ImageName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, ImageName), FileMode.Create)) //path to store image
                    {
                        file.CopyTo(fileStreams); //copy image to path

                        //check if image exists
                        string oldImagePath = ViewBag.stream;
                        if (System.IO.File.Exists(Path.Combine(wwwRootPath, "images", "products") + oldImagePath))
                        {
                            System.IO.File.Delete(Path.Combine(wwwRootPath, "images", "products") + oldImagePath);
                        }
                    }
                    obj.Product.ImageUrl = ImageName; //store image to DB
                }

                if (ModelState.IsValid)
                {
                    _db.Products.Add(obj.Product);
                    _db.SaveChanges();

                    TempData["success"] = "Product Added Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                throw;
            }

            ProductViewModel productVM = new()
            {
                Product = new(),
                CategoryList = _db.Categories.Select(
                        u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString()
                        }),
            };
            return View(productVM);
        }
        public IActionResult Edit(int? id)
        {
            try
            {
                //Render dropdown list of Product
                ProductViewModel productVM = new()
                {
                    Product = new(),
                    CategoryList = _db.Categories.Select(
                            u => new SelectListItem
                            {
                                Text = u.Name,
                                Value = u.Id.ToString()
                            }),
                };

                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    productVM.Product = _db.Products.FirstOrDefault(u => u.Id == id);
                    return View(productVM);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductViewModel obj, IFormFile? file)
        {
            string ImageName = "";
            string wwwRootPath = _hostEnvironment.WebRootPath;
            try
            {
                //Validate Image Upload
                if (file != null)
                {
                    string FileName = file.FileName.ToString();
                    var uploads = Path.Combine(wwwRootPath, "images", "products"); //store image
                    var extension = Path.GetExtension(file.FileName);

                    if (extension == ".jpg" || extension == ".JPG" || extension == ".jpeg" || extension == ".JPEG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF" || extension == ".jfif" || extension == ".JFIF")
                    {
                        ImageName = "image_" + file.FileName.ToLower(); //rename file name
                    }
                    else
                    {
                        ModelState.AddModelError("ImageUrl", "Invalid Image.");
                        return View(obj);
                    }

                    string path = Path.Combine(wwwRootPath, "images", "products");
                    string filepath = Path.Combine(path, ImageName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, ImageName), FileMode.Create)) //path to store image
                    {
                        file.CopyTo(fileStreams); //copy image to path

                        //check if image exists
                        string oldImagePath = ViewBag.stream;
                        if (System.IO.File.Exists(Path.Combine(wwwRootPath, "images", "products") + oldImagePath))
                        {
                            System.IO.File.Delete(Path.Combine(wwwRootPath, "images", "products") + oldImagePath);
                        }
                    }
                    obj.Product.ImageUrl = ImageName; //store image to DB

                }

                if (ModelState.IsValid)
                {
                    _db.Products.Update(obj.Product);
                    _db.SaveChanges();

                    TempData["success"] = "Product Updated Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                throw;
            }

            ProductViewModel productVM = new()
            {
                Product = new(),
                CategoryList = _db.Categories.Select(
                        u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString()
                        }),
            };
            return View(productVM);
        }

       
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Product Not Found";
                return RedirectToAction("Index");
            }

            try
            {
                var productFromDb = _db.Products.Find(id);
                if (productFromDb == null)
                {
                    TempData["error"] = "Product Not Found";
                    return RedirectToAction("Index");
                }

                //Eager loading: Populate Category Model Properties
                var category = _db.Categories
                        .Include(c => c.Name)
                        .ToList();
                return View(productFromDb);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult PostDelete(int id)
        {
            try
            {
                var obj = _db.Products.Find(id);
                if (obj == null)
                {
                    return NotFound();
                }
                else
                {
                    _db.Products.Remove(obj);
                    _db.SaveChanges();

                    TempData["success"] = "Product Deleted Successfully";
                    return RedirectToAction("Index");

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*#region API CALLS
        [HttpGet]
        public IActionResult GetAll(string? includeProperties = null)
        {
            var productList = _db.Products.Include(x => x.Category);

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    productList = productList.Include(includeProp);
                }
            }

            return Json(new { data = productList });
        }
        #endregion*/
    }
}
