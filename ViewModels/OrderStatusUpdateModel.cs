using iFood.Data.Enum;

namespace iFood.ViewModels;

public class OrderStatusUpdateModel
{
    public int Id { get; set; }
    public Status Status { get; set; }
}
