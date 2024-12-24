using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Category")]

    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            return View(_dataContext.Categories.OrderByDescending(c => c.Id).ToList());
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Đã có danh mục trong database");
                    return View(category);
                }

                category.Status = 1;
                _dataContext.Categories.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Thêm Danh Mục Thành Công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Đã có lỗi xảy ra";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                    string errorMessage = string.Join("\n", errors);
                    return BadRequest(errorMessage);
                }
            }
            return View();
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");

                // Kiểm tra Slug đã tồn tại hay chưa, nhưng không bao gồm sản phẩm hiện tại
                var existingProductWithSlug = await _dataContext.Categories
                    .Where(p => p.Slug == category.Slug && p.Id != category.Id)
                    .FirstOrDefaultAsync();

                if (existingProductWithSlug != null)
                {
                    ModelState.AddModelError("", "Đã có danh mục trong database");
                    return View(category);
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Cập Nhật danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Đã có lỗi xảy ra";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(category);
        }

        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            // Tìm danh mục với ID
            var category = await _dataContext.Categories.FindAsync(Id);

            // Kiểm tra nếu danh mục không tồn tại
            if (category == null)
            {
                TempData["error"] = "Danh mục không tồn tại hoặc đã bị xóa.";
                return RedirectToAction("Index");
            }
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Danh mục đã xóa thành công";
            return RedirectToAction("Index");

            /*TempData["error"] = "Đã có lỗi xảy ra";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);*/


        }
    }
}
