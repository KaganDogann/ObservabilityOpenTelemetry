using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;

namespace Logging.Shared;

public static class Logging
{
    //public static void AddOpenTelemetryLog(this WebApplicationBuilder builder) //Neden? 
    //{


    //    builder.Logging.AddOpenTelemetry(cfg =>
    //    {
    //        var serviceName = builder.Configuration.GetSection("OpenTelemetry")["ServiceName"];
    //        var serviceVersion = builder.Configuration.GetSection("OpenTelemetry")["ServiceVersion"];

    //        cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName!, serviceVersion));

    //        cfg.AddOtlpExporter();
    //    });
    //}

    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {

        builder.Logging.AddOpenTelemetry(cfg =>
        {
            var serviceName = builder.Configuration.GetSection("OpenTelemetry")["ServiceName"];
            var serviceVersion = builder.Configuration.GetSection("OpenTelemetry")["ServiceVersion"];
            cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName!, serviceVersion: serviceVersion));
            cfg.AddOtlpExporter();
        });
    }

    // Action delegesi nedir? parametre alan geriye bir şey dönmeyen metotları temsil eder. 
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (builderContext, loggerConfiguration) =>
    {
        var environment = builderContext.HostingEnvironment;

        loggerConfiguration
            .ReadFrom.Configuration(builderContext.Configuration) // AppSettiings içerisinde ki verilen ayaları alacak serilog için.
                                      // Bu FromLogContext ne işe yarıyor? biz buraa log üretirken bir trace Id vermemiz lazım bu traceId yi merkezi bir yerden vereceğiz.
            .Enrich.FromLogContext()  // Yoksa her log yazdığım yere gidip ekleyemem traceId'yi.
            .Enrich.WithExceptionDetails()  //Eğerki bir exception fırlatılırsa onun detaylarınıda yaz.
            .Enrich.WithProperty("Env", environment.EnvironmentName) // Her Log  atarken bir prop ekle hangi ortamdan atıldığını.
            .Enrich.WithProperty("AppName", environment.ApplicationName);


        var elasticSearchBaseUrl = builderContext.Configuration.GetSection("ElasticSearch")["BaseUrl"];
        var elasticSearchUserName = builderContext.Configuration.GetSection("ElasticSearch")["UserName"];
        var elasticSearchPassword = builderContext.Configuration.GetSection("ElasticSearch")["Password"];
        var elasticSearchIndexName = builderContext.Configuration.GetSection("ElasticSearch")["IndexName"];


        loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(elasticSearchBaseUrl))
        {
            AutoRegisterTemplate = true, // bizim elastic search'e gidecek olan template'imizi true olarak set ettik. 
            AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8, // Versiyonu belirtiyorum
            IndexFormat = $"{elasticSearchIndexName}-{environment.EnvironmentName}-logs-"+ "{0:yyy.MM.dd}", //Bocket yani kovalarımın formatını ayarlıyorum.
            ModifyConnectionSettings = x => x.BasicAuthentication(elasticSearchUserName, elasticSearchPassword),
            CustomFormatter = new ElasticsearchJsonFormatter() // Formatımı da belirttim
        });
    };
}