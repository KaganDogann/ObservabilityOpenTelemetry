using Common.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.OrderServices;

namespace Order.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderController(OrderService orderService, IPublishEndpoint publishEndpoint)
    {
        _orderService = orderService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateReuqestDto  reuqest)
    {

        var result = await _orderService.CreateAsync(reuqest);

        return new ObjectResult(result) { StatusCode = result.StatusCode };


        #region exception
        //var a = 10;
        //var b = 0;     EXCEPTION 
        //var c = a / b; 
        #endregion
    }


    [HttpGet]

    public async Task<IActionResult> SendOrderCreatedEvent()
    {
        //Kuyruğa mesaj gönder

        await _publishEndpoint.Publish(new OrderCreatedEvent() { OrderCode = Guid.NewGuid().ToString() });

        return Ok();
    }
}