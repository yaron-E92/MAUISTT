using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Yaref92.MAUISTT.UI;

/// <summary>
/// A base class for view models which use MAUISTT's audio input
/// capabilities.
/// Inherits from <seealso cref="ObservableObject"/>
/// </summary>
public abstract partial class STTViewModelBase : AudioInputViewModelBase
{

    [ObservableProperty]
    protected string? _ongoingSTTText;

    protected StringBuilder _editableStringBuilder;

    [ObservableProperty]
    protected string _editableSTTText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTextReadOnly))]
    protected bool _isTextEditable = false;

    protected int _emptinessCounter = 0;

    public bool IsTextReadOnly => !IsTextEditable;

    protected void CheckOngoingSTT(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (string.IsNullOrEmpty(OngoingSTTText))
            {
                _emptinessCounter++;
            }
            else
            {
                _emptinessCounter = 0;
            }

            if (_emptinessCounter > 6)
            {
                await StopListening();
            }
        });
    }

    protected abstract Task StartListening();
    protected abstract Task PauseListening();
    protected abstract Task ResumeListening();
    protected abstract Task StopListening();
}
