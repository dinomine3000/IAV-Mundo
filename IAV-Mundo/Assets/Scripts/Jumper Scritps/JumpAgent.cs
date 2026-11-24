using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JumpAgent : Agent
{
    [System.Serializable]
    public class Lesson
    {
        public string name;
        [Tooltip("Numero minimo e maximo de buracos")]
        public int minHoles;
        public int maxHoles;
        [Tooltip("Largura do buraco como fracao do alcance maximo do salto")]
        public float minWidthFrac;
        public float maxWidthFrac;
        [Tooltip("Largura de cada plataforma")]
        public float segmentWidth;
        [Tooltip("Desvio lateral maximo entre plataformas consecutivas")] //---
        public float maxLateralShift;

        public Lesson(string name, int minHoles, int maxHoles,
                      float minWidthFrac, float maxWidthFrac,
                      float segmentWidth, float maxLateralShift)
        {
            this.name = name;
            this.minHoles = minHoles;
            this.maxHoles = maxHoles;
            this.minWidthFrac = minWidthFrac;
            this.maxWidthFrac = maxWidthFrac;
            this.segmentWidth = segmentWidth;
            this.maxLateralShift = maxLateralShift;
        }
    }

    public Transform targetTransform;
    public MeshRenderer feedbackRenderer;
    public Material winMaterial;
    public Material loseMaterial;
    public Material platformMaterial;

    [Header("Movimento")]
    public float moveSpeed = 4f;
    public float jumpSpeed = 7f;
    public float gravity = 20f;

    [Header("Recompensa")]
    public float jumpCost = 0.01f;
    public bool useCrossingReward = false; // caso queira recompensar por atravesar um buraco
    public float crossingReward = 0.1f;
    public float fallY = -2f;

    [Header("Curriculo")]
    public int defaultDifficulty = 0;
    public Lesson[] lessons = new Lesson[]
    {
        //          nome               holes     hole width    width.plat   desvio
        new Lesson("L0_plano",         0, 0,    0.00f, 0.00f,      6f,        0f),
        new Lesson("L1_estreito",      1, 1,    0.50f, 0.50f,      6f,        0f),
        new Lesson("L2_largo",         1, 1,    0.85f, 0.95f,      6f,        0f),
        new Lesson("L3_varios",        2, 3,    0.50f, 0.90f,      6f,        0f),
        new Lesson("L4_ilhas",         3, 3,    0.50f, 0.75f,      3f,        1.5f),
    };

    public float courseStartZ = -11f;
    public float flatLength = 14f;
    public float firstSegmentLength = 5f;
    public float minMidSegment = 2.5f;
    public float maxMidSegment = 3.5f;
    public float lastSegmentLength = 4f;
    public float maxCourseX = 1.5f;
    public float segmentThickness = 1f;
    public float targetHeight = 1f;
    public float maxObsDistance = 25f;

    private CharacterController controller;
    private Vector3 velocity;
    private Transform courseRoot;
    private readonly List<float> holeFarEdges = new List<float>();

    private int jumpsThisEpisode;
    private int holesCrossed;
    private bool reachedTarget;
    private bool hasEpisodeData;
    private int currentDifficulty;

    public float JumpRange => 2f * jumpSpeed / gravity * moveSpeed;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        controller.stepOffset = 0.3f;
    }

    public override void OnEpisodeBegin()
    {
        RecordEpisodeStats();

        int d = Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                    .GetWithDefault("difficulty", defaultDifficulty));
        currentDifficulty = Mathf.Clamp(d, 0, lessons.Length - 1);

        Vector3 spawn = BuildCourse(currentDifficulty);

        controller.enabled = false;
        transform.localPosition = spawn;
        transform.localRotation = Quaternion.identity;
        controller.enabled = true;

        velocity = Vector3.zero;
        jumpsThisEpisode = 0;
        holesCrossed = 0;
        reachedTarget = false;
        hasEpisodeData = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 toTarget = targetTransform.localPosition - transform.localPosition;
        sensor.AddObservation(toTarget.normalized);
        sensor.AddObservation(Mathf.Clamp01(toTarget.magnitude / maxObsDistance));
        sensor.AddObservation(controller.isGrounded ? 1f : 0f);
        sensor.AddObservation(Mathf.Clamp(velocity.y / jumpSpeed, -1f, 1f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-1f / MaxStep); // time penalty total approx -1 se nada acontecer

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 horizontal = new Vector3(moveX, 0f, moveZ) * moveSpeed;

        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        // Salto: só recebe penalidade se estiver no chão.
        if (actions.DiscreteActions[0] == 1 && grounded)
        {
            velocity.y = jumpSpeed;
            jumpsThisEpisode++;
            AddReward(-jumpCost);
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move((horizontal + Vector3.up * velocity.y) * Time.deltaTime);

        CheckHoleCrossing();

        if (transform.localPosition.y < fallY)
        {
            AddReward(-1f);
            Finish(false);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        var keyboard = Keyboard.current;

        if (keyboard == null) return;

        float horizontal = 0;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;

        float vertical = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;

        ca[0] = horizontal;
        ca[1] = vertical;
        var da = actionsOut.DiscreteActions;
        da[0] = keyboard.spaceKey.isPressed ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            AddReward(+1f);
            Finish(true);
        }
        else if (other.CompareTag("Wall")) // caso adicionem paredes à cena
        {
            AddReward(-1f);
            Finish(false);
        }
    }

    private void Finish(bool success)
    {
        reachedTarget = success;
        if (feedbackRenderer != null)
            feedbackRenderer.material = success ? winMaterial : loseMaterial;
        EndEpisode();
    }

    private void CheckHoleCrossing()
    {
        if (!controller.isGrounded) return;
        while (holesCrossed < holeFarEdges.Count &&
               transform.localPosition.z > holeFarEdges[holesCrossed] + 0.3f)
        {
            holesCrossed++;
            if (useCrossingReward) AddReward(crossingReward); // uma vez por buraco
        }
    }

    private void RecordEpisodeStats()
    {
        if (!hasEpisodeData) return;
        var stats = Academy.Instance.StatsRecorder;
        stats.Add("Jump/SuccessRate", reachedTarget ? 1f : 0f);
        stats.Add("Jump/JumpsPerEpisode", jumpsThisEpisode);
        stats.Add("Jump/HolesCrossed", holesCrossed);
        if (reachedTarget)
        {
            // Saltos a mais = saltos que não serviram para atravessar um buraco.
            // Se este valor for alto, o agente está a fazer "salto-spam".
            stats.Add("Jump/ExtraJumps", jumpsThisEpisode - holeFarEdges.Count);
        }
    }

    private Vector3 BuildCourse(int difficulty)
    {
        if (courseRoot != null)
        {
            courseRoot.gameObject.SetActive(false);
            Destroy(courseRoot.gameObject);
        }
        holeFarEdges.Clear();

        courseRoot = new GameObject("CourseRoot").transform;
        courseRoot.SetParent(transform.parent, false);

        Lesson L = lessons[difficulty];
        float halfW = L.segmentWidth / 2f;
        float margin = Mathf.Min(1f, halfW * 0.5f);
        int holes = Random.Range(L.minHoles, L.maxHoles + 1);

        float z = courseStartZ;
        float x = 0f;
        Vector3 spawn;

        if (holes == 0)
        {
            // L0: uma plataforma longa, alvo à frente em posição aleatória.
            CreateSegment(x, z, flatLength, L.segmentWidth);
            spawn = new Vector3(Random.Range(-halfW + margin, halfW - margin), 1f,
                                z + Random.Range(0.5f, 2.5f));
            targetTransform.localPosition = new Vector3(
                Random.Range(-halfW + margin, halfW - margin), targetHeight,
                z + Random.Range(6f, flatLength - 1f));
            return spawn;
        }

        CreateSegment(x, z, firstSegmentLength, L.segmentWidth);
        spawn = new Vector3(x + Random.Range(-halfW + margin, halfW - margin), 1f,
                            z + Random.Range(0.5f, firstSegmentLength - 2.5f));
        z += firstSegmentLength;

        for (int i = 0; i < holes; i++)
        {
            float w = Random.Range(L.minWidthFrac, L.maxWidthFrac) * JumpRange;
            z += w;
            holeFarEdges.Add(z);

            bool last = (i == holes - 1);
            float len = last ? lastSegmentLength : Random.Range(minMidSegment, maxMidSegment);

            if (L.maxLateralShift > 0f)
                x = Mathf.Clamp(x + Random.Range(-L.maxLateralShift, L.maxLateralShift),
                                -maxCourseX, maxCourseX);

            CreateSegment(x, z, len, L.segmentWidth);

            if (last)
            {
                targetTransform.localPosition = new Vector3(
                    x + Random.Range(-halfW + margin, halfW - margin), targetHeight,
                    z + Random.Range(1.5f, len - 1f));
            }
            z += len;
        }
        return spawn;
    }

    private void CreateSegment(float centerX, float startZ, float length, float width)
    {
        var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seg.name = "Platform";
        seg.tag = "Floor";
        seg.transform.SetParent(courseRoot, false);
        seg.transform.localPosition = new Vector3(centerX, -segmentThickness / 2f, startZ + length / 2f);
        seg.transform.localScale = new Vector3(width, segmentThickness, length);
        if (platformMaterial != null)
            seg.GetComponent<MeshRenderer>().sharedMaterial = platformMaterial;
    }
}
