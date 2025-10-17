using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

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
    private List<AlienQuest> quests = new();

    public string AlienId => alienId;
    public Emotion StartEmotion => startEmotion;
    public RuntimeAnimatorController Animator => animator;
    public ReactionMatrix Reactions => reactions;
    public DialogueDatabase Dialogue => dialogue;
    public List<AlienQuest> Quests => quests;

    public void SetUniqueId(string id)
    {
        alienId = id;
    }
    
    private void OnEnable()
    {
        foreach (var quest in Quests)
        {
            quest.SetAlien(this);
        }
    }
}
