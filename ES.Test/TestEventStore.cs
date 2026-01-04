using Eventuous;

namespace ES.Test;

public class TestEventStore : IEventStore
{
    private readonly Queue<(StreamName stream, NewStreamEvent evt, long version)> _events = new();
    public Task<AppendEventsResult> AppendEvents(StreamName stream, ExpectedStreamVersion expectedVersion, IReadOnlyCollection<NewStreamEvent> events, CancellationToken cancellationToken)
    {
        (StreamName stream, NewStreamEvent evt, long version) last = _events.LastOrDefault(x => x.stream.Equals(stream));

        if (expectedVersion == ExpectedStreamVersion.NoStream && last != default)
        {
            return Task.FromResult(AppendEventsResult.NoOp);
        }

        if (expectedVersion.Value >= 0)
        {
          /*  if (expectedVersion.Value != last.version + 1)
                return Task.FromResult(AppendEventsResult.NoOp); */
        }

        long version = last.stream == default ? -1 : last.version;

        foreach ( var evt in events)
            _events.Enqueue((stream, evt, ++version));

        return Task.FromResult(new AppendEventsResult((ulong)_events.Count, version));
    }

    public Task DeleteStream(StreamName stream, ExpectedStreamVersion expectedVersion, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEvent[]> ReadEvents(StreamName stream, StreamReadPosition start, int count, CancellationToken cancellationToken)
    {
        var result = new List<StreamEvent>();

        if (start == StreamReadPosition.End)
            return Task.FromResult<StreamEvent[]>([]);

        long position = 0;
        int readCount = 0;

        if (start != StreamReadPosition.Start) 
        {
            position = start.Value;
        }
        
        while (position < _events.Count && readCount < count)
        {
            var x = _events.Skip((int)position).First();
            position++;

            if (stream != "-" && x.stream != stream)
                continue;

            result.Add(new StreamEvent(x.evt.Id, x.evt.Payload, x.evt.Metadata, "", position));
            readCount++;
        }

        return Task.FromResult(result.ToArray());
    }

    public Task<StreamEvent[]> ReadEventsBackwards(StreamName stream, StreamReadPosition start, int count, CancellationToken cancellationToken)
    {
        var result = new List<StreamEvent>();

        if (start == StreamReadPosition.Start)
            return Task.FromResult<StreamEvent[]>([]);

        long position = _events.Count - 1;
        int readCount = 0;

        if (start != StreamReadPosition.End)
        {
            position = start.Value;
        }

        while (position > 0 && readCount < count)
        {
            var x = _events.Skip((int)position).First();
            position--;
            if (x.stream != stream)
                continue;

            result.Add(new StreamEvent(x.evt.Id, x.evt.Payload, x.evt.Metadata, "", position));
            readCount++;
        }

        return Task.FromResult(result.ToArray());
    }

    public Task<bool> StreamExists(StreamName stream, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events.Any(x => x.stream == stream));
    }

    public Task TruncateStream(StreamName stream, StreamTruncatePosition truncatePosition, ExpectedStreamVersion expectedVersion, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}