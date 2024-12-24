using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Shipping")]
    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;
        public ShippingController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var item = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shipping = item;    
            return View();
        }


        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string tinh, string quan, decimal price)
        {
            shippingModel.City = tinh;
            shippingModel.Ward = phuong;
            shippingModel.Districe = quan;
            shippingModel.Price = price;

            try
            {
                var existingShippnig = await _dataContext.Shippings
                    .AnyAsync(x => x.City == tinh && x.Ward == phuong && x.Districe == quan);
                if (existingShippnig)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu bị trùng lặp" });
                }
                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm thành công" });
            }
            catch(Exception)
            {
                return StatusCode(500, "Error occurred");
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);

            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Đã xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
