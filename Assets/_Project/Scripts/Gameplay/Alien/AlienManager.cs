using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Gameplay.Alien
{
    public sealed class AlienManager : MonoBehaviour
    {
        public static AlienManager Instance { get; private set; }

        private readonly List<Alien> aliens = new();
        private bool initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            SetAliensUniqueIds();
        }

        public void RegisterAlien(Alien alien)
        {
            if (alien != null && !aliens.Contains(alien))
            {
                aliens.Add(alien);
            }
        }

        public void UnregisterAlien(Alien alien)
        {
            if (alien != null)
            {
                aliens.Remove(alien);
            }
        }

        private void SetAliensUniqueIds()
        {
            for (var i = 0; i < aliens.Count; i++)
            {
                var alien = aliens[i];
                if (alien == null || alien.Definition == null)
                {
                    continue;
                }

                var uniqueId = (i + 1).ToString();
                alien.Definition.SetUniqueId(uniqueId);
            }
        }
    }
}
