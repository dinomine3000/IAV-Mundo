using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ChryonAgent : Agent
{
    [Header("Config")]
    public EnvironmentManager envManager;
    public TargetScript targetScript;
    public GameObject target;
    public MeshRenderer floorRenderer;
    public Material defaultMaterial, winMaterial, loseMaterial;
    public Material attackingMat;
    public Material defaultMat;
    public string shadeTagName = "Shade";
    private CharacterController controller;
    public float gravity = 20f;
    [Header("Rewards")]
    public float sunPenalty = -1f;
    public float deathPenalty = -1f;
    public float hitReward = 1f;
    public float uselessAttackPenalty = -2f;
    public float killReward = 3f;
    public float shadeReward = 1f;
    public float timePenalty = -2f;

    [Header("Stats")]
    public float moveSpeed = 4f;
    public float secondsPerAttack = 2f;
    public float damage = 7f;
    public int maxHealth = 20;
    public float healthLosePerSecond = 2f;
    public float distToAttack = 1f;

    private Vector3 velocity;
    private bool isInsideShade = false;
    private float health = 20f;
    private float timeSinceAttack = 0f;    
    private Transform nearestShade = null;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnEpisodeBegin()
    {
        int stage = Mathf.RoundToInt(
            Academy.Instance.EnvironmentParameters
                .GetWithDefault("stage", 3f));
        Vector3 agentStartPos = new Vector3(
            Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, -0.5f));;
        targetScript.SetMoving(stage > 0);
        if (stage > 1 && envManager != null)
        {
            envManager.ResetSun(stage > 2, true);
            nearestShade = envManager.GetNearestShade(agentStartPos);
        } 
        else envManager.SetDoCycle(false);
        if(nearestShade != null)
        {
            agentStartPos = nearestShade.localPosition;
        }

        controller.enabled = false;
        transform.localPosition = agentStartPos;
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
        bool day = envManager.GetIsDay();
        sensor.AddObservation(day ? 1f : 0f); //1
        sensor.AddObservation(isInsideShade || !day ? 1f : 0f); //1

        Vector3 toTarget = (target.transform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toTarget); //3

        float distToTarget = Vector3.Distance(target.transform.localPosition, transform.localPosition) / 20f;
        sensor.AddObservation(distToTarget); //1

        if(nearestShade == null || !day)
        {
            sensor.AddObservation(new Vector3());
            sensor.AddObservation(0);
        } else
        {
            Vector3 toShade = (nearestShade.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(toShade); //3

            float distToShade = Vector3.Distance(nearestShade.localPosition, transform.localPosition) / 20f;
            sensor.AddObservation(distToShade); //1            
        }

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
        //add time penalty during the night
        if(!day) AddReward(timePenalty/MaxStep);
        if (day && !isInsideShade)
        {
            float selfDamage = healthLosePerSecond*Time.deltaTime;;
            health -= selfDamage;
            AddReward(sunPenalty*selfDamage);

            if (nearestShade != null)
            {
                float distToShade = Vector3.Distance(transform.localPosition, nearestShade.localPosition);
                AddReward(-2*(1/20f) * distToShade * Time.deltaTime); // scales with how far you wandered
            }
        }
        if(health <= 0)
        {
            AddReward(deathPenalty);
            if (floorRenderer != null) floorRenderer.material = loseMaterial;
            EndEpisode();
        }
        /*if (day && isInsideShade)
        {
            float distToTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);
            float proximityBonus = distToAttack / (distToTarget + 1);
            AddReward(0.3f * proximityBonus * Time.deltaTime);
        }*/

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
