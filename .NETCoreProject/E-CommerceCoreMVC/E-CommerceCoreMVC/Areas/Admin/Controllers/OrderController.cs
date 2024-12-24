using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Order")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var item = await _dataContext.Orders.OrderByDescending(o => o.Id).ToListAsync();
            return View(item);
        }
        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string? ordercode)
        {
            var item = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();
            var shippingCost = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
            ViewBag.ShippingCost = shippingCost.ShippingCost;
            ViewBag.Status = shippingCost.Status;
            return View(item);
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string? ordercode, int status)
        {
            // Log để kiểm tra giá trị nhận từ client
            Console.WriteLine($"OrderCode from client: {ordercode}");
            Console.WriteLine($"Status from client: {status}");

            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }
            _dataContext.Orders.Attach(order);
            order.Status = status;
            if (status == 2)
            {
                var detailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice,
                    }).ToListAsync();

                var statiscalModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);

                if (statiscalModel != null)
                {
                    foreach (var item in detailsOrder)
                    {
                        statiscalModel.Quantity += 1;
                        statiscalModel.Sold += item.Quantity;
                        statiscalModel.Revenue += item.Quantity * item.Price;
                        statiscalModel.Profit += item.Price - item.CapitalPrice;
                    }
                    _dataContext.Update(statiscalModel);
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    foreach (var item in detailsOrder)
                    {
                        new_quantity += 1;
                        new_sold += item.Quantity;
                        new_profit += item.Price - item.CapitalPrice;

                        statiscalModel = new StatisticalModel
                        {
                            DateCreated = order.CreatedDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = item.Quantity * item.Price,
                            Profit = new_profit
                        };
                        _dataContext.Add(statiscalModel);
                    }
                }
            }

            _dataContext.Entry(order).Property(x => x.Status).IsModified = true;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the order status", error = ex.Message });
            }
        }

    }
}
