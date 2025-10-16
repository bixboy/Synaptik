using System;
using UnityEngine;

public class AngryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.gameObject.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            Debug.Log("Player entered the angry zone!");
            playerInteraction.IsInAngryZone = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.gameObject.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            Debug.Log("Player entered the angry zone!");
            playerInteraction.IsInAngryZone = false;
        }
    }
}
