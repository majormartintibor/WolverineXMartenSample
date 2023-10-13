using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Sample.API;

public static class Endpoints
{
    public static void MapPromotionEndpoints(this WebApplication app)
    {
        var promotionEndpoints = app.MapGroup("/promotion").WithOpenApi();

        promotionEndpoints.MapPost(
            "/new",
            (RequestPromotion intent, IMessageBus bus) => bus.InvokeAsync<Guid>(intent));

        promotionEndpoints.MapPut(
            "/SupervisorResponse",
            (SupervisorResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPut(
            "/HRResponse",
            (HRResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPut(
            "/CEOResponse",
            (CEOResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapGet(
            "/Status/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<PromotionStatus>(new RequestPromotionStatus(Id)));

        promotionEndpoints.MapGet(
            "/Details/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<PromotionDetails?>(new RequestPromotionDetails(Id)));

        promotionEndpoints.MapGet(
            "/VersionedDetails",            
            (Guid Id, int Version, IMessageBus bus)
                => bus.InvokeAsync<PromotionDetails?>(new RequestPromotionDetailsWithVersion(Id, Version)));
    }
}