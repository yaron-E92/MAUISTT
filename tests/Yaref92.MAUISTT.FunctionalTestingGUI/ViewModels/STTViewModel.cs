using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Yaref92.Events.Abstractions;
using Yaref92.MAUISTT.Abstractions;
using Yaref92.MAUISTT.SpeechToTextConversion;
using Yaref92.MAUISTT.UI;

namespace Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

public partial class STTViewModel : STTViewModelBase, IEventSubscriber<UpdatedSpeechToText>, IEventSubscriber<SpeechToTextPausedAutomatically>
{
    public ISubscription Subscription { get; }

    private readonly IEventAggregator _eventAggregator;
    private readonly ISpeechToTextConverter _speechToTextConverter;
    private bool _intialized = false;

    public STTViewModel(ISubscription subscription, IEventAggregator eventAggregator, ISpeechToTextConverter speechToTextConverter)
    {
        Subscription = subscription;
        _eventAggregator = eventAggregator;
        _speechToTextConverter = speechToTextConverter;
        SubscribeToSTTEvents();
        InitTimer();
        timer.Tick += CheckOngoingSTT;
    }

    private void SubscribeToSTTEvents()
    {
        _eventAggregator.SubscribeToEventType<UpdatedSpeechToText>(this);
        _eventAggregator.SubscribeToEventType<SpeechToTextPausedAutomatically>(this);
    }

    public void OnCompleted()
    {
        return; // Do nothing
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(UpdatedSpeechToText value)
    {
        OngoingSTTText = value.Text;
    }

    public void OnNext(SpeechToTextPausedAutomatically value)
    {
        _editableStringBuilder.Append(value.Text);
        ApplyPause();
    }

    private void ApplyPause()
    {
        OngoingSTTText = "";
        IsPaused = true;
        IsTextEditable = true;
        timer.Stop();
    }

    [RelayCommand]
    protected override async Task StartListening()
    {
        if (!_intialized)
        {
            _editableStringBuilder = new();
            await GrantMicrophonePermissionIfNecessary();
            await GrantSpeechRecognitionPermissionIfNecessary();
            _intialized = true;
        }
        await _speechToTextConverter.StartListenAsync();
        recordingLength = TimeSpan.Zero;
        timer.Start();
        IsTextEditable = false;
        IsRecording = true;
    }

    [RelayCommand]
    protected override async Task PauseListening()
    {
        string editable = await _speechToTextConverter.PauseListenAsync();
        UpdateEditable(editable);
        _emptinessCounter = 0;
        ApplyPause();
    }

    private void UpdateEditable(string editable)
    {
        _editableStringBuilder.Append(editable);
        EditableSTTText = _editableStringBuilder.ToString();
    }

    [RelayCommand]
    protected override async Task ResumeListening()
    {
        await _speechToTextConverter.ResumeListenAsync();
        timer.Start();
        IsTextEditable = false;
        IsPaused = false;
    }

    [RelayCommand]
    protected override async Task StopListening()
    {
        string editable = await _speechToTextConverter.StopListenAsync();
        UpdateEditable(editable);
        OngoingSTTText = "";
        _emptinessCounter = 0;
        IsPaused = false;
        IsRecording = false;
        IsTextEditable = true;
        timer.Stop();
        CurrentAudioPostion = string.Format("{0:mm\\:ss}", TimeSpan.Zero);
    }

    internal void ReplaceEditable(string newTextValue)
    {
        if (EditableSTTText != newTextValue)
        {
            _editableStringBuilder.Clear().Append(newTextValue);
            EditableSTTText = _editableStringBuilder.ToString(); 
        }
    }
}
