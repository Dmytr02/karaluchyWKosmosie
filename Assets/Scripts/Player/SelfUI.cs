using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelfUI : MonoBehaviour
{
    public Joystick joystick;
    
    public static SelfUI instance;

	public MultiTouchEventTrigger eventTriggerJumpButon;
	public MultiTouchEventTrigger eventTriggerRunButon;
	public MultiTouchEventTrigger eventTriggerFullScrean;
    

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }
}
