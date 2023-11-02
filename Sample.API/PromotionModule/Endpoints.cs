using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.Marten;
using static Sample.API.PromotionModule.Promotion;
using static Sample.API.PromotionModule.PromotionFact;

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
    /// This is an example for even less ceremony. With Wolverine 1.1 you can now get the Aggregate
    /// directly based on the supplied aggreagteID from the route. Similar as with the Aggregate Handlers
    /// the return values will be appended to the stream or sent to the appropriate queue.
    /// In case the aggregate does not yet exist a 404 response is returned.
    /// </summary>
    /// <param name="intent"></param>
    /// <param name="state"></param>
    /// <param name="someRandomService"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [WolverinePost("/promotion/supervisorResponseNew/{promotionId}"), EmptyResponse]
    public static IEnumerable<object> SupervisorResponds(
        SupervisorRespondsWithoutId intent,
        [Aggregate] Promotion state,
        ISomeRandomService someRandomService)
    {
        if (state is not OpenPromotion)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedBySupervisor(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection(state.Id);
            yield return new Contracts.PromotionExternals.Emailing.PromotionRejected(state.Promotee);
        }

        ApprovedBySupervisor approved = new(intent.DecisionMadeAt);
        yield return approved;

        var newState = state.Apply(approved);
        if (newState.ApprovedBySupervisor != null)
        {
            someRandomService.DoSomething();
        }
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