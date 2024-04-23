using Common.Shared.DTOs;

namespace Order.API.StockServices;

public class StockService
{
    private readonly HttpClient _client;

    public StockService(HttpClient client)
    {
        _client = client;
    }

    public async Task<(bool isSuccess, string? failMessage)> CheckStockAndPaymentStart(StockCheckAndPaymentRequestDto requestDto)
    {
        var response = await _client.PostAsJsonAsync<StockCheckAndPaymentRequestDto>("api/Stock/CheckAndPaymentStart", requestDto);

        var responseContent = await response.Content.ReadFromJsonAsync<ResponseDto<StockCheckAndPaymentResponseDto>>();

        return response.IsSuccessStatusCode ? (true, null) : (false, responseContent!.Errors!.First());
    }
}
