using TMPro;
using Unity.VisualScripting;
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
    public TextMeshPro speed;
    public TextMeshPro stamina;
    public TextMeshPro power;
    public TextMeshPro guts;
    public TextMeshPro wit;

    [Header("Parameters")]
    public Slider energySlider;
    public Slider healthSlider;
    private Image healthFillArea;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthFillArea = healthSlider.fillRect.GetComponent<Image>();

        statsCont.SetActive(true);
        performanceCont.SetActive(false);
        performanceBtn.interactable = false;

        statsBtn.onClick.AddListener(() => SwapMenu(true));
        performanceBtn.onClick.AddListener(() => SwapMenu(false));
    }

    // Método para atualizar a UI com os dados do PetAgent
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

    // Método chamado no final da sessão para mostrar o menu de performance
    public void ShowPerformance()
    {
        performanceBtn.interactable = true;
        SwapMenu(false);
    }

    // Método que alterna entre os containers
    public void SwapMenu(bool showStats)
    {
        statsCont.SetActive(showStats);
        performanceCont.SetActive(!showStats);
    }

    // Método que ajusta a cor da barra de health de acordo com a percentagem do slider
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
}
