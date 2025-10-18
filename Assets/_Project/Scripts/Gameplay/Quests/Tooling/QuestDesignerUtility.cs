#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Helper methods used by designers or build scripts to generate quest assets
    /// from external JSON/XML data. Provides a reproducible pipeline for data-driven quests.
    /// </summary>
    public static class QuestDesignerUtility
    {
        [Serializable]
        private class QuestDefinitionDto
        {
            public string questId;
            public string title;
            public string description;
            public bool autoRegisterMission = true;
            public bool autoCompleteMissionOnQuestEnd = true;
            public QuestStepDto[] steps = Array.Empty<QuestStepDto>();
        }

        [Serializable]
        private class QuestStepDto
        {
            public string stepId;
            public string type;
            public bool completesQuest;
            public string nextStepId;
            public string speakerId;
            public bool requireSpecificSpeaker;
            public string requiredItemId;
            public int requiredQuantity = 1;
        }

        public static QuestDefinition ImportFromJson(string json, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON payload cannot be empty", nameof(json));
            }

            var dto = JsonUtility.FromJson<QuestDefinitionDto>(json);
            if (dto == null)
            {
                throw new InvalidOperationException("Failed to deserialize quest definition JSON.");
            }

            var definition = ScriptableObject.CreateInstance<QuestDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);

            var definitionObject = new SerializedObject(definition);
            definitionObject.FindProperty("questId").stringValue = dto.questId;
            definitionObject.FindProperty("title").stringValue = dto.title;
            definitionObject.FindProperty("description").stringValue = dto.description;
            definitionObject.FindProperty("autoRegisterMission").boolValue = dto.autoRegisterMission;
            definitionObject.FindProperty("autoCompleteMissionOnQuestEnd").boolValue = dto.autoCompleteMissionOnQuestEnd;

            var stepsProperty = definitionObject.FindProperty("steps");
            stepsProperty.ClearArray();

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < dto.steps.Length; i++)
                {
                    var stepDto = dto.steps[i];
                    var stepAsset = CreateStepAsset(stepDto);
                    if (stepAsset == null)
                    {
                        continue;
                    }

                    AssetDatabase.AddObjectToAsset(stepAsset, definition);

                    var stepObject = new SerializedObject(stepAsset);
                    stepObject.FindProperty("stepId").stringValue = stepDto.stepId;
                    stepObject.FindProperty("completesQuest").boolValue = stepDto.completesQuest;
                    stepObject.FindProperty("nextStepId").stringValue = stepDto.nextStepId;

                    if (stepAsset is TalkQuestStepDefinition)
                    {
                        stepObject.FindProperty("expectedSignal").enumValueIndex = (int)QuestSignalType.Talk;
                        stepObject.FindProperty("requireSpecificSpeaker").boolValue = stepDto.requireSpecificSpeaker;
                        stepObject.FindProperty("speakerId").stringValue = stepDto.speakerId;
                    }
                    else if (stepAsset is GiveItemQuestStepDefinition)
                    {
                        stepObject.FindProperty("expectedSignal").enumValueIndex = (int)QuestSignalType.GiveItem;
                        stepObject.FindProperty("requiredItemId").stringValue = stepDto.requiredItemId;
                        stepObject.FindProperty("requiredQuantity").intValue = Mathf.Max(1, stepDto.requiredQuantity);
                    }

                    stepObject.ApplyModifiedPropertiesWithoutUndo();

                    stepsProperty.InsertArrayElementAtIndex(i);
                    stepsProperty.GetArrayElementAtIndex(i).objectReferenceValue = stepAsset;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            definitionObject.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return definition;
        }

        private static QuestStepDefinition CreateStepAsset(QuestStepDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.type))
            {
                Debug.LogWarning("Quest step DTO missing type information.");
                return null;
            }

            switch (dto.type.ToLowerInvariant())
            {
                case "talk":
                    return ScriptableObject.CreateInstance<TalkQuestStepDefinition>();
                case "giveitem":
                case "give_item":
                    return ScriptableObject.CreateInstance<GiveItemQuestStepDefinition>();
                default:
                    Debug.LogWarning($"Unknown quest step type '{dto.type}'.");
                    return null;
            }
        }
    }
}
#endif
