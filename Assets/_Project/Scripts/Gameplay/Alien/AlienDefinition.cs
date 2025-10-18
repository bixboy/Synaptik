using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Synaptik.Gameplay.Quests;

[CreateAssetMenu(menuName = "Synaptik/Alien/Definition", fileName = "AlienDefinition")]
public class AlienDefinition : ScriptableObject
{
    [SerializeField, ReadOnly]
    private string alienId;

    [SerializeField]
    private Emotion startEmotion = Emotion.Curious;

    [SerializeField]
    private RuntimeAnimatorController animator;

    [SerializeField]
    private ReactionMatrix reactions;

    [SerializeField]
    private DialogueDatabase dialogue;

    [SerializeField]
    private List<QuestDefinition> quests = new();

    public string AlienId => alienId;
    public Emotion StartEmotion => startEmotion;
    public RuntimeAnimatorController Animator => animator;
    public ReactionMatrix Reactions => reactions;
    public DialogueDatabase Dialogue => dialogue;
    public IReadOnlyList<QuestDefinition> Quests => quests;

    public void SetUniqueId(string id)
    {
        alienId = id;
    }
}
