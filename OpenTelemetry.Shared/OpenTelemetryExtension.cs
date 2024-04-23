using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Shared;

public static class OpenTelemetryExtension
{
    public static void AddOpenTelemetryExt(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));

        var openTelemetryConstants = (configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>())!;

        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants.ActivitySourceName);

        services.AddOpenTelemetry().WithTracing(options =>
        {

            options.AddSource(openTelemetryConstants.ActivitySourceName)
            .ConfigureResource(resource =>
            {
                resource.AddService(openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
            }); // Kendim using ile aktivite oluşturduğumda bu arkadaşın Source'unu kullanmış olacağım. LibraryName değişiyor trace de 

            options.AddAspNetCoreInstrumentation(aspNetCoreOptions =>
            {
                aspNetCoreOptions.Filter = (context) =>
                {
                    if (!string.IsNullOrEmpty(context.Request.Path.Value))
                        return context.Request.Path.Value.Contains("api", StringComparison.InvariantCulture); // sadece gelen endpoint isteklerinin trace edilmesini sağladı.              
                                                                                                              // Bir istek olduğunda json'ların index.html'lerin trace edilmesini istemiyorum
                                                                                                              //bu yüzden başı sadece api ile başlayan requestlerin trace edilmesi gerek
                                                                                                              // Controller içerisinde Route'da başı api/... diye başlayanların trace edilmesi.
                    return false;
                };

                aspNetCoreOptions.RecordException = true; // Default olarak exception aldığında sadece en temel exceptionbilgilerini veriyor
                                                          // ama ben mesela stack i de istersem buraya yazdığım bu konfigürasyon sayesinde exception'ın stacklerinide trace etmiş oluyorum
                                                          // Stack dediğiö şeyde şu bu kod yazılmadan önce sadece hata exception adını yazıyor.
                                                          // Mesela NullReference veya Attempted to divide by zero gibi.
                                                          // Fakat bu kodu yazdıktan sonra artık exception'ın her şeyini yazıyor.
                                                          // eğerki exceptionları logluyorsam buna ihtiyacım yok gereksiz maaliyete gerek yok

                aspNetCoreOptions.EnrichWithException = (activity, exception) => // Uygulama bir exception fırlatırsa buraya geliyor.
                { //Bende burada güncel aktiviteye set tag diyip o an ki exception'a ekstra bilgi kaydedebiliyorum bu şekilde.
                    activity.SetTag("key1exception", exception);
                };
            }); //AddAspNetCoreInstrumentation yazdığım anda bu arkadaş kendi source'unu üretiyor ve kendi ürettiği tüm trace'leri kendi source'ından üretiyor.

            options.AddEntityFrameworkCoreInstrumentation(efcoreOptions =>
            {
                efcoreOptions.SetDbStatementForText = true; // DB ifadelerinin activity içerisine text olarak kaydedilsin mi?
                efcoreOptions.SetDbStatementForStoredProcedure = true; // StoreProcedure olarak kaydedilsin mi?
                efcoreOptions.EnrichWithIDbCommand = (activity, dbCommand) =>
                {
                   //EF Core ile ilgili üretilen sql cümlkeciğiniactivity yani span olarak her kaydettiğimizde bu metot tetikleniyor.
                   //Ek oalrak bir şeyler daha eklemek istiyorsam yanında tag event falan burada ekleyebiliyorum.
                   //Bilerek boşbırakıldı.
                }; //Burada db context i zengileştirme ile alakalı bir property, ekstra datalarında kaydedilmesinide sağlayabilirsin.
                   //EFCore ile iligili üretilen sql cümleciğini aktivite olarak her kaydedildiğinde tetikleniyor.
            });

            options.AddHttpClientInstrumentation(httpOptions =>
            {
                httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                {
                    var requestContent = "empty";

                    if(request.Content is not null)
                    {
                        requestContent = await request.Content.ReadAsStringAsync(); // Bunun yerine her mikroserviste middleware kullanılması daha sağlıklı. 
                    }                                                               // Third party bir yere istek atıyorsam mesela google'a o zaman bu arkadaşı kullanmak mantıklı.
                                                                                    // Mesela bir kamu kurumuna istek atıyoruz E-Devlete falan orada bu işe yarar tutar request ve response'u

                    activity.SetTag("http.request.body", requestContent);
                };

                httpOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
                {
                    if (response.Content is not null)
                    {
                        activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                    }
                };
            });

            options.AddRedisInstrumentation(redisOptions =>
            {
                redisOptions.SetVerboseDatabaseStatements = true; // Database'ler ile ilgili statement'ları kaydet diyorum
            });// bu redisi kaydedebilmek bana DI containerdan alabileceğim abna bir ConnectionMultiplexer ver ki
               // ben bu redis ile ilgili dataları bu ConnectionMultiplexer üzerinden izleyebileyim.
               // Bunuda RedisService içinde Property oluşturup ekledim. Program.cs'de de Singleton olarak verdim

            options.AddConsoleExporter();
            options.AddOtlpExporter(); // jeager a gönder trace'leri
        });
    }
}