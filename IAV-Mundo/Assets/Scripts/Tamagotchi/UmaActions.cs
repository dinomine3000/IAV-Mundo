using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UmaActions : MonoBehaviour
{
    public GameManager gameManager;
    private UmaLLM llmAgent;
    private UmaAgent rlAgent;
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

    private void Awake()
    {
        llmAgent = GetComponent<UmaLLM>();
        rlAgent = GetComponent<UmaAgent>();
    }

    private void Start()
    {
        llmAgent.RegisterTool("doAction", DoAction);
    }

    private void DoAction(JObject args)
    {
        Dictionary<string, float> orders = args.ToObject<Dictionary<string, float>>();
        rlAgent.SetActiveOrders(orders);
        rlAgent.RequestDecision();
    }

    public UmaAction GetRandomAction()
    {
        return actions[Random.Range(0, actions.Count)];
    }

    public List<string> InvokeAction(int index, UmaHealth petHealth, GameObject agent)
    {
        actions[index].DoAction(petHealth, agent);
        gameManager.ChangeImage(index);
        return actions[index].ordersMet;
    }

    public int TotalActionCount() { return actions.Count; }

    private static void DoSpeed(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoSpeed: Speed +10, Power +5");
        petHealth.Speed(10);
        petHealth.Power(5);
    }

    private static void DoStamina(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoStamina: Stamina +10, Guts +5");
        petHealth.Stamina(10);
        petHealth.Guts(5);
    }

    private static void DoPower(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoPower: Power +10, Stamina +5");
        petHealth.Power(10);
        petHealth.Stamina(5);
    }

    private static void DoGuts(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoGuts: Guts +10, Speed +5, Power +5");
        petHealth.Guts(10);
        petHealth.Speed(5);
        petHealth.Power(5);
    }

    private static void DoWit(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoWit: Wit +10, Speed +5");
        petHealth.Wit(10);
        petHealth.Speed(5);
    }

    private static void DoRest(UmaHealth petHealth, GameObject agent)
    {
        float restAmount = petHealth.maxEnergy / 2f;
        Debug.Log($"[{agent.name}] Executing DoRest: Recovering {restAmount} Energy");
        petHealth.Rest(restAmount);
    }

    private static void DoInfirmary(UmaHealth petHealth, GameObject agent)
    {
        Debug.Log($"[{agent.name}] Executing DoInfirmary: Healing conditions");
        petHealth.Infirmary();
    }
}