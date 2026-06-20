using UnityEngine;

public class UmaHealth : MonoBehaviour
{
    [Header("Current Stats")]
    [SerializeField] private float health;
    [SerializeField] private float energy;
    [SerializeField] private float speed;
    [SerializeField] private float stamina;
    [SerializeField] private float power;
    [SerializeField] private float guts;
    [SerializeField] private float wit;

    [Header("Max Limits")]
    public float maxEnergy = 120f;
    public float maxHealth = 10f;

    [Header("Degradation Rates (Per Second)")]
    public float healthDepletionRate = 1f;
    public float healthLowEnergyDepletionRate = 1f;
    public float energyDepletionRate = 35f;

    // Normalized Getters (0.0 to 1.0)
    public float EnergyPercentage => maxEnergy > 0 ? energy / maxEnergy : 0f;
    public float HealthPercentage => maxHealth > 0 ? health / maxHealth : 0f;

    public bool IsDead()
    {
        return health <= 0;
    }

    public void Reset()
    {
        energy = maxEnergy;
        health = maxHealth;
        wit = 0;
        guts = 0;
        power = 0;
        stamina = 0;
        speed = 0;
    }

    public void Tick()
    {
        if (IsDead()) return;

        energy -= energyDepletionRate;
        if(energy <= 0)
            health -= healthLowEnergyDepletionRate;
        else
            health -= healthDepletionRate;
    }

    public void Speed(float amount){speed = Mathf.Max(0, speed + amount);}
    public void Stamina(float amount){stamina = Mathf.Max(0, stamina + amount);}
    public void Power(float amount){power = Mathf.Max(0, power + amount);}
    public void Guts(float amount){guts = Mathf.Max(0, guts + amount);}
    public void Wit(float amount){wit = Mathf.Max(0, wit + amount);}
    public void Rest(float amount){energy = Mathf.Clamp(energy + amount, 0, maxEnergy);}
    public void Infirmary(){health = maxHealth;}
}