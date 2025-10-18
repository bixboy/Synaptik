using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Lightweight context shared across quest runtime instances.
    /// Provides access to the quest agent, the owning behaviour and
    /// commonly used services (GameManager, etc.).
    /// </summary>
    public readonly struct QuestRuntimeContext
    {
        public QuestAgent Agent { get; }
        public MonoBehaviour OwnerBehaviour { get; }

        public GameManager GameManager => GameManager.Instance;

        public QuestRuntimeContext(QuestAgent agent, MonoBehaviour ownerBehaviour)
        {
            Agent = agent;
            OwnerBehaviour = ownerBehaviour;
        }

        public TOwner GetOwner<TOwner>() where TOwner : MonoBehaviour
        {
            return OwnerBehaviour as TOwner;
        }
    }
}
