using System;
using UnityEngine;

public class SelfUI : MonoBehaviour
{
    public DynamicJoystick joystick;
    
    public static SelfUI instance;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }
}
