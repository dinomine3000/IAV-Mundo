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
    private List<GameObject> activeShades = new();

    public void SetDoCycle(bool val) { doCycle = val; }

    public void ResetSun(bool doCycle, bool isDay)
    {
        foreach(GameObject go in activeShades)
            Destroy(go);
        activeShades.Clear();
        SetDoCycle(doCycle);
        SetDayState(isDay);
    }

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

    public Transform GetNearestShade(Vector3 pos)
    {
        return activeShades[0].transform;
    }

    private void SpawnShades(int n)
    {
        ActivateShades();
        if(activeShades.Count >= n) return;
        for (int i = 0; i < n - activeShades.Count; i++)
        {
            float scaleX = Random.Range(3f, 6f);
            float scaleZ = Random.Range(3f, 6f);

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
            obj.SetActive(false);
    }
    private void ActivateShades()
    {
        foreach (GameObject obj in activeShades)
            obj.SetActive(true);
    }
}