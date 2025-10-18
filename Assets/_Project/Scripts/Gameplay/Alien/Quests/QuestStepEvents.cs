using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Designer-friendly UnityEvents invoked when a quest step changes lifecycle states.
/// Allows binding VFX, audio, UI, and other reactions without custom code.
/// </summary>
[Serializable]
public sealed class QuestStepEvents
{
    [SerializeField]
    private UnityEvent onStepActivated = new UnityEvent();

    [SerializeField]
    private UnityEvent onStepCompleted = new UnityEvent();

    [SerializeField]
    private UnityEvent onStepFailed = new UnityEvent();

    public static QuestStepEvents Empty
    {
        get
        {
            _empty ??= new QuestStepEvents();
            return _empty;
        }
    }

    private static QuestStepEvents _empty;

    public QuestStepEvents()
    {
        EnsureEvents();
    }

    /// <summary>
    /// Invoked when the step becomes the active objective for the player.
    /// </summary>
    public void InvokeActivated()
    {
        EnsureEvents();
        onStepActivated.Invoke();
    }

    /// <summary>
    /// Invoked when the step successfully completes.
    /// </summary>
    public void InvokeCompleted()
    {
        EnsureEvents();
        onStepCompleted.Invoke();
    }

    /// <summary>
    /// Invoked when the step is failed or skipped.
    /// </summary>
    public void InvokeFailed()
    {
        EnsureEvents();
        onStepFailed.Invoke();
    }

    private void EnsureEvents()
    {
        onStepActivated ??= new UnityEvent();
        onStepCompleted ??= new UnityEvent();
        onStepFailed ??= new UnityEvent();
    }
}
