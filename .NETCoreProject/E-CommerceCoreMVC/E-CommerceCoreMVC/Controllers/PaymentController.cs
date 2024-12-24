using E_CommerceCoreMVC.Models.Vnpay;
using E_CommerceCoreMVC.Services.VNpay;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceCoreMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
    }
}
