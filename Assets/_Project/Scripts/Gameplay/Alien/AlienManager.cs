using System.Collections.Generic;
using Synaptik.Game;
using UnityEngine;

public class AlienManager : MonoBehaviour
{
    private List<Alien> _aliens = new List<Alien>();
    
    
    public static AlienManager Instance;
    private bool _initialized = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    public void RegisterAlien(Alien alien)
    {
        if (alien != null && !_aliens.Contains(alien))
        {
            _aliens.Add(alien);
        }
    }
    
    public void UnregisterAlien(Alien alien)
    {
        if (alien != null && _aliens.Contains(alien))
        {
            _aliens.Remove(alien);
        }
    }
    
    
    private void SetAliensUniqueIds()
    {
        for (int i = 0; i < _aliens.Count; i++)
        {
            var alien = _aliens[i];
            if (alien != null && alien.Definition != null)
            {
                string uniqueId = (i + 1).ToString();
                alien.Definition.SetUniqueId(uniqueId);
            }
        }
    }
    void Update()
    {
        if (!_initialized)
        {
            _initialized = true;
            SetAliensUniqueIds();
        }
    }
    
    
}
