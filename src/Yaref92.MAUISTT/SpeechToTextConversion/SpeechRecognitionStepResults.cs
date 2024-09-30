using System.Text;

using Yaref92.Events.Abstractions;

namespace Yaref92.MAUISTT.SpeechToTextConversion;

public class SpeechRecognitionStepResults
{
    private readonly IEventAggregator _eventAggregator;

    private readonly StringBuilder _ongoingSpeechRecognitionResult = new();

    public string OngoingSpeechRecognitionText => _ongoingSpeechRecognitionResult.ToString();
    public string StoppedRecognitionText { get; private set; }

    public SpeechRecognitionStepResults(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        _eventAggregator.RegisterEventType<UpdatedSpeechToText>();
        _eventAggregator.RegisterEventType<SpeechToTextPausedAutomatically>();
    }

    public void AppendOngoing(string currentOngoingRecognitionPart)
    {
        _ongoingSpeechRecognitionResult.Append(currentOngoingRecognitionPart);
        _eventAggregator.PublishEvent(new UpdatedSpeechToText()
        {
            Text = _ongoingSpeechRecognitionResult.ToString()
        });
    }

    internal void CompleteRecognition()
    {
        StoppedRecognitionText = $"{StoppedRecognitionText}{_ongoingSpeechRecognitionResult}\n";
        _ongoingSpeechRecognitionResult.Clear();
    }

    internal void Clear()
    {
        _ongoingSpeechRecognitionResult.Clear();
        StoppedRecognitionText = "";
    }
}
