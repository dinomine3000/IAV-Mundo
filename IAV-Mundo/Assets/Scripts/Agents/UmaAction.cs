using System;
using System.Collections.Generic;
using UnityEngine;

public class UmaAction
{

    public UmaAction(List<string> ordersMet, Action<UmaHealth, GameObject> action)
    {
        this.ordersMet = ordersMet;
        this.action = action;
    }

    public List<string> ordersMet { get; private set; }
    private Action<UmaHealth, GameObject> action;
    
    public void DoAction(UmaHealth petHealth, GameObject gameObject){action.Invoke(petHealth, gameObject);}
}