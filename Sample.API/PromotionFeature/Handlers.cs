using Marten;
using Wolverine.Marten;
using static Sample.API.PromotionFeature.Promotion;
using static Sample.API.PromotionFeature.PromotionFact;

namespace Sample.API.PromotionFeature;

public sealed class RequestPromotionHandler
{
    public async Task<Guid> Handle(RequestPromotion intent, IDocumentSession session)
    {
        var id = Guid.NewGuid();

        session.Events
            .StartStream<Promotion>(id, new PromotionRequested(id, intent.Promotee));

        await session.SaveChangesAsync();

        return id;
    }
}

public static class SupervisorRespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(
        SupervisorResponds intent, Promotion promotion, ISomeRandomService someRandomService)
    {
        if(promotion is not OpenedPromotion)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }        

        if (!intent.Verdict)
        {
            yield return new RejectedBySupervisor(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }        

        ApprovedBySupervisor approved = new(intent.DecisionMadeAt);
        
        //Just showing here some concepts you can do:
        //1. You can use method injection to inject some service you might need for some business logic
        //2. You are getting the last state reconstructed in memory from the facts
        //3. You can apply a new fact and check the sate it will produce to make further decisions
        var newState = promotion.Apply(approved);
        if (newState.ApprovedBySupervisor != null)
        {
            someRandomService.DoSomething();
        }

        yield return approved;
    }
}

public static class HRRespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(HRResponds intent, Promotion promotion)
    {
        if (promotion is not PassedSupervisorApproval)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedByHR(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }

        ApprovedByHR approved = new(intent.DecisionMadeAt);
        yield return approved;
    }
}

public static class CEORespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(CEOResponds intent, Promotion promotion)
    {
        if (promotion is not PassedHRApproval)
        {
            throw new InvalidOperationException("Promotion is in an invalid state!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedByCEO(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }

        ApprovedByCEO approved = new(intent.DecisionMadeAt);
        yield return approved;
        yield return new PromotionClosedWithAcceptance();
    }
}

public sealed class RequestPromotionStatusHandler
{
    public async Task<PromotionStatus?> Handle(RequestPromotionStatus request, IQuerySession querySession)
    {
        var result = await querySession
            .Query<PromotionStatus>()
            .SingleAsync(p => p.Id == request.PromotionId);

        return result;
    }
}

public sealed class RequestPromotionDetailsHandler
{
    public async Task<PromotionDetails?> Handle(RequestPromotionDetails request, IQuerySession querySession)
    {
        var result = await querySession
            .Query<PromotionDetails>()
            .SingleAsync(p => p.Id == request.PromotionId);

        return result;
    }
}

public sealed class RequestPromotionDetailsWithVersionHandler
{
    public async Task<PromotionDetails?> Handle(RequestPromotionDetailsWithVersion request, IQuerySession querySession)
    {
        var result = await querySession
            .Events
            .AggregateStreamAsync<PromotionDetails>(request.PromotionId, request.Version);

        return result;
    }
}