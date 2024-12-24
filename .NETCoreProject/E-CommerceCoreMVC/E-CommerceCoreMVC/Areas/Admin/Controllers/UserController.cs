using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public UserController(DataContext dataContext, RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Route("Index")]
        /*public async Task<IActionResult> Index()
        {
            var userWithRole = await (from u in _userManager.Users
                                      join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                      join r in _roleManager.Roles on ur.RoleId equals r.Id
                                      select new { User = u, RoleName = r.Name }).ToListAsync();
            //return View(_userManager.Users.OrderByDescending(b => b.Id).ToList());
            return View(userWithRole);
        }*/
        public async Task<IActionResult> Index()
        {
            var userWithRole = await (from u in _userManager.Users
                                      join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                      join r in _roleManager.Roles on ur.RoleId equals r.Id
                                      select new UserWithRoleViewModel 
                                      {
                                          UserId = u.Id,
                                          UserName = u.UserName,
                                          Email = u.Email,
                                          PhoneNumber = u.PhoneNumber,
                                          RoleName = r.Name
                                      }).ToListAsync();

            return View(userWithRole);
        }



        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email); //tìm user dựa vào mail
                    var userId = createUser.Id; //lấy userId
                    var role = _roleManager.FindByIdAsync(user.RoleId); // lấy roleid
                    //gán quyền
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    TempData["message"] = "Thêm tài khoản thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
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
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }



        /// <summary>
        /// Edit 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>


        [HttpGet]
        [Route("Edit")]
        /*public async Task<IActionResult> Edit(string? Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            // Lấy vai trò hiện tại của người dùng
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string? id, AppUserModel user)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update user properties
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    // Remove the user from all existing roles
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                    if (!removeRolesResult.Succeeded)
                    {
                        // Handle errors if removing roles fails
                        foreach (var error in removeRolesResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(user);
                    }

                    // Fetch the new role name using RoleId and assign it
                    if (!string.IsNullOrEmpty(user.RoleId))
                    {
                        var role = await _roleManager.FindByIdAsync(user.RoleId);
                        if (role != null)
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, role.Name);
                            if (!addToRoleResult.Succeeded)
                            {
                                // Handle errors if adding the role fails
                                foreach (var error in addToRoleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return View(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "The specified role does not exist.");
                            return View(user);
                        }
                    }

                    TempData["message"] = "Cập Nhật Thành Công";
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    // Handle errors if updating user properties fails
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Populate roles for dropdown selection if an error occurs
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            // Display validation errors if ModelState is invalid
            return View(user);
        }*/


        public async Task<IActionResult> Edit(string? Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy vai trò hiện tại của người dùng
            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            // Tạo EditUserViewModel và thêm thông tin người dùng
            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = currentRole, // Vai trò hiện tại
                RoleName = currentRole // Lưu tên của role
            };

            // Lấy danh sách các vai trò và tạo SelectList
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Role = new SelectList(roles, "Id", "Name", viewModel.RoleId); // Cập nhật SelectList đúng

            return View(viewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        /*public async Task<IActionResult> Edit(string? id, EditUserViewModel viewModel)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Lấy danh sách các vai trò từ RoleManager và thiết lập ViewBag.Role
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Role = new SelectList(roles, "Id", "Name", viewModel.RoleId);

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin người dùng, không gán lại RoleId ở đây
                existingUser.UserName = viewModel.UserName;
                existingUser.Email = viewModel.Email;
                existingUser.PhoneNumber = viewModel.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    // Lấy vai trò hiện tại của người dùng
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    var currentRole = currentRoles.FirstOrDefault();

                    // Nếu vai trò thay đổi, thực hiện thay đổi vai trò
                    if (currentRole != viewModel.RoleId)
                    {
                        if (currentRole != null)
                        {
                            // Xóa vai trò hiện tại
                            var removeRoleResult = await _userManager.RemoveFromRoleAsync(existingUser, currentRole);
                            if (!removeRoleResult.Succeeded)
                            {
                                foreach (var error in removeRoleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return View(viewModel);
                            }
                        }

                        // Gán vai trò mới
                        var newRole = await _roleManager.FindByIdAsync(viewModel.RoleId);
                        if (newRole != null)
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, newRole.Name);
                            if (!addToRoleResult.Succeeded)
                            {
                                foreach (var error in addToRoleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Vai trò được chọn không tồn tại.");
                            return View(viewModel);
                        }
                    }

                    TempData["message"] = "Cập nhật thành công";
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(viewModel);
        }*/
        public async Task<ActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin người dùng (không thay đổi vai trò tại đây)
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    AddIdentityErrors(result);
                    return View(model);
                }

                // Lấy vai trò hiện tại của user
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRoleName = currentRoles.FirstOrDefault(); // Lấy tên vai trò hiện tại (nếu có)

                // Lấy tên vai trò mới từ RoleId trong model
                var newRole = await _roleManager.FindByIdAsync(model.RoleId);
                var newRoleName = newRole?.Name;

                // Chỉ thực hiện khi tên vai trò mới khác với tên vai trò hiện tại
                if (newRoleName != null && currentRoleName != newRoleName)
                {
                    // Xóa các vai trò hiện tại
                    foreach (var role in currentRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }

                    // Thêm vai trò mới
                    await _userManager.AddToRoleAsync(user, newRoleName);
                }

                TempData["message"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ hoặc có lỗi, tải lại danh sách vai trò
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Role = new SelectList(roles, "Id", "Name", model.RoleId);
            return View(model);
        }













        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string? Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["message"] = "Xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
