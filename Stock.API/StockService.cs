using Common.Shared.DTOs;
using Stock.API.Services;
using System.Net;

namespace Stock.API;

public class StockService
{

    private readonly PaymentService _paymentService;

    public StockService(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Dictionary<int, int> GetProductStockList()
    {
        Dictionary<int, int> productStockList = new();

        productStockList.Add(1, 10);
        productStockList.Add(2, 20);
        productStockList.Add(3, 30);

        return productStockList;
    }

    public async Task<ResponseDto<StockCheckAndPaymentResponseDto>> CheckAndPaymentProcess(StockCheckAndPaymentRequestDto requestDto)
    {
        var productStockList = GetProductStockList();

        var stockStatus = new List<(int productId, bool hasStockExist)>();

        foreach (var orderItem in requestDto.OrderItems)
        {
            var hasExistStock = productStockList.Any(x => x.Key == orderItem.ProductId && x.Value >= orderItem.Count);

            stockStatus.Add((orderItem.ProductId, hasExistStock));

        }

        if (stockStatus.Any(x => x.hasStockExist == false)) 
        {
            return ResponseDto<StockCheckAndPaymentResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(), "Stock Yetersiz.");
        }


        var (isSuccess, failMessage) = await _paymentService.CreatePaymentProcess(new PaymentCreateRequestDto()
        {
            OrderCode = requestDto.OrderCode,
            TotalPrice = requestDto.OrderItems.Sum(x => x.Price)
        });

        if (isSuccess)
        {
            return ResponseDto<StockCheckAndPaymentResponseDto>.Success(HttpStatusCode.OK.GetHashCode(), 
                new() { Description = "ödeme süreci tamamlandı." });
        }




        return ResponseDto<StockCheckAndPaymentResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(), failMessage!);

    }
}
