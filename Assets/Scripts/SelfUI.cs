using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelfUI : MonoBehaviour
{
    public Joystick joystick;
    
    public static SelfUI instance;

	public EventTrigger eventTriggerJumpButon;
	public EventTrigger eventTriggerRunButon;
	public EventTrigger eventTriggerFullScrean;
    

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }
}
