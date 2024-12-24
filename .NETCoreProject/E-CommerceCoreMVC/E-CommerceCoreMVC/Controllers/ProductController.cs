using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Models.ViewModel;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        // GET: ProductController
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Search(string SearchTerm)
        {
            var product = _dataContext.Products.
                Where(p => p.Name.Contains(SearchTerm) 
                || p.Description.Contains(SearchTerm) 
                || p.Brand.Name.Contains(SearchTerm) || p.Category.Name.Contains(SearchTerm))
                .ToList();
            ViewBag.Keyword = SearchTerm;
            return View(product);
        }


        // GET: ProductController/Details/5
        /*public async Task<IActionResult> Details(int id)
        {
            if (id == null) return RedirectToAction("Index");

            var productById = _dataContext.Products
                .Include(p => p.Ratings).Where(p => p.Id == id).FirstOrDefault();
            //related product
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;
            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productById,
            };
            return View(viewModel);
        }*/
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return RedirectToAction("Index");

            var productById = _dataContext.Products
                .Include(p => p.Ratings) // Gồm cả Ratings (bình luận)
                .FirstOrDefault(p => p.Id == id);

            if (productById == null) return NotFound();

            // Lấy sản phẩm liên quan
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Tạo ViewModel và gán danh sách bình luận
            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productById,
                Comments = productById.Ratings.ToList() // Lấy danh sách bình luận
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star,
                    NgayDang = DateTime.Now
                };
                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["message"] = "Thêm đánh giá thành công";
                return Redirect(Request.Headers["Referer"]);
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
                return RedirectToAction("Detail", new { id = rating.ProductId });
            }
            return Redirect(Request.Headers["Referer"]);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
