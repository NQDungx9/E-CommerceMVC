using E_CommerceCoreMVC.Models.Vnpay;

namespace E_CommerceCoreMVC.Services.VNpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);


    }
}
