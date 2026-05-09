using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ChryonAgent : Agent
{
    public EnvironmentManager envManager;
    public TargetScript targetScript;
    public GameObject target;
    public MeshRenderer floorRenderer;
    public Material defaultMaterial, winMaterial, loseMaterial;

    public float moveSpeed = 4f;
    public float gravity = 20f;
    public float sunPenalty = -1f;
    public float uselessAttackPenalty = 2f;
    public float killReward = 3f;
    public float hitReward = 1f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isInsideShade = false;
    private float health = 20f;
    private int maxHealth = 20;
    public float healthLosePerSecond = 2f;
    public float damage = 7f;
    private float timeSinceAttack = 0f;
    public float secondsPerAttack = 2f;
    public float distToAttack = 1f;
    public Material attackingMat;
    public Material defaultMat;
    public string shadeTagName = "Shade";

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnEpisodeBegin()
    {
        int stage = Mathf.RoundToInt(
            Academy.Instance.EnvironmentParameters
                .GetWithDefault("stage", 2f));
        targetScript.SetMoving(stage > 0);
        if (stage > 1 && envManager != null) envManager.StartCycle(false);
        else envManager.SetDoCycle(false);

        controller.enabled = false;
        transform.localPosition = new Vector3(
            Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, -0.5f));
        controller.enabled = true;

        targetScript.Reset();

        velocity = Vector3.zero;
        isInsideShade = false;
        timeSinceAttack = 0;
        health = maxHealth;
        timeSinceAttack = secondsPerAttack;
        //if (floorRenderer != null) floorRenderer.material = defaultMaterial;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AddReward(-1f / MaxStep);

        sensor.AddObservation(envManager.GetIsDay() ? 1f : 0f);
        sensor.AddObservation(isInsideShade || !envManager.GetIsDay() ? 1f : 0f);

        Vector3 toTarget = (target.transform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toTarget);

        float distToTarget = Vector3.Distance(target.transform.localPosition, transform.localPosition) / 20f;
        sensor.AddObservation(distToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed;

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y -= gravity * Time.deltaTime;
        controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);

        bool day = envManager.GetIsDay();
        if (day && !isInsideShade)
        {
            health -= healthLosePerSecond*Time.deltaTime;
            AddReward(sunPenalty*Time.deltaTime);
        }
        if(health <= 0)
        {
            AddReward(-1);
            if (floorRenderer != null) floorRenderer.material = loseMaterial;
            EndEpisode();
        }
        timeSinceAttack += Time.deltaTime;
        if(timeSinceAttack > secondsPerAttack)
        {
            GetComponent<MeshRenderer>().material = defaultMat;
            //if attacking
            if(actions.DiscreteActions[0] == 1)
            {
                GetComponent<MeshRenderer>().material = attackingMat;
                timeSinceAttack = 0;
                float distToTarget = Vector3.Distance(target.transform.localPosition, transform.localPosition);
                if((!day || isInsideShade) && distToTarget < distToAttack)
                {
                    if (Hurt())
                    {
                        AddReward(killReward);
                        if (floorRenderer != null) floorRenderer.material = winMaterial;
                        EndEpisode();
                    } else
                        AddReward(hitReward);
                } else
                {
                    AddReward(uselessAttackPenalty);
                }
            }   
        }
    }

    private bool Hurt()
    {
        return targetScript.Hurt(damage);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(shadeTagName))
        {
            isInsideShade = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(shadeTagName))
        {
            isInsideShade = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            AddReward(-2f);
            if (floorRenderer != null) floorRenderer.material = loseMaterial;
                EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        var da = actionsOut.DiscreteActions;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null) return;

        float horizontal = 0;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;

        float vertical = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;

        int attack = 0;
        if (mouse.leftButton.isPressed) attack = 1;

        ca[0] = horizontal;
        ca[1] = vertical;
        da[0] = attack;
    }
}
