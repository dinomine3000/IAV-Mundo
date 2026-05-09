using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public int cycleTime = 10;
    public int numberOfShades = 5;
    public Vector2 arenaSize = new Vector2(16, 16);
    
    [SerializeField] private GameObject shadeObject;
    [SerializeField] private GameObject lightObject;
    private float time = 0f;
    private bool doCycle = true;
    private bool isDay = false;
    private List<GameObject> activeShades = new List<GameObject>();

    public void SetDoCycle(bool val) { doCycle = val; }

    public void StartCycle(bool cycle)
    {
        time = 0f;
        doCycle = cycle;
        SetDayState(true);
    }

    void Start()
    {
        SetDayState(false);
    }

    void Update()
    {
        if (doCycle)
        {
            time += Time.deltaTime;
            if (time > cycleTime)
            {
                time = 0f;
                SetDayState(!isDay);
            }
        }
    }

    private void SetDayState(bool day)
    {
        isDay = day;
        if (isDay)
            SpawnShades(numberOfShades);
        else
            ClearShades();
        lightObject.SetActive(isDay);
    }

    public bool GetIsDay()
    {
        return isDay;
    }

    private void SpawnShades(int n)
    {
        ClearShades();
        for (int i = 0; i < n; i++)
        {
            float scaleX = Random.Range(1f, 5f);
            float scaleZ = Random.Range(1f, 5f);

            float posX = Random.Range(-(arenaSize.x / 2) + (scaleX / 2), (arenaSize.x / 2) - (scaleX / 2));
            float posZ = Random.Range(-(arenaSize.y / 2) + (scaleZ / 2), (arenaSize.y / 2) - (scaleZ / 2));

            GameObject shade = Instantiate(shadeObject, transform);
            shade.transform.localPosition = new Vector3(posX, 0, posZ);
            shade.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

            activeShades.Add(shade);
        }
    }

    private void ClearShades()
    {
        foreach (GameObject obj in activeShades)
            Destroy(obj);
        activeShades.Clear();
    }
}