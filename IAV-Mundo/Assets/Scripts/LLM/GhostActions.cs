using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GhostLLM))]
public class GhostActions : MonoBehaviour
{
    [SerializeField] private GameObject ghostOrbPrefab; 
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;

    private GhostLLM agent;
    private TMP_Text agentReplyText;

    private void Awake()
    {
        agent = GetComponent<GhostLLM>();
        var go = GameObject.FindWithTag("AgentAnswer");
        if (go != null) agentReplyText = go.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        agent.RegisterTool("Shoot",    HandleFlicker);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    /*private void HandleGoTo(JObject args)
    {
        if (patroller == null || waypoints == null)
        {
            Debug.LogWarning("GoTo: patroller ou waypoints não atribuídos.");
            return;
        }

        string target = args["target"]?.ToString();
        if (string.IsNullOrWhiteSpace(target))
        {
            Debug.LogWarning("GoTo: argumento 'target' ausente.");
            return;
        }

        if (!waypoints.TryResolve(target, out var t))
        {
            Debug.LogWarning($"GoTo: waypoint desconhecido '{target}'.");
            // Falha graciosa — não rebenta a conversa.
            return;
        }

        patroller.SetGoal(t);
    }

    private void HandleOpenDoor(JObject args)
    {
        if (door == null) { Debug.Log("OpenDoor: porta não atribuída"); return; }
        doorOpen = !doorOpen;
        StopAllCoroutines();
        StartCoroutine(RotateDoor(doorOpen ? openAngle : 0f));
    }*/

    private void HandleFlicker(JObject args)
    {
        // Check if the LLM specified a duration, otherwise default to 2 seconds
        float duration = args["duration"]?.Value<float>() ?? 2.0f;
        Debug.Log($"Ghost Action — Flickering lights for {duration}s");

        // Assuming you have a reference to a light or find one in the room
        Light roomLight = FindAnyObjectByType<Light>(); 
        if (roomLight != null)
        {
            StartCoroutine(FlickerRoutine(roomLight, duration));
        }
    }
    private System.Collections.IEnumerator FlickerRoutine(Light light, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            light.enabled = !light.enabled;
            float delay = Random.Range(0.05f, 0.2f);
            yield return new WaitForSeconds(delay);
            elapsed += delay;
        }
        light.enabled = true; // Ensure light stays on (or off) after flickering
    }
    private void HandleBangDoor(JObject args)
    {
        string doorId = args["doorId"]?.ToString() ?? "ClosestDoor";
        Debug.Log($"Ghost Action — Slamming door: {doorId}");

        // Find the door by name/tag or just grab the closest one to the player
        GameObject door = GameObject.Find(doorId);
        if (door != null)
        {
            // 1. Play a slam audio clip at the door's location
            // AudioSource.PlayClipAtPoint(slamSound, door.transform.position);

            // 2. Simple visual slam using Physics (requires a Rigidbody on the door)
            Rigidbody rb = door.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(door.transform.forward * 500f, ForceMode.Impulse);
            }
        }
    }
    
    private void HandleShowOrbs(JObject args)
    {
        int orbCount = args["count"]?.Value<int>() ?? 3;
        Debug.Log($"Ghost Action — Spawning {orbCount} ghostly orbs");

        // Find the ghost's current favorite room center
        Vector3 spawnPosition = GameObject.FindWithTag("GhostRoom")?.transform.position ?? Vector3.zero;

        for (int i = 0; i < orbCount; i++)
        {
            // Add a bit of random offset so they aren't stacked on top of each other
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(0.5f, 2f), Random.Range(-2f, 2f));
            
            if (ghostOrbPrefab != null)
            {
                GameObject orb = Instantiate(ghostOrbPrefab, spawnPosition + randomOffset, Quaternion.identity);
                
                // Automatically destroy the orb after 10 seconds so they don't clutter the map
                Destroy(orb, 10.0f); 
            }
        }
    }
    private void HandleBleedingWalls(JObject args)
    {
        // The LLM can pass intensity (e.g., "light", "heavy")
        string intensity = args["intensity"]?.ToString() ?? "light";
        Debug.Log($"Ghost Action — Walls begin bleeding. Intensity: {intensity}");

        // Find the blood particle systems or blood decals pre-placed in the room
        GameObject[] bloodEffects = GameObject.FindGameObjectsWithTag("WallBloodEffect");
        
        foreach (GameObject blood in bloodEffects)
        {
            ParticleSystem ps = blood.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                // Adjust emission speed based on LLM input
                main.simulationSpeed = (intensity == "heavy") ? 2.0f : 1.0f; 
                
                ps.Play();
            }
        }
    }
    // ── Animação da porta (igual à aula 10) ─────────────────────────────────

    private System.Collections.IEnumerator RotateDoor(float targetY)
    {
        Quaternion start = door.localRotation;
        Quaternion end = Quaternion.Euler(0, targetY, 0);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.localRotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        door.localRotation = end;
    }
}
