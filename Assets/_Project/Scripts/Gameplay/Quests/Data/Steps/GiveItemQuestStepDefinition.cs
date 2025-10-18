using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    [CreateAssetMenu(menuName = "Synaptik/Quests/Steps/Give Item", fileName = "GiveItemQuestStep")]
    public sealed class GiveItemQuestStepDefinition : QuestStepDefinition
    {
        [SerializeField]
        private string requiredItemId;

        [SerializeField, Min(1)]
        private int requiredQuantity = 1;

        protected override bool OnCanProgress(QuestRuntimeContext context, in QuestSignal signal)
        {
            if (!signal.TryGetPayload<GiveItemSignalPayload>(out var payload) || payload == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredItemId) && payload.ItemId != requiredItemId)
            {
                return false;
            }

            return payload.Quantity >= requiredQuantity;
        }
    }

    public sealed class GiveItemSignalPayload : IQuestSignalPayload
    {
        public string ItemId { get; }
        public int Quantity { get; }

        public GiveItemSignalPayload(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}
