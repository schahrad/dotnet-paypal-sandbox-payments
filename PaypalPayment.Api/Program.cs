using Microsoft.AspNetCore.Mvc;
using PaypalPayment.Application.DependencyInjection;
using PaypalPayment.Application.Payments;
using PaypalPayment.Application.Usecases.Payments;
using PaypalPayment.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new {status = "OK"}));

//Endpoints: public API of service
// Create payment
app.MapPost("/api/payments", async (
    [FromBody] PaymentRequest request,
    CreatePayment createPayment,
    CancellationToken ct) =>
{
    try
    {
        var created = await createPayment.ExecuteAsync(request, ct);
        return Results.Created($"/api/payments/{created.Id}", created);
    }
    catch (PaymentGatewayException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
    }
});

// Get payment by PayPal order id
app.MapGet("/api/payments/{id}", async (
    string id,
    GetPayment getPayment,
    CancellationToken ct) =>
{
    try
    {
        var payment = await getPayment.ExecuteAsync(id, ct);
        return payment is null
            ? Results.NotFound()
            : Results.Ok(payment);
    }
    catch (PaymentGatewayException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
