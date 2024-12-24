using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Brand")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpGet]
        [Route("Index")]
        //public IActionResult Index()
        //{
        //    return View(_dataContext.Brands.OrderByDescending(b => b.Id).ToList());
        //}
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brands = _dataContext.Brands.ToList();
            const int pageSize = 5; //5 item trên 1 trang
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = brands.Count;
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = brands.Skip(recSkip).Take(pager.PageSize).ToList();
            ViewBag.Pager = pager;
            return View(data);
        }


        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Đã có thương hiệu trong database");
                    return View(brand);
                }

                _dataContext.Brands.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Thêm thương hiệu Thành Công";
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

        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");

                // Kiểm tra Slug đã tồn tại hay chưa, nhưng không bao gồm sản phẩm hiện tại
                var existingProductWithSlug = await _dataContext.Brands
                    .Where(p => p.Slug == brand.Slug && p.Id != brand.Id)
                    .FirstOrDefaultAsync();

                if (existingProductWithSlug != null)
                {
                    ModelState.AddModelError("", "Đã có thương hiệu trong database");
                    return View(brand);
                }

                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Cập Nhật thương mục thành công";
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

            return View(brand);
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            // Tìm danh mục với ID
            var brand = await _dataContext.Brands.FindAsync(Id);

            // Kiểm tra nếu danh mục không tồn tại
            if (brand == null)
            {
                TempData["error"] = "Thương hiệu không tồn tại hoặc đã bị xóa.";
                return RedirectToAction("Index");
            }
            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Danh mục đã xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
