using System;
using UnityEngine;

public struct Task
{
    public string Name;
    public Emotion Emotion;
    public Behavior Behavior;
}

public class UFO : MonoBehaviour, IInteraction
{
    [SerializeField]
    private Task _task;

    private void Start()
    {
        Mission mission;
        mission.Name     = _task.Name;
        mission.IsFinish = false;
        mission.UfoRef   = this;
        
        GameManager.Instance.RegisterMission(mission);       
    }

    public void Interact(ActionValues _action)
    {
        if (_action._behavior != Behavior.None)
            return;
        
        if (_action._emotion !=  Emotion.None)
            return;

        if (_task.Behavior == _action._behavior &&  _task.Emotion == _action._emotion)
        {
            print("Test interact" + transform.parent.name);
        }
    }

    public void RegisterMission()
    {
        throw new System.NotImplementedException();
    }
}
