using UnityEngine;

[System.Serializable]
public struct Task
{
    public string Name;
    public Emotion Emotion;
    public Behavior Behavior;
}

public class UFO : MonoBehaviour, IInteraction
{
    public delegate void UfoInteractHandler(UFO ufo, ActionValues action);
    public event UfoInteractHandler OnUfoInteract;

    
    [SerializeField] private Task _task;
    [SerializeField] private GameObject dialoguePrefab;
    [SerializeField] private Transform bubbleAnchor; 

    private void Start()
    {
        Mission mission;
        mission.Name       = _task.Name;
        mission.IsFinished = false;
        mission.UfoRef     = this;
        
        GameManager.Instance.RegisterMission(mission);       
    }

    public void Interact(ActionValues action)
    {
        if (_task.Behavior == action._behavior &&  _task.Emotion == action._emotion)
        {
            OnUfoInteract?.Invoke(this, action);
            
            print("Test interact" + transform.parent.name);
            SpawnDialogueBubble("Mission accomplie !");
        }
    }
    
    private void SpawnDialogueBubble(string text)
    {
        if (dialoguePrefab == null || bubbleAnchor == null)
        {
            Debug.LogWarning("Dialogue prefab ou anchor non assigné sur " + name);
            return;
        }

        GameObject bubble = Instantiate(dialoguePrefab, bubbleAnchor.position, Quaternion.identity);
        bubble.GetComponent<DialogueBubble>().SetText(text);
    }

    public void RegisterMission()
    {
        throw new System.NotImplementedException();
    }
}
