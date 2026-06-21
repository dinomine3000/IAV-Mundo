using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsMenu : MonoBehaviour
{
    [Header("Menus")]
    public Button statsBtn;
    public Button performanceBtn;
    public GameObject statsCont;
    public GameObject performanceCont;

    [Header("Stats")]
    public TextMeshProUGUI speed;
    public TextMeshProUGUI stamina;
    public TextMeshProUGUI power;
    public TextMeshProUGUI guts;
    public TextMeshProUGUI wit;

    [Header("Parameters")]
    public Slider energySlider;
    public Slider healthSlider;
    private Image healthFillArea;

    [Header("Score")]
    public TextMeshProUGUI score;
    public Button restartBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AtStart();
    }

    // Metodo para atualizar a UI com os dados do PetAgent
    public void UpdateStats(float max, float spd, float sta, float pwr, float gts, float wt, float nrg, float hlth)
    {
        string max_string = max.ToString("F0");
        speed.text = spd.ToString("F0") + "/" + max_string;
        stamina.text = sta.ToString("F0") + "/" + max_string;
        power.text = pwr.ToString("F0") + "/" + max_string;
        guts.text = gts.ToString("F0") + "/" + max_string;
        wit.text = wt.ToString("F0") + "/" + max_string;

        energySlider.value = nrg;
        healthSlider.value = hlth;

        HealthDisplay();
    }
    public void UpdateStats(UmaHealth umaHealth)
    {
        string max_string = 120.ToString("F0");
        speed.text = umaHealth.SpeedStat.ToString("F0") + "/" + max_string;
        stamina.text = umaHealth.StaminaStat.ToString("F0") + "/" + max_string;
        power.text = umaHealth.PowerStat.ToString("F0") + "/" + max_string;
        guts.text = umaHealth.GutsStat.ToString("F0") + "/" + max_string;
        wit.text = umaHealth.WitStat.ToString("F0") + "/" + max_string;

        energySlider.value = umaHealth.EnergyPercentage;
        healthSlider.value = umaHealth.HealthPercentage;

        HealthDisplay();
    }

    // Metodo chamado no final da sess�o para mostrar o menu de performance
    public void ShowPerformance(UmaHealth umaHealth)
    {
        SwapMenu(false);

        restartBtn.gameObject.SetActive(true);
        float finalScore = CalculateFinalScore(umaHealth);
        score.text = finalScore.ToString("F0");
    }

    // Metodo que alterna entre os containers
    public void SwapMenu(bool showStats)
    {
        statsCont.SetActive(showStats);
        performanceCont.SetActive(!showStats);
    }

    // Metodo que ajusta a cor da barra de health de acordo com a percentagem do slider
    private void HealthDisplay()
    {
        if (healthFillArea == null) return;

        float percentage = healthSlider.value / healthSlider.maxValue;

        if (percentage <= 0.25f)
        {
            healthFillArea.color = Color.red;
        }
        else if (percentage <= 0.50f)
        {
            healthFillArea.color = new Color(1f, 0.5f, 0f); // Laranja
        }
        else if (percentage <= 0.75f)
        {
            healthFillArea.color = Color.yellow;
        }
        else
        {
            healthFillArea.color = Color.green;
        }
    }

    public float CalculateFinalScore(UmaHealth petHealth)
    {
        if (petHealth == null) return 0f;

        float totalStats = petHealth.SpeedStat
                         + petHealth.StaminaStat
                         + petHealth.PowerStat
                         + petHealth.GutsStat
                         + petHealth.WitStat;

        float damageFactor = 1f - petHealth.HealthPercentage;

        float penalty = damageFactor * 50;

        float finalScore = totalStats - penalty;

        return Mathf.Max(0f, finalScore);
    }

    public void AtStart()
    {
        statsCont.SetActive(true);
        performanceCont.SetActive(false);
        restartBtn.gameObject.SetActive(false);
    }
}
