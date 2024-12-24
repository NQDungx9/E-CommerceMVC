using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Slider")]
    [Authorize]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SliderController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {
            List<SliderModel> sliders = _dataContext.Sliders.ToList();
            const int pageSize = 5; //5 item trên 1 trang
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = sliders.Count;
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = sliders.Skip(recSkip).Take(pager.PageSize).ToList();
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
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                /*var check = await _dataContext.Sliders.FirstOrDefaultAsync(p => p.Name == slider.Name);
                bool slide = await _dataContext.Sliders.AnyAsync(s => s.Name == check.Name);
                if (slide == true)
                {
                    ModelState.AddModelError("", "Đã có slider trong database");
                    return View(slider);
                }*/
                if (slider.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }
                _dataContext.Sliders.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Thêm slider Thành Công";
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
        public IActionResult Edit(int Id)
        {
            var item = _dataContext.Sliders.Find(Id);
            return View(item);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {
            if (ModelState.IsValid)
            {

                // Lấy sản phẩm hiện tại từ cơ sở dữ liệu để truy cập ảnh cũ
                var currentsliderInDb = await _dataContext.Sliders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == slider.Id);

                if (slider.ImageUpload != null)
                {
                    // Thêm ảnh mới
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(currentsliderInDb?.Image))
                    {
                        string oldFileImage = Path.Combine(uploadDir, currentsliderInDb.Image);
                        try
                        {
                            if (System.IO.File.Exists(oldFileImage))
                            {
                                System.IO.File.Delete(oldFileImage);
                            }
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình xóa ảnh cũ.");
                        }
                    }

                    // Lưu ảnh mới
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await slider.ImageUpload.CopyToAsync(fs);
                    }
                    slider.Image = imageName;
                }
                else
                {
                    // Nếu không có ảnh mới, giữ lại ảnh cũ
                    slider.Image = currentsliderInDb?.Image;
                }

                _dataContext.Update(slider);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Cập Nhật sản phẩm thành công";
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

            return View(slider);
        }

        [Route("Delete")]

        public async Task<IActionResult> Delete(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
            if (!string.Equals(slider.Image, "noname.jpg"))
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                string oldfileImage = Path.Combine(uploadDir, slider.Image);
                try
                {
                    if (System.IO.File.Exists(oldfileImage))
                    {
                        System.IO.File.Delete(oldfileImage);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình xóa slider");
                }


            }
            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Slider đã xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
