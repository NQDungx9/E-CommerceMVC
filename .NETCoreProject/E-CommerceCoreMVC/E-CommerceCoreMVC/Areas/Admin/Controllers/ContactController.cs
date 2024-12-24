using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Contact")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ContactController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            var item = _dataContext.Contacts.ToList();
            return View(item);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            if (ModelState.IsValid)
            {

                // Lấy sản phẩm hiện tại từ cơ sở dữ liệu để truy cập ảnh cũ
                var currentcontactInDb = await _dataContext.Contacts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Name == contact.Name);

                if (contact.ImageUpload != null)
                {
                    // Thêm ảnh mới
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/contacts");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(currentcontactInDb?.LogoImage))
                    {
                        string oldFileImage = Path.Combine(uploadDir, currentcontactInDb.LogoImage);
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
                        await contact.ImageUpload.CopyToAsync(fs);
                    }
                    contact.LogoImage = imageName;
                }
                else
                {
                    // Nếu không có ảnh mới, giữ lại ảnh cũ
                    contact.LogoImage = currentcontactInDb?.LogoImage;
                }

                _dataContext.Update(contact);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Cập Nhật thành công";
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

            return View(contact);
        }
    }
}
