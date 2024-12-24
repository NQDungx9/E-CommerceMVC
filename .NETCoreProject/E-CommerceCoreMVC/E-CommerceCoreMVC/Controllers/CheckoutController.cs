using E_CommerceCoreMVC.Areas.Admin.Repository;
using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using E_CommerceCoreMVC.Services.VNpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace E_CommerceCoreMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        public CheckoutController(DataContext dataContext, IEmailSender emailSender, IVnPayService vnPayService)
        {
            _emailSender = emailSender;
            _dataContext = dataContext;
            _vnPayService = vnPayService;
        }
        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = orderCode;
                orderItem.UserName = userEmail;
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                else
                {
                    shippingPrice = 0;
                }

                var coupon_code = Request.Cookies["CouponTitle"];
                if(PaymentMethod != "VnPay")
                {
                    orderItem.PaymentMethod = "COD";
                }
                else if(PaymentMethod == "VnPay")
                {
                    orderItem.PaymentMethod = "VnPay" + " " + PaymentId;
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetail = new OrderDetails();
                    orderdetail.UserName = userEmail;
                    orderdetail.OrderCode = orderCode;
                    orderdetail.ProductId = cart.ProductId;
                    orderdetail.Price = cart.Price;
                    orderdetail.Quantity = cart.Quantity;
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);
                    _dataContext.Add(orderdetail);
                    _dataContext.SaveChanges();
                }
                HttpContext.Session.Remove("Cart");

                var receiver = userEmail;
                var subject = "Đặt hàng thành công";
                var message = "Thanh toán thành công, cảm ơn bạn vì đã mua hàng";
                await _emailSender.SendEmailAsync(receiver, subject, message);
    
                TempData["message"] = "Checkout thành công vui lòng duyệt đơn hàng";

                return RedirectToAction("History","Account");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00")
            {
                var newVnPayInsert = new VnPayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    CreatedDate = DateTime.Now
                };
                _dataContext.Add(newVnPayInsert);
                await _dataContext.SaveChangesAsync();
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["message"] = "Giao dịch thành công";
                return RedirectToAction("Index", "Cart");
            }
            //return Json(response);
            return View(response);
        }
    }
}
