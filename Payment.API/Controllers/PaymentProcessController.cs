using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PaymentProcessController : ControllerBase
{

    private readonly ILogger<PaymentProcessController> _logger;

    public PaymentProcessController(ILogger<PaymentProcessController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Create(PaymentCreateRequestDto request)
    {
        const decimal balance = 1000;

        if (request.TotalPrice > balance)
        {
            _logger.LogInformation("Ytersiz bakiye. orderCode={@orderCode}");
            return BadRequest(ResponseDto<PaymentCreateResponseDto>.Fail(400, "yetersiz bakiye"));
        }


        _logger.LogInformation("Kart işlemi başarıyla gerçekleşmiştir. orderCode={@orderCode}");
        return Ok(ResponseDto<PaymentCreateResponseDto>.Success(200, new PaymentCreateResponseDto()
        {
            Description = "kart işlemi başarıyla gerçekleşti"
        }));
    }
}