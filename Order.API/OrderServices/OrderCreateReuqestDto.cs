using Common.Shared.DTOs;

namespace Order.API.OrderServices;

public record OrderCreateReuqestDto // neden record? çünkü immutable olmasını istiyorum end point in request'inde geldiği anda bir adha değiştirilmesin diye
{
    public int UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = null!;
}
