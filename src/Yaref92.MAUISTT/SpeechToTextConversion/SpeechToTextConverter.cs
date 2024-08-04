using CommunityToolkit.Maui.Media;

using Yaref92.MAUISTT.Abstractions;
using System.Text;
using Yaref92.MAUISTT.Utils;
using Yaref92.Events.Abstractions;
using System.Globalization;

namespace Yaref92.MAUISTT.SpeechToTextConversion;

public class SpeechToTextConverter : ISpeechToTextConverter
{
    public MediaRecorderState State { get; private set; }
    private readonly IEventAggregator _eventAggregator;

    private readonly StringBuilder _interruptedSpeechRecognitionResult = new();
    private readonly StringBuilder _ongoingSpeechRecognitionResult = new();
    private readonly ISpeechToText _speechToText;

    public SpeechToTextConverter(ISpeechToText speechToText, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        State = MediaRecorderState.Initial;

        _speechToText = speechToText;
        _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

        _eventAggregator.RegisterEventType<UpdatedSpeechToText>();
        _eventAggregator.RegisterEventType<SpeechToTextPausedAutomatically>();
        State = MediaRecorderState.Reset;
    }

    /// <summary>
    /// This method is necessary for the situation when the speech to text stopped listening but didn't pick up anything or
    /// didn't trigger <see cref="OnRecognitionTextCompleted"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SpeechToTextConverter_EndOfSpeech(object? sender, EventArgs e)
    {
        if (State == MediaRecorderState.Recording)
        {
            string recognitionResult = _ongoingSpeechRecognitionResult.ToString();
            if (string.IsNullOrEmpty(recognitionResult))
            {
                State = MediaRecorderState.Stopped;
            }
            else
            {
                OnRecognitionTextCompleted(this, new SpeechToTextRecognitionResultCompletedEventArgs(recognitionResult));
            }
        }
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        _ongoingSpeechRecognitionResult.Append(args.RecognitionResult);
        _eventAggregator.PublishEvent(new UpdatedSpeechToText()
        {
            Text = _ongoingSpeechRecognitionResult.ToString()
        });
    }

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        _interruptedSpeechRecognitionResult.Clear().Append(args.RecognitionResult);
        _ongoingSpeechRecognitionResult.Clear();

        if (State == MediaRecorderState.Recording)
        {
            _eventAggregator.PublishEvent(new SpeechToTextPausedAutomatically()
            {
                Text = _interruptedSpeechRecognitionResult.ToString()
            });
            State = MediaRecorderState.Paused;
        }
    }

    public async Task StartListenAsync()
    {
        if (State is MediaRecorderState.Initial)
        {
            throw new InvalidOperationException("Speech to text converter was not initialized");
        }
        if (State is MediaRecorderState.Reset or MediaRecorderState.Stopped)
        {
            await _speechToText.StartListenAsync(CultureInfo.CurrentCulture, new CancellationToken());
            State = MediaRecorderState.Recording;
        }
        else
        {
            //TODO: Handle or not
        }
    }

    public async Task<string> PauseListenAsync()
    {
        string result = await InterruptListening();
        State = MediaRecorderState.Paused;

        return result;
    }

    private async Task<string> InterruptListening()
    {
        if (State is not MediaRecorderState.Recording)
        {
            return string.Empty;
        }

        await _speechToText.StopListenAsync();
        string interruptedString = _ongoingSpeechRecognitionResult.ToString();
        _ongoingSpeechRecognitionResult.Clear();
        return interruptedString;
    }

    public async Task ResumeListenAsync()
    {
        if (State is MediaRecorderState.Paused)
        {
            _ongoingSpeechRecognitionResult.Append(' ');
            await _speechToText.StartListenAsync(CultureInfo.CurrentCulture, new CancellationToken());
            State = MediaRecorderState.Recording;
        }
        else
        {
            // TODO: Handle or not
        }
    }

    public async Task<string> StopListenAsync()
    {
        string result = await InterruptListening();
        _interruptedSpeechRecognitionResult.Clear();
        State = MediaRecorderState.Stopped;

        return result;
    }

    public void ResetConverter()
    {
        _ongoingSpeechRecognitionResult.Clear();
        _interruptedSpeechRecognitionResult.Clear();
        State = MediaRecorderState.Reset;
    }
}
