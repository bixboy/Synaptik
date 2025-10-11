using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public struct InterractionRule
{
    [SerializeField]
    private Behavior channel;

    [SerializeField]
    private Emotion playerEmotion;

    [SerializeField]
    private string questId;

    [SerializeField]
    private string questStepId;

    [SerializeField]
    private int suspicionDelta;

    [SerializeField]
    private bool setNewEmotion;

    [SerializeField]
    private Emotion newEmotion;

    public Behavior Channel => channel;
    public Emotion PlayerEmotion => playerEmotion;
    public string QuestId => questId;
    public string QuestStepId => questStepId;
    public int SuspicionDelta => suspicionDelta;
    public bool SetNewEmotion => setNewEmotion;
    public Emotion NewEmotion => newEmotion;
}

[Serializable]
public struct ItemRule
{
    [Header("Item Reaction")]
    [SerializeField]
    private string questId;

    [SerializeField]
    private string questStepId;

    [SerializeField]
    private string expectedItemId;

    [SerializeField]
    private int suspicionDelta;

    [SerializeField]
    private int expectedItemQuantity;

    [SerializeField]
    private bool setIfGoodItem;

    [SerializeField]
    private Emotion newEmotionIfGoodItem;

    public string QuestId => questId;
    public string QuestStepId => questStepId;
    public string ExpectedItemId => expectedItemId;
    public int SuspicionDelta => suspicionDelta;
    public int ExpectedItemQuantity => expectedItemQuantity <= 0 ? 1 : expectedItemQuantity;
    public bool SetIfGoodItem => setIfGoodItem;
    public Emotion NewEmotionIfGoodItem => newEmotionIfGoodItem;
}
