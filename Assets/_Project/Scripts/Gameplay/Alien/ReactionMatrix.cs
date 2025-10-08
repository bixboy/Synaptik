using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Synaptik/Alien/Reaction Matrix", fileName = "ReactionMatrix")]
public class ReactionMatrix : ScriptableObject
{

    [SerializeField] private Rule[] _rules = Array.Empty<Rule>();

    public bool TryFindRule(Behavior channel, Emotion playerEmotion, out Rule rule)
    {
        var rules = _rules;
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

    public bool TryFindItemRule(string itemId, out Rule rule)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            rule = default;
            return false;
        }

        var rules = _rules;
        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].DependsOnItem && rules[i].ExpectedItemId == itemId)
            {
                rule = rules[i];
                return true;
            }
        }

        rule = default;
        return false;
    }
}
