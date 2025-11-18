using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private List<MultiTouchEventTrigger> list = new();

    void Update()
    {
        foreach (var i in list)
            i.OrderedUpdate();
    }
}

interface IOrdered
{
    public void OrderedUpdate();
}
