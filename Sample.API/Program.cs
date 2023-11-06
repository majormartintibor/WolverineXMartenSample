using JasperFx.Core;
using Marten;
using Marten.Exceptions;
using Npgsql;
using Sample.API;
using Sample.API.PromotionModule;
using Weasel.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

// Adding Marten
builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")!;
    opts.Connection(connectionString);

    opts.AutoCreateSchemaObjects = AutoCreate.All;
    opts.DatabaseSchemaName = "promotion";    

    opts.AddPromotionProjections();
}).UseLightweightSessions()

    // Adding the Wolverine integration for Marten.
    .IntegrateWithWolverine()
    .EventForwardingToWolverine();

//LocalQueue subscriptions could be moved to an extension method.
builder.Host.UseWolverine(opts =>
{
    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Controlling.DoControllingStuffWhenPromotionAccepted>()
        .ToLocalQueue("promotionexternals.controlling.promotionaccepted")
        .UseDurableInbox();

    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Controlling.DoControllingStuffWhenPromotionRejected>()
        .ToLocalQueue("promotionexternals.controlling.promotionrejected")
        .UseDurableInbox();

    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Marketing.DoMarketingStuffWhenPromotionAccepted>()
        .ToLocalQueue("promotionexternals.marketing.promotionaccepted")
        .UseDurableInbox();

    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Marketing.DoMarketingStuffWhenPromotionRejected>()
        .ToLocalQueue("promotionexternals.marketing.promotionrejected")
        .UseDurableInbox();

    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionRejected>()
        .ToLocalQueue("promotionexternals.emailing.promotionrejected")
        .UseDurableInbox();

    opts.PublishMessage<Sample.API.Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionAccepted>()
        .ToLocalQueue("promotionexternals.emailing.promotionaccepted")
        .UseDurableInbox();

    // Retry policies if a Marten concurrency exception is encountered
    opts.OnException<ConcurrencyException>()
        .RetryOnce()
        .Then.RetryWithCooldown(100.Milliseconds(), 250.Milliseconds())
        .Then.Discard();

    //Retry for infrastructure hiccups
    opts.Policies.OnException<NpgsqlException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());    

    // Automatic usage of transactional middleware as 
    // Wolverine recognizes that an HTTP endpoint or message handler
    // persists data
    opts.Policies.AutoApplyTransactions();

    opts.UseFluentValidation();    

    opts.Services.AddSingleton(typeof(IFailureAction<>), typeof(CustomFailureAction<>));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDateTimeOffsetProvider, DateTimeOffsetProvider>();
builder.Services.AddScoped<ISomeRandomService, SomeRandomService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPromotionEndpoints();
app.MapWolverineEndpoints();

await app.RunAsync();