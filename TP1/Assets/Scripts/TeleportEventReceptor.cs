using Oculus.Interaction.Locomotion;
using System;
using UnityEngine;

public class TeleportEventReceptor : MonoBehaviour, ILocomotionEventHandler
{
    public event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled;

    public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
    {
        if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute)
        {
            // Debug.Log("We received request to teleport");
            GetComponent<OVRScreenFade>().FadeIn();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
