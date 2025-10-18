using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Component responsible for instantiating and updating quest runtimes for a given actor.
    /// It is intentionally lightweight so it can be reused by any gameplay entity.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuestAgent : MonoBehaviour
    {
        [SerializeField]
        private List<QuestDefinition> questDefinitions = new();

        private readonly Dictionary<string, QuestRuntime> _runtimes = new();
        private QuestRuntimeContext _context;
        private MonoBehaviour _owner;

        public IReadOnlyDictionary<string, QuestRuntime> Runtimes => _runtimes;
        public event Action<QuestRuntime> OnQuestStarted;
        public event Action<QuestRuntime> OnQuestCompleted;
        public event Action<QuestRuntime, IQuestStepInstance> OnStepStarted;
        public event Action<QuestRuntime, IQuestStepInstance> OnStepCompleted;

        public void Initialise(MonoBehaviour owner)
        {
            _owner = owner;
            _context = new QuestRuntimeContext(this, owner);

            InitialiseDefinitions();
        }

        public void AssignDefinitions(IEnumerable<QuestDefinition> definitions)
        {
            questDefinitions.Clear();
            if (definitions != null)
            {
                questDefinitions.AddRange(definitions);
            }
            InitialiseDefinitions();
        }

        public bool TryHandleSignal(in QuestSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.QuestId))
            {
                return false;
            }

            if (!_runtimes.TryGetValue(signal.QuestId, out var runtime))
            {
                return false;
            }

            return runtime.TryProcessSignal(signal);
        }

        public bool IsStepActive(string questId, string stepId, QuestSignalType signalType)
        {
            return _runtimes.TryGetValue(questId, out var runtime) && runtime.IsStepActive(stepId, signalType);
        }

        private void InitialiseDefinitions()
        {
            foreach (var runtime in _runtimes.Values)
            {
                runtime.OnQuestCompleted -= HandleQuestCompleted;
                runtime.OnStepStarted -= HandleStepStarted;
                runtime.OnStepCompleted -= HandleStepCompleted;
            }

            _runtimes.Clear();

            foreach (var definition in questDefinitions)
            {
                RegisterDefinition(definition);
            }
        }

        public void RegisterDefinition(QuestDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(definition.QuestId))
            {
                Debug.LogWarning($"[QuestAgent] Quest definition on {name} has no identifier and will be ignored.");
                return;
            }

            if (_runtimes.ContainsKey(definition.QuestId))
            {
                Debug.LogWarning($"[QuestAgent] Duplicate quest id '{definition.QuestId}' ignored on {name}.");
                return;
            }

            var runtime = new QuestRuntime(definition, _context);
            runtime.OnQuestCompleted += HandleQuestCompleted;
            runtime.OnStepStarted += HandleStepStarted;
            runtime.OnStepCompleted += HandleStepCompleted;

            _runtimes.Add(definition.QuestId, runtime);

            if (definition.AutoRegisterMission && _context.GameManager != null)
            {
                _context.GameManager.RegisterMission(new Mission(definition.QuestId, definition.Title, definition.Description));
            }

            runtime.Start();
            OnQuestStarted?.Invoke(runtime);
        }

        private void HandleQuestCompleted(QuestRuntime runtime)
        {
            if (runtime.Definition.AutoCompleteMissionOnQuestEnd && _context.GameManager != null)
            {
                var missionTarget = ResolveMissionTarget();
                _context.GameManager.SetMissionFinished(runtime.Definition.QuestId, missionTarget);
            }

            OnQuestCompleted?.Invoke(runtime);
        }

        private void HandleStepStarted(QuestRuntime runtime, IQuestStepInstance step)
        {
            OnStepStarted?.Invoke(runtime, step);
        }

        private void HandleStepCompleted(QuestRuntime runtime, IQuestStepInstance step)
        {
            OnStepCompleted?.Invoke(runtime, step);
        }

        public IEnumerable<QuestRuntime> EnumerateRuntimes()
        {
            return _runtimes.Values;
        }

        private AlienDefinition ResolveMissionTarget()
        {
            if (_owner is IQuestMissionTarget directTarget && directTarget.MissionAlienDefinition != null)
            {
                return directTarget.MissionAlienDefinition;
            }

            if (TryGetComponent<IQuestMissionTarget>(out var componentTarget) && componentTarget.MissionAlienDefinition != null)
            {
                return componentTarget.MissionAlienDefinition;
            }

            return null;
        }
    }
}
