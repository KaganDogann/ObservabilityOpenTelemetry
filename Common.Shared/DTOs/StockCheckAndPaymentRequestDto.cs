namespace Common.Shared.DTOs;

public record StockCheckAndPaymentRequestDto
{
    public string OrderCode { get; set; } = null!;

    public List<OrderItemDto> OrderItems { get; set; } = null!;
}
