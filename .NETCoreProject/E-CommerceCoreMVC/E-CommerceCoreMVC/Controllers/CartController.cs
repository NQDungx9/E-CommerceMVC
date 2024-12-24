using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Models.ViewModel;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace E_CommerceCoreMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        public CartController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if(shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            //Nhận coupon từ cookie
            var coupon_code = Request.Cookies["CouponTitle"];

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingCost = shippingPrice,
                TotalAmount = cartItems.Sum(x => x.Quantity * x.Price) + shippingPrice,
                CouponCode = coupon_code
            };
            return View(cartVM);
        }
        [HttpPost]
        public async Task<IActionResult> Add(int Id, int Quantity)
        {
            // Kiểm tra người dùng đã đăng nhập hay chưa
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            // Tìm sản phẩm theo ID
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            if (Quantity <= 0)
            {
                Quantity = 1;
            }

            // Lấy giỏ hàng từ session hoặc tạo mới
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem == null)
            {
                // Nếu chưa có, thêm sản phẩm mới vào giỏ hàng
                cart.Add(new CartItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = Quantity,
                    Image = product.Image
                });
            }
            else
            {
                // Nếu đã có, tăng số lượng
                cartItem.Quantity += Quantity;
            }

            // Cập nhật lại giỏ hàng vào session
            HttpContext.Session.setJson("Cart", cart);

            // Trả về JSON cho AJAX
            var totalItems = cart.Sum(c => c.Quantity);

            return Json(new { success = true, totalItems });
        }



        [HttpPost]
        public IActionResult CheckLoginStatus()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            return Json(new { isAuthenticated });
        }


        //public async Task<IActionResult> Add(int Id)
        //{
        //    ProductModel product = await _dataContext.Products.FindAsync(Id);

        //    List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

        //    CartItemModel cartItems = cart.Where(c=>c.ProductId == Id).FirstOrDefault();

        //    if(cartItems == null)
        //    {
        //        cart.Add(new CartItemModel(product));
        //    }
        //    else
        //    {
        //        cartItems.Quantity += 1;
        //    }
        //    TempData["message"] = "Thêm vào giỏ hàng thành công";

        //    HttpContext.Session.setJson("Cart", cart);

        //    return Redirect(Request.Headers["Referer"].ToString());
        //}

        //public async Task<IActionResult> Decrease(int Id)
        //{
        //    List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

        //    CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

        //    if(cartItem.Quantity > 1)
        //    {
        //        --cartItem.Quantity;
        //    }
        //    else
        //    {
        //        cart.RemoveAll(p=>p.ProductId == Id);
        //    }
        //    if(cart.Count == 0)
        //    {
        //        HttpContext.Session.Remove("Cart");
        //    }
        //    else
        //    {
        //        HttpContext.Session.setJson("Cart", cart);
        //    }
        //    return RedirectToAction("Index");
        //}

        //public async Task<IActionResult> Increase(int Id)
        //{
        //    List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

        //    CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

        //    if (cartItem.Quantity >= 1)
        //    {
        //        ++cartItem.Quantity;
        //    }
        //    else
        //    {
        //        cart.RemoveAll(p => p.ProductId == Id);
        //    }
        //    if (cart.Count == 0)
        //    {
        //        HttpContext.Session.Remove("Cart");
        //    }
        //    else
        //    {
        //        HttpContext.Session.setJson("Cart", cart);
        //    }
        //    return RedirectToAction("Index");
        //}



        [HttpPost]
		public async Task<IActionResult> DecreaseAsync(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    --cartItem.Quantity;
                }
                else
                {
                    cart.RemoveAll(p => p.ProductId == Id);
                }

                // Cập nhật session hoặc xóa session nếu giỏ hàng rỗng
                if (cart.Count == 0)
                {
                    HttpContext.Session.Remove("Cart");
                    return Json(new
                    {
                        success = true,
                        quantity = 0,
                        totalPrice = 0,
                        grandTotal = 0,
                        totalItems = 0
                    });
                }
                else
                {
                    HttpContext.Session.setJson("Cart", cart);
                }
            }

            // Tính tổng tiền sau khi cập nhật giỏ hàng
            var grandTotal = cart.Sum(c => c.Quantity * c.Price);
            var totalPrice = cartItem?.Quantity * cartItem?.Price ?? 0;

            return Json(new
            {
                success = true,
                quantity = cartItem?.Quantity ?? 0,
                totalPrice = totalPrice,
                grandTotal = grandTotal,
                totalItems = cart.Sum(c => c.Quantity)
            });
        }


		[HttpPost]
		public async Task<IActionResult> Increase(int Id)
		{
			ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
			{
				++cartItem.Quantity;
				HttpContext.Session.setJson("Cart", cart);
			}
			else
			{
				cartItem.Quantity =  product.Quantity;
				TempData["message"] = "Tăng số lượng sản phẩm thất bại ";
			}
            // Tính tổng tiền sau khi cập nhật giỏ hàng
            var grandTotal = cart.Sum(c => c.Quantity * c.Price);
            var totalPrice = cartItem?.Quantity * cartItem?.Price ?? 0;

            return Json(new
            {
                success = true,
                quantity = cartItem?.Quantity ?? 0,
                totalPrice = totalPrice,
                grandTotal = grandTotal,
                totalItems = cart.Sum(c => c.Quantity)
            });
        }

		public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            cart.RemoveAll(p => p.ProductId == Id);
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
                
            }
            else
            {
                HttpContext.Session.setJson("Cart", cart);
            }
            TempData["delete"] = "Đã xóa sản phẩm khỏi giỏ hàng thành công";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["clear"] = "Đã clear giỏ hàng thành công";
            return RedirectToAction("Index");
        }


        /*[HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shipping, string quan, string tinh, string phuong)
        {
            var existingShip = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.Districe == quan && x.Ward == phuong);

            decimal shippingPrice = 0;
            if (existingShip != null)
            {
                shippingPrice = existingShip.Price;
            }
            else
            {
                shippingPrice = 50000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOpitons = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOpitons);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding Shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }*/

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shipping, string quan, string tinh, string phuong)
        {
            if (string.IsNullOrWhiteSpace(tinh) || string.IsNullOrWhiteSpace(quan) || string.IsNullOrWhiteSpace(phuong))
            {
                return Json(new { success = false, message = "Vui lòng chọn đầy đủ địa chỉ giao hàng" });
            }

            var existingShip = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.Districe == quan && x.Ward == phuong);

            decimal shippingPrice = (existingShip != null) ? existingShip.Price : 50000;

            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding Shipping price cookie: {ex.Message}");
            }
            return Json(new { success = true, shippingPrice });
        }


        [HttpGet]
        [Route("Cart/DeleteShipping")]
        public async Task<IActionResult> DeleteShipping()
        {
            Response.Cookies.Delete("ShippingPrice");
            return RedirectToAction("Index","Cart");
        }


        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            string couponTitle = validCoupon.Name + " | " + validCoupon?.Description;

            if (validCoupon != null)
            {
                TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
                int dayremaining = remainingTime.Days;

                if (dayremaining >= 0)
                {
                    try
                    {
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                        };

                        Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                        return Ok(new { success = true, message = "Coupon Apply successfully" });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding coupon {ex.Message}");
                        return Ok(new { success = false, message = "Coupon apply failed" });
                    }
                }
                else
                {
                    return Ok(new { success = false, message = "Coupon has Expired" });
                }
            }
            else
            {
                return Ok(new { success = false, message = "Coupon not Existed" });
            }
            return Json(new { CouponTitle = couponTitle });
        }
    }
}
