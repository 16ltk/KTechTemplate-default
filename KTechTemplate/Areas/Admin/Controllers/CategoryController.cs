using Microsoft.AspNetCore.Mvc;
using KTechTemplateDAL;
using KTechTemplateDAL.Models;
using Microsoft.AspNetCore.Authorization;

namespace KTechTemplate.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// LeeuwTK:
        /// Retrieve data from DB
        /// </summary>
        /// <returns></returns>

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _db.Categories; //retrieve all Categories and convert to List
            return View(objCategoryList);

        }

        public IActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Category Not Found";
                return RedirectToAction("Index");
            }

            try
            {
                var categoryFromDb = _db.Categories.Find(id);
                if (categoryFromDb == null)
                {
                    TempData["error"] = "Category Not Found";
                    return RedirectToAction("Index");
                }

                return View(categoryFromDb);
            }
            catch(Exception)
            {
                throw;
            }
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            try
            {
                //Server Side Validations
                if (obj.Name == obj.DisplayOrder.ToString())
                {
                    ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
                }

                if (ModelState.IsValid)
                {
                    _db.Categories.Add(obj);
                    _db.SaveChanges();
                    TempData["success"] = "Category Created Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(obj);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }


        //GET
        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    var categoryFromDb = _db.Categories.FirstOrDefault(x => x.Id == id);
                    return View(categoryFromDb);
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
        public IActionResult Edit(Category obj)
        {
            try
            {
                //Server Side Validations
                if (obj.Name == obj.DisplayOrder.ToString())
                {
                    ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
                }

                if (ModelState.IsValid)
                {
                    _db.Categories.Update(obj);
                    _db.SaveChanges();

                    TempData["success"] = "Category Updated Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View(obj);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Category Not Found";
                return RedirectToAction("Index");
            }

            try
            {
                var categoryFromDb = _db.Categories.Find(id);
                if (categoryFromDb == null)
                {
                    TempData["error"] = "Category Not Found";
                    return RedirectToAction("Index");
                }

                return View(categoryFromDb);
            }
            catch (Exception)
            {

                throw;
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PostDelete(int? id)
        {
            try
            {
                var obj = _db.Categories.Find(id);
                if (obj == null)
                {
                    return NotFound();
                }
                else
                {
                    _db.Categories.Remove(obj);
                    _db.SaveChanges();

                    TempData["success"] = "Category Deleted Successfully";
                    return RedirectToAction("Index");

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
