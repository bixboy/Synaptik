using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Primary authoring asset for a quest. Designers can create one asset per quest
    /// and attach step definitions as sub-assets to keep data organised.
    /// </summary>
    [CreateAssetMenu(menuName = "Synaptik/Quests/Quest Definition", fileName = "QuestDefinition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField]
        private string questId;

        [SerializeField]
        private string title;

        [SerializeField, TextArea]
        private string description;

        [SerializeField]
        private bool autoRegisterMission = true;

        [SerializeField]
        private bool autoCompleteMissionOnQuestEnd = true;

        [SerializeField]
        private List<QuestStepDefinition> steps = new();

        public string QuestId => questId;
        public string Title => title;
        public string Description => description;
        public bool AutoRegisterMission => autoRegisterMission;
        public bool AutoCompleteMissionOnQuestEnd => autoCompleteMissionOnQuestEnd;
        public IReadOnlyList<QuestStepDefinition> Steps => steps;

        public QuestStepDefinition GetFirstStep()
        {
            return steps.Count > 0 ? steps[0] : null;
        }

        public bool TryGetStep(string stepId, out QuestStepDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(stepId))
            {
                definition = GetFirstStep();
                return definition != null;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null)
                {
                    continue;
                }

                if (steps[i].StepId == stepId)
                {
                    definition = steps[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public void EnsureSubAssetsAreRegistered()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    if (step != null && !step.IsSubAssetOf(this))
                    {
                        UnityEditor.AssetDatabase.AddObjectToAsset(step, this);
                    }
                }
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureSubAssetsAreRegistered();
        }
#endif
    }
}
