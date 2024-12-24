using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Product")]

    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        [Route("Index")]
        // GET: ProductController
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        //}
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 5; // 5 item trên 1 trang

            // Kiểm tra nếu _dataContext là null
            if (_dataContext == null)
            {
                return View("Error", new { message = "DataContext is null" });
            }

            var products = await _dataContext.Products.Include("Category").Include("Brand").ToListAsync();

            // Kiểm tra nếu products là null hoặc không có dữ liệu
            if (products == null || products.Count == 0)
            {
                return View("Error", new { message = "No products found." });
            }

            if (pg < 1)
            {
                pg = 1;
            }

            // Tính tổng số bản ghi
            int recsCount = products.Count;

            // Tạo phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Lấy dữ liệu phân trang trực tiếp từ cơ sở dữ liệu
            var data = products.Skip((pg - 1) * pageSize).Take(pageSize).ToList();

            // Gửi phân trang đến View
            ViewBag.Pager = pager;

            return View(data);
        }


        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Đã có sản phẩm trong database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["message"] = "Thêm sản phẩm thành công";
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
                //return RedirectToAction("Index");
            }
            return View(product);
        }


        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }
        [HttpPost]
        [Route("Edit")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");

                // Kiểm tra Slug đã tồn tại hay chưa, nhưng không bao gồm sản phẩm hiện tại
                var existingProductWithSlug = await _dataContext.Products
                    .Where(p => p.Slug == product.Slug && p.Id != product.Id)
                    .FirstOrDefaultAsync();

                if (existingProductWithSlug != null)
                {
                    ModelState.AddModelError("", "Đã có sản phẩm trong database");
                    return View(product);
                }

                // Lấy sản phẩm hiện tại từ cơ sở dữ liệu để truy cập ảnh cũ
                var currentProductInDb = await _dataContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (product.ImageUpload != null)
                {
                    // Thêm ảnh mới
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(currentProductInDb?.Image))
                    {
                        string oldFileImage = Path.Combine(uploadDir, currentProductInDb.Image);
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
                        await product.ImageUpload.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }
                else
                {
                    // Nếu không có ảnh mới, giữ lại ảnh cũ
                    product.Image = currentProductInDb?.Image;
                }

                product.Quantity = product.Quantity;
                _dataContext.Update(product);
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

            return View(product);
        }

        //public async Task<IActionResult> Edit(ProductModel product)
        //{
        //    ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        //    ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
        //    //var existingProduct = _dataContext.Products.Find(product.Id);

        //    if (ModelState.IsValid)
        //    {
        //        product.Slug = product.Name.Replace(" ", "-");
        //        var existingProduct = await _dataContext.Products
        //         .Where(p => p.Slug == product.Slug && p.Id != product.Id)
        //         .FirstOrDefaultAsync();


        //        /*if (product.ImageUpload != null)
        //        {
        //            ModelState.AddModelError("", "Đã có sản phẩm trong database");
        //            return View(product);
        //        }*/

        //        if (product.ImageUpload != null)
        //        {
        //            //add new image
        //            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
        //            string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
        //            string filePath = Path.Combine(uploadDir, imageName);


        //            //delete old image
        //            string oldfileImage = Path.Combine(uploadDir, existingProduct.Image);
        //            try
        //            {
        //                if (System.IO.File.Exists(oldfileImage))
        //                {
        //                    System.IO.File.Delete(oldfileImage);
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình chỉnh sửa sản phẩm");
        //            }

        //            FileStream fs = new FileStream(filePath, FileMode.Create);
        //            await product.ImageUpload.CopyToAsync(fs);
        //            fs.Close();
        //            product.Image = imageName;


        //        }
        //        else
        //        {
        //            var productInDb = await _dataContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
        //            product.Image = productInDb?.Image;
        //        }

        //        _dataContext.Update(product);
        //        _dataContext.Entry(product).State = EntityState.Modified;
        //        await _dataContext.SaveChangesAsync();
        //        TempData["message"] = "Cập Nhật sản phẩm thành công";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Đã có lỗi xảy ra";
        //        List<string> errors = new List<string>();
        //        foreach (var value in ModelState.Values)
        //        {
        //            foreach (var error in value.Errors)
        //            {
        //                errors.Add(error.ErrorMessage);
        //            }
        //            string errorMessage = string.Join("\n", errors);
        //            return BadRequest(errorMessage);
        //        }
        //        //return RedirectToAction("Index");
        //    }
        //    return View(product);
        //}
        [Route("Delete")]

        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.Equals(product.Image, "noname.jpg"))
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldfileImage = Path.Combine(uploadDir, product.Image);
                try
                {
                    if (System.IO.File.Exists(oldfileImage))
                    {
                        System.IO.File.Delete(oldfileImage);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình xóa sản phẩm");
                }


            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["message"] = "Sản phẩm đã xóa thành công";
            return RedirectToAction("Index");
        }

        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int id)
        {
            var productByQuantity = await _dataContext.ProductQuantitys.Where(pq => pq.ProductId == id).ToListAsync();
            ViewBag.ProductByQuantity = productByQuantity;
            ViewBag.Id = id;
            return View();
        }

        [Route("StoreProductQuantity")]
        [HttpPost]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.CreatedDate = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChanges();
            TempData["message"] = "Thêm số lượng thành công";
            return RedirectToAction("AddQuantity", "Product", new { id = productQuantityModel.ProductId });
        }
    }
}
