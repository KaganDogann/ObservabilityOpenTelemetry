using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System.Diagnostics;

namespace Common.Shared;

public class RequestAndResponseActivityMiddleware
{
    // Şimdi biz burada gelen request i ve response'u okuyacağız ya bunu performanslı okumamız için bir tane library kullanacağız,
    // memory leak'i azaltmak için çünkü burada stream üzerinden request ve response'u okuyacağız.
    // Yükleyeceğimiz paket microsoft.io.recyclablememorystream bu. Amaç performansı arttırmak garbage collector'ün yükünü azaltmak. 

    private readonly RequestDelegate _next; // bu next gelen request'i bir sonraki middleware e gönder demek

    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

    public RequestAndResponseActivityMiddleware(RequestDelegate next)
    {
        _next = next;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await AddRequestBodyContentToActivityTags(context);
        await AddResponseBodyContentToActivityTags(context);
    }

    private async Task AddRequestBodyContentToActivityTags(HttpContext context)
    {
        context.Request.EnableBuffering(); //Bu kod, HTTP isteğinin veri akışını bellekte bir önbelleğe almak için kullanılır.
                                           //Bu işlem, isteğin veri akışının birden fazla kez okunmasını sağlar.
                                           //Özellikle, isteğin gövdesini birden fazla kez okumak gerektiğinde kullanışlıdır.

        var requestBodyStreamReader = new StreamReader(context.Request.Body); // Bu kod, isteğin gövdesini okumak için bir StreamReader nesnesi oluşturur.
                                                                              // Bu nesne, HTTP isteğinin gövdesinden veri okumak için kullanılacaktır.

        var requestBodyContent = await requestBodyStreamReader.ReadToEndAsync(); // Bu kod, isteğin gövdesini tamamen okur ve bu gövdeyi bir dize olarak requestBodyContent değişkenine atar
                                                                                 // . ReadToEndAsync() metodu, gövdeyi tamamen okumak için kullanılır 

        Activity.Current?.SetTag("http.request.body", requestBodyContent); //Bu kod, isteğin gövdesini loglamak veya izlemek için kullanılır.
                                                                           //Örneğin, Activity.Current bir etkinliği temsil eder ve SetTag() metodu,
                                                                           //belirli bir etiket değeri ile bu etkinliğe bir etiket ekler.

        context.Request.Body.Position = 0; // Bu kod, isteğin gövdesinin okuma konumunu sıfırlar.
                                           // Önceki işlemde isteğin gövdesi tamamen okunmuştu ve bu satır,
                                           // isteğin bir başka yerde de okunması gerektiğinde, okuma işleminin başlangıç konumunu sıfırlar.
                                           // Bu, EnableBuffering() ile önbelleğe alınan gövdenin tekrar okunmasını sağlar.
    }


    private async Task AddResponseBodyContentToActivityTags(HttpContext context)
    {

        var originalResponse = context.Response.Body; // orjinal response yaptım body zaten şu an boş,
                                                      // bu aşamada daha repsonse a geçmedi yani body!yi doğrudan okursak eğer uygulama başka yerden okuyamayacağı için body'yi patlayacaktır

        await using var responseBodyMemoryStream = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBodyMemoryStream;


        await _next(context);

        responseBodyMemoryStream.Position = 0;

        var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);
        var responseBodyContent = await responseBodyStreamReader.ReadToEndAsync();
        Activity.Current?.SetTag("http.response.body", responseBodyContent);
        responseBodyMemoryStream.Position = 0;
        await responseBodyMemoryStream.CopyToAsync(originalResponse);



    }
}
