using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DashboardController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;

            return View();
        }
        [HttpPost]
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var data = _dataContext.Statisticals
                .Select(x => new
                {
                    date = x.DateCreated.ToString("dd/MM/yyyy"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public async Task<IActionResult> GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _dataContext.Statisticals
                .Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
                .Select(x => new
                {
                    date = x.DateCreated.ToString("dd/MM/yyyy"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }

        [HttpPost]
        [Route("FilterData")]
        public async Task<IActionResult> FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _dataContext.Statisticals.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.DateCreated >= fromDate);
            }
            if (toDate.HasValue)
            {
                query = query.Where(s => s.DateCreated <= toDate);
            }

            var data = query
                .Select(x => new
                {
                    date = x.DateCreated.ToString("dd/MM/yyyy"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }
    }
}
