using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace E_CommerceCoreMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;

        private readonly ILogger<HomeController> _logger;
        private UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext dataContext, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var item = _dataContext.Products.Include("Category").Include("Brand").ToList();
            var slider = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
            ViewBag.Sliders = slider;
            return View(item);
        }


        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> WishList()
        {
            // Lấy thông tin UserId của người dùng hiện tại
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (currentUser == null)
            {
                TempData["error"] = "Không thể xác định người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = currentUser.Id;

            // Lấy danh sách wishlist của người dùng
            var wishlist = await _dataContext.Wishlists
                                             .Include(w => w.Product) // Nạp dữ liệu từ bảng Product
                                             .Where(w => w.UserId == currentUserId)
                                             .ToListAsync();

            return View(wishlist);
        }



        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                          join p in _dataContext.Products on c.ProductId equals p.Id
                                          join u in _dataContext.Users on c.UserId equals u.Id
                                          select new { User = u, Product = p, Compares = c }).ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> AddWistList(int Id, WishlistModel wishlist)
        {
            var user = await _userManager.GetUserAsync(User);
            var wishListProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id,
            };
            wishlist.ProductId = Id;
            wishlist.UserId = user.Id;
            _dataContext.Wishlists.Add(wishListProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add To Wishlist Successfully" });
            }
            catch (Exception )
            {
                return StatusCode(500, "An erro occurred while updating the status");
            }
        }

        public async Task<IActionResult> AddCompare(int Id, CompareModel compare)
        {
            var user = await _userManager.GetUserAsync(User);
            var comPareProduct = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id,
            };
            _dataContext.Compares.Add(comPareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add To Compare Successfully" });
            }
            catch (Exception )
            {
                return StatusCode(500, "An erro occurred while updating the status");
            }
        }


        
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "đã xóa thành công";
            return RedirectToAction("Compare", "Home");
        }

        public async Task<IActionResult> DeleteWishList(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);
            if (wishlist == null)
            {
                TempData["error"] = "Mục không tồn tại.";
                return RedirectToAction("WishList", "Home");
            }

            _dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Đã xóa thành công.";
            return RedirectToAction("WishList", "Home");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
