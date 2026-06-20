using System.Collections.Generic;
using UnityEngine;

public class UmaActions : MonoBehaviour
{
    private List<UmaAction> actions = new()
    {
        new(new(){"speed", "power"}, DoSpeed),
        new(new(){"stamina", "guts"}, DoStamina),
        new(new(){"power", "stamina"}, DoPower),
        new(new(){"guts", "speed", "power"}, DoGuts),
        new(new(){"wit", "speed", "rest"}, DoWit),
        new(new(){"rest"}, DoRest),
        new(new(){"infirmary"}, DoInfirmary),
    };

    public UmaAction GetRandomAction()
    {
        return actions[Random.Range(0, actions.Count)];
    }

    public List<string> InvokeAction(int index, UmaHealth petHealth, GameObject agent)
    {
        actions[index].DoAction(petHealth, agent);
        return actions[index].ordersMet;
    }

    public int TotalActionCount() { return actions.Count; }

    private static void DoSpeed(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Speed(10);
        petHealth.Power(5);
    }

    private static void DoStamina(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Stamina(10);
        petHealth.Guts(5);
    }

    private static void DoPower(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Power(10);
        petHealth.Stamina(5);
    }

    private static void DoGuts(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Guts(10);
        petHealth.Speed(5);
        petHealth.Power(5);
    }

    private static void DoWit(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Wit(10);
        petHealth.Speed(5);
    }

    private static void DoRest(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Rest(petHealth.maxEnergy / 2f);
    }

    private static void DoInfirmary(UmaHealth petHealth, GameObject agent)
    {
        petHealth.Infirmary();
    }
}