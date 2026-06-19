using UnityEngine;

public class PetHealth : MonoBehaviour
{
    [Header("Current Stats")]
    [SerializeField] private float sleep;
    [SerializeField] private float hunger;
    [SerializeField] private float fun;

    [Header("Max Limits")]
    public float maxSleep = 10f;
    public float maxHunger = 10f;
    public float maxFun = 10f;

    [Header("Degradation Rates (Per Second)")]
    public float hungerDepletionRate = 1f;
    public float funDepletionRate = 0.5f;
    public float sleepDepletionRate = 2f;

    // Normalized Getters (0.0 to 1.0)
    public float SleepPercentage => maxSleep > 0 ? sleep / maxSleep : 0f;
    public float HungerPercentage => maxHunger > 0 ? hunger / maxHunger : 0f;
    public float FunPercentage => maxFun > 0 ? fun / maxFun : 0f;

    public bool IsDead()
    {
        return sleep <= 0 || hunger <= 0 || fun <= 0;
    }

    public void Reset()
    {
        sleep = maxSleep;
        hunger = maxHunger;
        fun = maxFun;
    }

    public void Tick()
    {
        if (IsDead()) return;

        // Drain needs over time
        hunger = Mathf.Max(0f, hunger - hungerDepletionRate);
        fun = Mathf.Max(0f, fun - funDepletionRate);
        sleep = Mathf.Max(0f, sleep - sleepDepletionRate);
    }

    public void Fun(float amount){fun = Mathf.Clamp(fun + amount, 0, maxFun);}
    public void Sleep(float amount){sleep = Mathf.Clamp(sleep + amount, 0, maxSleep);}
    public void Eat(float amount){hunger = Mathf.Clamp(hunger + amount, 0, maxHunger);}
}