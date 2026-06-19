using System;
using System.Collections.Generic;
using UnityEngine;

public class PetAction
{

    public PetAction(List<string> ordersMet, Action<PetHealth, GameObject> action)
    {
        this.ordersMet = ordersMet;
        this.action = action;
    }

    public List<string> ordersMet { get; private set; }
    private Action<PetHealth, GameObject> action;
    
    public void DoAction(PetHealth petHealth, GameObject gameObject){action.Invoke(petHealth, gameObject);}
}