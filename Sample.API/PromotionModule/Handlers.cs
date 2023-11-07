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
        var facts = new Events();

        if (state is not OpenPromotion)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            facts += new RejectedBySupervisor(intent.DecisionMadeAt);

            //There will be a reaction to this internal/private event you can find
            //in the PromotionClosedWithRejectionHandler. It will react to this fact
            //and do whatever logic, you can inject whatever service to the handler.
            //The advantage is that you can keep the "Decider" simple and "clean"...
            facts += new PromotionClosedWithRejection(state.Id);
            
            //...or you can do it like shown here and just directly send a message to the queue.
            //This example is simple, but maybe you would need services, have more complex logic, etc
            //and that would make it harder to reason about this method.
            messages.Add(new Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionRejected(state.Promotee));
            return (facts, messages);
        }

        ApprovedBySupervisor approved = new(intent.DecisionMadeAt);
        facts += approved;

        //Just showing here some concepts you can do:
        //1. You can use method injection to inject some service you might need for some business logic
        //2. You are getting the last state reconstructed in memory from the facts
        //3. You can apply a new fact and check the state it will produce to make further decisions
        var newState = state.Apply(approved);
        if (newState.ApprovedBySupervisor != null)
        {
            someRandomService.DoSomething();
        }

        return (facts, messages);
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
            yield return new PromotionClosedWithRejection(state.Id);
            
            //Showing here that sendig a message can also be done with
            //static IEnumerable<object> Handle signature. 
            yield return new Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionRejected(state.Promotee);
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
        var facts = new Events();

        if (state is not PassedHRApproval)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            facts += new RejectedByCEO(intent.DecisionMadeAt);
            facts += new PromotionClosedWithRejection(state.Id);
           
            messages.Add(new Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionRejected(state.Promotee));
            return (facts, messages);
        }

        ApprovedByCEO approved = new(intent.DecisionMadeAt);
        facts += approved;
        facts += new PromotionClosedWithAcceptance(state.Id);
        
        messages.Add(new Contracts.PromotionExternals.Emailing.DoEmailingStuffWhenPromotionAccepted(state.Promotee));

        return (facts, messages);
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

public static class PromotionClosedWithAcceptanceHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(PromotionClosedWithAcceptance @event, Promotion promotion)
    {
        //In real world scenario handle the failover as you wish
        if (promotion is not ApprovedPromotion)
            yield break;

        var controllingNotification = new Contracts.PromotionExternals.Controlling.DoControllingStuffWhenPromotionAccepted(
            promotion.Promotee,
            (DateTimeOffset)promotion.ApprovedBySupervisor!,
            (DateTimeOffset)promotion.ApprovedByHR!,
            (DateTimeOffset)promotion.ApprovedByCEO!);

        var marketingNotification = new Contracts.PromotionExternals.Marketing.DoMarketingStuffWhenPromotionAccepted(
            promotion.Promotee);

        yield return controllingNotification;
        yield return marketingNotification;
    }
}

public static class PromotionClosedWithRejectionHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(PromotionClosedWithRejection @event, Promotion promotion)
    {
        //In real world scenario handle the failover as you wish
        if (promotion is not RejectedPromotion)
            yield break;

        var controllingNotification = new Contracts.PromotionExternals.Controlling.DoControllingStuffWhenPromotionRejected(
            promotion.Promotee,
            (DateTimeOffset)promotion.RejectedAt!);

        var marketingNotification = new Contracts.PromotionExternals.Marketing.DoMarketingStuffWhenPromotionRejected(
            promotion.Promotee);

        yield return controllingNotification;
        yield return marketingNotification;
    }
}