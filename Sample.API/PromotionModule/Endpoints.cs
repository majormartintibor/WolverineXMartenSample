using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
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

        //I did put an int Version property in the Responds record below. The reason is to easily showcase
        //concurrency exception in the demo. You could completly skip receiving the version number and
        //Marten would just +1 it, if two different changes to the stream would happen at the smae time
        //you would still get the concurrency check and exception, but it would be harder to demo.
        //You could also use ETag header for optimistic concurrency:
        //https://event-driven.io/en/how_to_use_etag_header_for_optimistic_concurrency/
        //However this is not scope of this demo and will be ignored for now.
        promotionEndpoints.MapPost(
            "/supervisorResponse",
            (SupervisorResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPost(
            "/hrResponse",
            (HRResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapPost(
            "/ceoResponse",
            (CEOResponds intent, IMessageBus bus) => bus.InvokeAsync(intent));

        promotionEndpoints.MapGet(
            "/status/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<Results<Ok<PromotionStatus>, NotFound>>(new RequestPromotionStatus(Id)));

        promotionEndpoints.MapGet(
            "/details/{id}",
            (Guid Id, IMessageBus bus) => bus.InvokeAsync<Results<Ok<PromotionDetails>, NotFound>>(new RequestPromotionDetails(Id)));

        promotionEndpoints.MapGet(
            "/versionedDetails",
            (Guid Id, int Version, IMessageBus bus)
                => bus.InvokeAsync<Results<Ok<PromotionDetails>, NotFound>>(new RequestPromotionDetailsWithVersion(Id, Version)));
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