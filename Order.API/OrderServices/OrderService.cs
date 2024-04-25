using Common.Shared.DTOs;
using Common.Shared.Events;
using MassTransit;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.RedisServices;
using Order.API.StockServices;
using System.Diagnostics;
using System.Net;

namespace Order.API.OrderServices;

public class OrderService
{

    private readonly AppDbContext _context;
    private readonly StockService _stockService;
    private readonly RedisService _redisService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, StockService stockService, RedisService redisService, IPublishEndpoint publishEndpoint, ILogger<OrderService> logger)
    {
        _context = context;
        _stockService = stockService;
        _redisService = redisService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateReuqestDto request)
    {


        // redis için örnek kod
        await _redisService.GetDb(0).StringSetAsync("userId", request.UserId);

        var redisUserId = _redisService.GetDb(0).StringGetAsync("userId");


        Activity.Current?.SetTag("Asp.Net Core(instrumentation) Tag1", "Asp.Net Core(instrumentation) Tag Value");

        using var activity = ActivitySourceProvider.Source.StartActivity();

        activity?.AddEvent(new("Spiariş Süreci Başladı"));

        // activity.SetBaggage("userId", request.UserId.ToString()); // Bunu yazdığım anda bu data,
                                                                  // bu requestten başka bir mikro servise istek atıldığında header'da taşınacak nerede taşıancak tracestate'te taşınacak

        var newOrder = new Order()
        {
            Created = DateTime.Now,
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Success,
            UserId = request.UserId,
            Items = request.Items.Select(x => new OrderItem()
            {
                Count = x.Count,
                ProductId = x.ProductId,
                Price = x.Price,
            }).ToList()

        };
        _context.Orders.Add(newOrder); ;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Sipariş veritabanına kaydedildi.{@userId}", request.UserId); // {@userId} bu elastic tarafında ayrı bir sütun oluşturur


        StockCheckAndPaymentRequestDto stockRequest = new();

        stockRequest.OrderCode = newOrder.OrderCode;
        stockRequest.OrderItems = request.Items;

        var(isSuccess, failMessage) = await _stockService.CheckStockAndPaymentStart(stockRequest);

        if (!isSuccess)
        {
            return ResponseDto<OrderCreateResponseDto>.Fail(HttpStatusCode.InternalServerError.GetHashCode(), failMessage!);
        }

        activity?.AddEvent(new("Spiariş Süreci Tamamlandı"));

        return ResponseDto<OrderCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(), new OrderCreateResponseDto() { Id = newOrder.Id });
    }
}
