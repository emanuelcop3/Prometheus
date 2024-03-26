using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Métricas personalizadas para a aplicação
var contadorSaudacoes = new Meter("OtPrGrYa.Exemplo", "1.0.0").CreateCounter<int>("saudacoes.contagem", description: "Conta o número de saudações");

// Fonte de atividade personalizada para a aplicação
var fonteAtividadeSaudacao = new ActivitySource("OtPrGrJa.Exemplo");

var endpointOTLPTelemetry = builder.Configuration["URL_PONTO_TERMINAL_OTLP"];
var otel = builder.Services.AddOpenTelemetry();

// Configuração de recursos do OpenTelemetry com o nome da aplicação
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Adiciona Métricas para ASP.NET Core e nossas métricas personalizadas e exporta para o Prometheus
otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddMeter(contadorSaudacoes.Name)
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter());

// Adiciona Rastreamento para ASP.NET Core e nossa Fonte de Atividade personalizada e exporta para o Jaeger
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource(fonteAtividadeSaudacao.Name);
    if (endpointOTLPTelemetry != null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(endpointOTLPTelemetry);
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});

builder.Services.AddHttpClient();

var app = builder.Build();

string EnviarSaudacao(ILogger<Program> logger)
{
    // Cria uma nova atividade com escopo para o método
    using var atividade = fonteAtividadeSaudacao.StartActivity("AtividadeSaudacao");

    // Registra uma mensagem
    logger.LogInformation("Enviando saudação");

    // Incrementa o contador personalizado
    contadorSaudacoes.Add(1);

    // Adiciona uma tag à Atividade
    atividade?.SetTag("saudacao", "Olá Mundo!");

    return "Olá Mundo!";
}

async Task EnviarSaudacaoAninhada(int nivelAninhamento, ILogger<Program> logger, HttpContext context, IHttpClientFactory clientFactory)
{
    // Cria uma nova atividade com escopo para o método
    using var atividade = fonteAtividadeSaudacao.StartActivity("AtividadeSaudacao");

    if (nivelAninhamento <= 5)
    {
        // Registra uma mensagem
        logger.LogInformation("Enviando saudação, nível {nivelAninhamento}", nivelAninhamento);

        // Incrementa o contador personalizado
        contadorSaudacoes.Add(1);

        // Adiciona uma tag à Atividade
        atividade?.SetTag("nivel-aninhamento", nivelAninhamento);

        await context.Response.WriteAsync($"Saudação Aninhada, nível: {nivelAninhamento}\r\n");

        if (nivelAninhamento > 0)
        {
            var request = context.Request;
            var url = new Uri($"{request.Scheme}://{request.Host}{request.Path}?nivelAninhamento={nivelAninhamento - 1}");

            // Realiza uma chamada http passando as informações da atividade como cabeçalhos http
            var resultadoAninhado = await clientFactory.CreateClient().GetStringAsync(url);
            await context.Response.WriteAsync(resultadoAninhado);
        }
    }
    else
    {
        // Registra uma mensagem de erro
        logger.LogError("Nível de aninhamento da saudação {nivelAninhamento} muito alto", nivelAninhamento);
        await context.Response.WriteAsync("Nível de aninhamento muito alto, o máximo é 5");
    }
}

app.MapGet("/", EnviarSaudacao);
app.MapGet("/SaudacaoAninhada", EnviarSaudacaoAninhada);

// Configura o endpoint de scraping do Prometheus
app.MapPrometheusScrapingEndpoint();

app.Run();
