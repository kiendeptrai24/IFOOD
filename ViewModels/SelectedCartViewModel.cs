using iFood.Data.Enum;

namespace iFood.ViewModels
{
    public class SelectedCartViewModel
    {
        public List<int> CartIds { get; set; }
        public PaymentMethod paymentMethod { get; set; }
    }
}