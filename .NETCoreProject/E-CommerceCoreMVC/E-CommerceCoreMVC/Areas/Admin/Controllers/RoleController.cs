using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext dataContext, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = dataContext;
            _roleManager = roleManager;
        }
        [HttpGet]
        [Route("Index")]
        public ActionResult Index()
        {
            var items = _dataContext.Roles.ToList();
            return View(items);
        }
        [HttpGet]
        [Route("Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(model);
                if (result.Succeeded)
                {
                    TempData["message"] = "Thêm role thành công";
                    return RedirectToAction("Index");
                }

                // Add errors to ModelState if creation failed
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }


        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string? Id)
        {
            var item = await _dataContext.Roles.FindAsync(Id);
            return View(item);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the role from the database first
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    ModelState.AddModelError("", "Role not found");
                    return View(model);
                }

                // Update only the properties you want to modify
                role.Name = model.Name;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    TempData["message"] = "Update role thành công";
                    return RedirectToAction("Index");
                }

                // Add errors to ModelState if creation failed
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }


        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string? Id)
        {
            var roles = await _dataContext.Roles.FindAsync(Id);
            // Kiểm tra nếu danh mục không tồn tại
            if (roles == null)
            {
                TempData["error"] = "Quyền không tồn tại hoặc đã bị xóa.";
                return RedirectToAction("Index");
            }
            _dataContext.Roles.Remove(roles);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Quền đã xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
