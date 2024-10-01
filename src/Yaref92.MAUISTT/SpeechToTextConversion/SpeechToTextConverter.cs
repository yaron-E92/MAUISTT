using CommunityToolkit.Maui.Media;

using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.Utils;
using Yaref92.Events.Abstractions;
using System.Globalization;

namespace Yaref92.MAUISTT.SpeechToTextConversion;

public class SpeechToTextConverter : ISpeechToTextConverter
{
    public MediaRecorderState State { get; private set; }
    private readonly IEventAggregator _eventAggregator;

    private readonly ISpeechToText _speechToText;

    private SpeechRecognitionStepResults _speechRecognitionStepResults;

    public SpeechToTextConverter(ISpeechToText speechToText, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        State = MediaRecorderState.Initial;

        _speechToText = speechToText;

        _speechRecognitionStepResults = new(_eventAggregator);
        State = MediaRecorderState.Reset;
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        _speechRecognitionStepResults.AppendOngoing(args.RecognitionResult);
    }

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        _speechRecognitionStepResults.CompleteRecognition();
    }

    public async Task StartListenAsync()
    {
        if (State is MediaRecorderState.Initial)
        {
            throw new InvalidOperationException("Speech to text converter was not initialized");
        }
        if (State is MediaRecorderState.Reset or MediaRecorderState.Stopped)
        {
            await AddEventHandling();
            await _speechToText.StartListenAsync(CultureInfo.CurrentCulture, new CancellationToken());
            State = MediaRecorderState.Recording;
        }
        else
        {
            //TODO: Handle or not
        }
    }

    private async Task AddEventHandling()
    {
        await Task.Run(() =>
        {
            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        });
    }

    public async Task<string> PauseListenAsync()
    {
        await InterruptListening();
        _speechRecognitionStepResults.CompleteRecognition();
        State = MediaRecorderState.Paused;

        string stoppedRecognitionText = _speechRecognitionStepResults.StoppedRecognitionText;
        _speechRecognitionStepResults.Clear();
        return stoppedRecognitionText;
    }

    private async Task InterruptListening()
    {
        if (State is not (MediaRecorderState.Recording or MediaRecorderState.Paused))
        {
            return;
        }

        await RemoveEventHandling();
    }

    private async Task RemoveEventHandling()
    {
        await Task.Run(() =>
        {
            _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        });
    }

    public async Task ResumeListenAsync()
    {
        if (State is MediaRecorderState.Paused)
        {
            await AddEventHandling();
            State = MediaRecorderState.Recording;
        }
        else
        {
            // TODO: Handle or not
        }
    }

    public async Task<string> StopListenAsync()
    {
        await _speechToText.StopListenAsync();
        await InterruptListening();
        _speechRecognitionStepResults.CompleteRecognition();
        State = MediaRecorderState.Stopped;

        string stoppedRecognitionText = _speechRecognitionStepResults.StoppedRecognitionText;
        _speechRecognitionStepResults.Clear();
        return stoppedRecognitionText;
    }

    public void ResetConverter()
    {
        RemoveEventHandling();
        _speechRecognitionStepResults.Clear();
        State = MediaRecorderState.Reset;
    }
}
