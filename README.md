# Microtopia

Combination of MicroService and Utopia or how I would like a microservice to be implemented with asp.net core

## Sketch

This is how I would see a asp.net core controller/service in a microservice context (http or message queueing)

```c#
public class EmergencyService : IService, IHandle<MessageReceived>, IHandle<AlarmElapsed>
{
    private readonly IDb _db;
    private readonly IGateway _gateway;

    private TimeSpan WaitBetweenMediumMessage { get; } = TimeSpan.FromMinutes(1);

    public EmergencyService(IDb db, IGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    public Emergency Get(Emergency request)
    {
        return _db.SingleById<Emergency>(request.Id);
    }

    public Emergency Post(Emergency request)
    {
        // ...

        foreach (var medium in request.Mediums)
        {
            _gateway.Send(new Alarm
            {
                Time = now,
                Flow = new ChannelFlow {Id = request.Id, Data = new EmergencyFlow {MediumId = medium.Id}}
            });

            // ...
        }

        request.Events.Add(new EmergencyEvent {Name = "AlarmsSent"});

        _db.Save(request);

        return request;
    }

    public void Handle(MessageReceived @event)
    {
        var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

        // ...

        _gateway.Send(new AlarmCancel {Flow = new ChannelFlow {Id = emergency.Id}});

        // ...

        _db.Save(emergency);
    }

    public void Handle(AlarmElapsed @event)
    {
        var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

        if (emergency.Status == EmergencyStatuses.Done)
            return;

        // ...

        _gateway.Send(new ChannelMessage
        {
            Message = emergency.Message,
            Address = medium.Address,
            Flow = @event.Flow
        });

        // ...

        _db.Save(emergency);
    }
}
```

## Implementation : MediatR

```
[Controller]
[Route("/emergency")]
public class EmergencyService : INotificationHandler<ChannelMessageReceived>, INotificationHandler<AlarmElapsed>
{
    private readonly IDb _db;
    private readonly IMediator _gateway;

    public EmergencyService(IDb db, IMediator gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Handle(AlarmElapsed @event, CancellationToken cancellationToken)
    {
        // ReSharper disable once PossibleInvalidOperationException
        var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

        // ...

        await _gateway.Send(new ChannelMessage
        {
            Message = emergency.Message,
            Address = medium.Address,
            Flow = @event.Flow
        }, cancellationToken);

        // ...

        _db.Save(emergency);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Handle(ChannelMessageReceived @event, CancellationToken cancellationToken)
    {
        // ReSharper disable once PossibleInvalidOperationException
        var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

        // ...

        _db.Save(emergency);

        if (emergency.Status == EmergencyStatuses.Done)
            return;

        await _gateway.Send(new AlarmCancel {Flow = new ChannelFlow {Id = emergency.Id}}, cancellationToken);

        emergency.Status = EmergencyStatuses.Done;
        _db.Save(emergency);

        // ...

        await _gateway.Publish(emergencyFinished, cancellationToken);
    }

    [HttpPost]
    public async Task<Emergency> Post([FromBody] Emergency request)
    {
        // ...

        _db.Save(request);

        foreach (var medium in request.Mediums)
        {
            await _gateway.Send(new Alarm
            {
                Time = now,
                Flow = new ChannelFlow {Id = request.Id, Data = new EmergencyFlow {MediumId = medium.Id}}
            });

            // ...
        }

        // ...

        _db.Save(request);

        return request;
    }

    [HttpGet("{id}")]
    public Emergency Get(Emergency request)
    {
        return _db.SingleById<Emergency>(request.Id);
    }
}
```
