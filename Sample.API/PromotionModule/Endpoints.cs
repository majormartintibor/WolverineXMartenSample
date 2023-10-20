using Marten;
using Wolverine;
using Wolverine.Http;

namespace Sample.API.PromotionModule;

public static class Endpoints
{
    public static void MapPromotionEndpoints(this WebApplication app)
    {
        var promotionEndpoints = app.MapGroup("/promotion").WithOpenApi();

        promotionEndpoints.MapPost(
            "/new",
            (RequestPromotion intent, IMessageBus bus) => bus.InvokeAsync<Guid>(intent));

        promotionEndpoints.MapPut(
            "/supervisorResponse",
            (SupervisorResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPut(
            "/hrResponse",
            (HRResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPut(
            "/ceoResponse",
            (CEOResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapGet(
            "/status/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<PromotionStatus>(new RequestPromotionStatus(Id)));

        promotionEndpoints.MapGet(
            "/details/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<PromotionDetails?>(new RequestPromotionDetails(Id)));

        promotionEndpoints.MapGet(
            "/versionedDetails",
            (Guid Id, int Version, IMessageBus bus)
                => bus.InvokeAsync<PromotionDetails?>(new RequestPromotionDetailsWithVersion(Id, Version)));
    }

    /// <summary>
    /// A simple example for Wolverine.Http endpoint
    /// </summary>
    /// <param name="querySession">[NotBody] specifies that it should be resolved from IOC 
    /// and be ignored as the request body</param>
    /// <returns>List of all PromotionDetails</returns>
    [WolverineGet("/promotion/listDetails")]
    public static async Task<IReadOnlyList<PromotionDetails>> GetPromotionDetails([NotBody] IQuerySession querySession)
    {
        return await querySession.Query<PromotionDetails>().ToListAsync();        
    }

    [WolverineGet("/promotion/listStatuses")]
    public static async Task<IReadOnlyList<PromotionStatus>> GetPromotionStatuses([NotBody] IQuerySession querySession)
    {
        return await querySession.Query<PromotionStatus>().ToListAsync();
    }    
}