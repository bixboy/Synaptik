using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(menuName = "Synaptik/Alien/Reaction Matrix", fileName = "ReactionMatrix")]
public class ReactionMatrix : ScriptableObject
{

    [SerializeField] private InterractionRule[] _interrationRules = Array.Empty<InterractionRule>();
    [SerializeField] private ItemRule[] _itemRules = Array.Empty<ItemRule>();
    

    public bool TryFindRule(Behavior channel, Emotion playerEmotion, out InterractionRule rule)
    {
        var rules = _interrationRules;
        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].Channel == channel && rules[i].PlayerEmotion == playerEmotion)
            {
                rule = rules[i];
                return true;
            }
        }

        rule = default;
        return false;
    }

    public bool TryFindItemRule(string itemId, out ItemRule rule)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            rule = default;
            return false;
        }

        var rules = _itemRules;
        for (int i = 0; i < rules.Length; i++)
        {
            if (_itemRules[i].ExpectedItemId == itemId)
            {
                rule = rules[i];
                return true;
            }
        }

        rule = default;
        return false;
    }
}
