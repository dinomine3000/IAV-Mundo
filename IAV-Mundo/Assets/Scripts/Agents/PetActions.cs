using System.Collections.Generic;
using UnityEngine;

public class PetActions: MonoBehaviour
{
    private List<PetAction> actions = new()
    {
        new(new(){"trick", "flip"}, DoFlip),
        new(new(){"eat"}, DoEat),
        new(new(){"sleep"}, DoSleep),
        new(new(){"trick", "sit"}, DoSit),
    };

    public PetAction GetRandomAction()
    {
        return actions[Random.Range(0, actions.Count)];
    }
    public List<string> InvokeAction(int index, PetHealth petHealth, GameObject agent)
    {
        actions[index].DoAction(petHealth, agent);
        return actions[index].ordersMet;
    }

    public int TotalActionCount(){return actions.Count;}

    private static void DoFlip(PetHealth petHealth, GameObject agent)
    {
        petHealth.Fun(petHealth.maxFun);
    }
    private static void DoSleep(PetHealth petHealth, GameObject agent)
    {
        petHealth.Sleep(petHealth.maxSleep);
    }
    private static void DoEat(PetHealth petHealth, GameObject agent)
    {
        petHealth.Eat(petHealth.maxHunger);
    }
    private static void DoSit(PetHealth petHealth, GameObject agent)
    {
        petHealth.Sleep(petHealth.maxSleep/2f);
    }
}