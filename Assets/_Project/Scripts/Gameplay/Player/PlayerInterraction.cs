using System;
using UnityEngine;

public class PlayerInterraction : MonoBehaviour
{
    
    [Header("PickUp settings")]
    [SerializeField] private Transform _handSocket;         // vide placé dans la main
    [SerializeField] private float _pickupRadius = 1.2f;    // portée
    [SerializeField] private LayerMask _pickupMask = ~0;    // couche(s) des objets
    [SerializeField] private float _dropForwardSpeed = 0f;  // 0 = lâcher sans lancer

    private HoldableItem _held;

    void Start()
    {
        InputsDetection.Instance.OnEmotionAction += Interact;
    }
    
    public void Interact(Emotion emotion, Behavior action)
    {
        switch (emotion)
        {
            case Emotion.Curious:
            {
                if (action == Behavior.Action) PickUp();
                if (action == Behavior.Talking) Ask();
                break;
            }
            case Emotion.Friendly:
            {
                if (action == Behavior.Action) Drop();
                if (action == Behavior.Talking) Compliment();
                break;
            }
            case Emotion.Fearful:
            {
                if (action == Behavior.Action) Run();
                if (action == Behavior.Talking) Yield();
                break;
            }
            case Emotion.Anger:
            {
                if (action == Behavior.Action) Punch();
                if (action == Behavior.Talking) Insult();
                break;
            }
        }
    }
    
    public void PickUp()
    {
        // 1) Cherche l'objet ramassable le plus proche autour de la main (ou du joueur)
        Vector3 origin = _handSocket ? _handSocket.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, _pickupRadius, _pickupMask, QueryTriggerInteraction.Ignore);

        HoldableItem best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var holdable = h.GetComponentInParent<HoldableItem>();
            if (holdable == null) continue;

            // On ne considère pas l'objet déjà tenu
            if (_held != null && holdable == _held) continue;

            // On ignore les objets déjà en main par quelqu'un d'autre
            if (holdable.IsHeld) continue;

            float d = Vector3.SqrMagnitude(h.transform.position - origin);
            if (d < bestDist) { bestDist = d; best = holdable; }
        }

        // Rien à proximité : si on tenait déjà quelque chose, on ne fait rien;
        // sinon on log, comme avant.
        if (best == null)
        {
            if (_held == null)
                Debug.Log("PickUp: aucun objet à portée");
            return;
        }

        // 2) Si on tient déjà un objet différent → on le drop d'abord (swap)
        if (_held != null)
        {
            Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
            _held.Drop(v);
            _held = null;
        }

        // 3) Ramasse le nouvel objet
        best.Pick(_handSocket != null ? _handSocket : transform);
        _held = best;
    }

    public void Drop()
    {
        if (_held == null) return;

        Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
        _held.Drop(v);
        _held = null;
    }



    public void Run()
    {
        Debug.Log("Run");
    }
    
    public void Punch()
    {
        Debug.Log("Punch");
    }
    
    
    public void Ask()
    {
        Debug.Log("Ask");
    }
    
    public void Compliment()
    {
        Debug.Log("Compliment");
    }

    public void Yield()
    {
        Debug.Log("Yield");
    }

    public void Insult()
    {
        Debug.Log("Insult");
    }

    

    
}
