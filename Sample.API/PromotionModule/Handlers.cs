using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;
using Wolverine.Marten;
using static Sample.API.PromotionModule.Promotion;
using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.PromotionModule;

public static class RequestPromotionHandler
{
    public static async Task<Guid> Handle(RequestPromotion request, IDocumentSession session)
    {
        var id = Guid.NewGuid();

        session.Events
            .StartStream<Promotion>(id, new PromotionOpened(id, request.Promotee));

        await session.SaveChangesAsync();

        return id;
    }
}

public static class SupervisorRespondsHandler
{
    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(
        SupervisorResponds intent, Promotion state, ISomeRandomService someRandomService)
    {
        var messages = new OutgoingMessages();
        var events = new Events();

        if (state is not OpenedPromotion)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            events += new RejectedBySupervisor(intent.DecisionMadeAt);
            events += new PromotionClosedWithRejection();
            
            messages.Add(new PromotionRejected(state.Id));
            return (events, messages);
        }

        ApprovedBySupervisor approved = new(intent.DecisionMadeAt);
        events += approved;

        //Just showing here some concepts you can do:
        //1. You can use method injection to inject some service you might need for some business logic
        //2. You are getting the last state reconstructed in memory from the facts
        //3. You can apply a new fact and check the state it will produce to make further decisions
        var newState = state.Apply(approved);
        if (newState.ApprovedBySupervisor != null)
        {
            someRandomService.DoSomething();
        }

        return (events, messages);
    }
}

public static class HRRespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(HRResponds intent, Promotion state)
    {
        if (state is not PassedSupervisorApproval)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedByHR(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            
            //Showing here that sendig a message can also be done with
            //static IEnumerable<object> Handle signature. 
            yield return new PromotionRejected(state.Id);
            yield break;
        }

        ApprovedByHR approved = new(intent.DecisionMadeAt);
        yield return approved;
    }
}

public static class CEORespondsHandler
{
    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(CEOResponds intent, Promotion state)
    {
        var messages = new OutgoingMessages();
        var events = new Events();

        if (state is not PassedHRApproval)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            events += new RejectedByCEO(intent.DecisionMadeAt);
            events += new PromotionClosedWithRejection();
           
            messages.Add(new PromotionRejected(state.Id));
            return (events, messages);
        }

        ApprovedByCEO approved = new(intent.DecisionMadeAt);
        events += approved;
        events += new PromotionClosedWithAcceptance();
        
        messages.Add(new PromotionAccepted(state.Id));

        return (events, messages);
    }
}

public static class RequestPromotionStatusHandler
{
    public static async Task<Results<Ok<PromotionStatus>, NotFound>> Handle(
        RequestPromotionStatus request, IQuerySession querySession)
    {
        var result = await querySession
            .Query<PromotionStatus>()
            .SingleOrDefaultAsync(p => p.Id == request.PromotionId);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}

public static class RequestPromotionDetailsHandler
{
    public static async Task<Results<Ok<PromotionDetails>, NotFound>> Handle(
        RequestPromotionDetails request, IQuerySession querySession)
    {
        var result = await querySession
            .Query<PromotionDetails>()
            .SingleOrDefaultAsync(p => p.Id == request.PromotionId);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}

public static class RequestPromotionDetailsWithVersionHandler
{
    public static async Task<Results<Ok<PromotionDetails>, NotFound>> Handle(
        RequestPromotionDetailsWithVersion request, IQuerySession querySession)
    {
        var result = await querySession
            .Events
            .AggregateStreamAsync<PromotionDetails>(request.PromotionId, request.Version);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}

public static class PromotionAcceptedHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(PromotionAccepted @event, Promotion promotion)
    {
        //In real world scenario handle the failover as you wish
        if (promotion is not ApprovedPromotion)
            yield break;

        var controllingNotification = new Contracts.PromotionExternals.Controlling.PromotionAccepted(
            promotion.Promotee,
            (DateTimeOffset)promotion.ApprovedBySupervisor!,
            (DateTimeOffset)promotion.ApprovedByHR!,
            (DateTimeOffset)promotion.ApprovedByCEO!);

        var marketingNotification = new Contracts.PromotionExternals.Marketing.PromotionAccepted(
            promotion.Promotee);

        yield return controllingNotification;
        yield return marketingNotification;
    }
}

public static class PromotionRejectedHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(PromotionRejected @event, Promotion promotion)
    {
        //In real world scenario handle the failover as you wish
        if (promotion is not RejectedPromotion)
            yield break;        

        var controllingNotification = new Contracts.PromotionExternals.Controlling.PromotionRejected(
            promotion.Promotee,
            (DateTimeOffset)promotion.RejectedAt!);

        var marketingNotification = new Contracts.PromotionExternals.Marketing.PromotionRejected(
            promotion.Promotee);

        yield return controllingNotification;
        yield return marketingNotification;
    }
}