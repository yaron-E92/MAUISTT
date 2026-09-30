using Yaref92.Events.Abstractions;

namespace Yaref92.MAUISTT.SpeechToTextConversion;

public class UpdatedSpeechToText : IDomainEvent
{
    public DateTime DateTimeOccurredUtc => throw new NotImplementedException();

    public string? Text { get; init; }
}
