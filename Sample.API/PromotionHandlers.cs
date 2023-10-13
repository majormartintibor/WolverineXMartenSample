using Marten;
using Wolverine.Marten;

namespace Sample.API;

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
    public static IEnumerable<object> Handle(SupervisorResponds intent, Promotion promotion)
    { 
        if (promotion.Closed)
        {
            throw new InvalidOperationException("The promoting process is already closed!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedBySupervisor(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }

        ApprovedBySupervisor approved = new(intent.DecisionMadeAt);
        yield return approved;

        promotion.Apply(approved);

        if (promotion.ApprovedByAll)
        {
            yield return new PromotionClosedWithAcceptance();
        }
    }
}

public static class HRRespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(HRResponds intent, Promotion promotion)
    {
        if (promotion.Closed)
        {
            throw new InvalidOperationException("The promoting process is already closed!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedByHR(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }
        
        ApprovedByHR approved = new(intent.DecisionMadeAt);
        yield return approved;

        promotion.Apply(approved);

        if (promotion.ApprovedByAll)
        {
            yield return new PromotionClosedWithAcceptance();
        }
    }
}

public static class CEORespondsHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(CEOResponds intent, Promotion promotion)
    {
        if (promotion.Closed)
        {
            throw new InvalidOperationException("The promoting process is already closed!");
        }

        if (!intent.Verdict)
        {
            yield return new RejectedByCEO(intent.DecisionMadeAt);
            yield return new PromotionClosedWithRejection();
            yield break;
        }       

        ApprovedByCEO approved = new(intent.DecisionMadeAt);
        yield return approved;

        promotion.Apply(approved);

        if (promotion.ApprovedByAll)
        {
            yield return new PromotionClosedWithAcceptance();
        }
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