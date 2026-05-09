using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ChryonAgent : Agent
{
    public EnvironmentManager envManager;
    public Transform targetTransform;
    public MeshRenderer floorRenderer;
    public Material defaultMaterial, winMaterial, loseMaterial;

    public float moveSpeed = 4f;
    public float gravity = 20f;
    public float sunPenalty = -0.01f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isInsideShade = false;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnEpisodeBegin()
    {
        if (envManager != null) envManager.StartCycle();

        controller.enabled = false;
        transform.localPosition = new Vector3(
            Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, -1.5f));
        controller.enabled = true;

        velocity = Vector3.zero;
        isInsideShade = false;
        if (floorRenderer != null) floorRenderer.material = defaultMaterial;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AddReward(-1f / MaxStep);

        sensor.AddObservation(envManager.GetIsDay() ? 1f : 0f);
        sensor.AddObservation(isInsideShade ? 1f : 0f);

        Vector3 toTarget = (targetTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toTarget);

        float distToTarget = Vector3.Distance(targetTransform.localPosition, transform.localPosition) / 20f;
        sensor.AddObservation(distToTarget);

        sensor.AddObservation(controller.velocity.x / moveSpeed);
        sensor.AddObservation(controller.velocity.z / moveSpeed);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed;

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y -= gravity * Time.deltaTime;
        controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);

        if (envManager.GetIsDay())
        {
            if (!isInsideShade)
            {
                AddReward(sunPenalty);
            }
            else
            {
                AddReward(0.001f);
            }
        }

        isInsideShade = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shade"))
        {
            isInsideShade = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(2.0f);
            if (floorRenderer != null) floorRenderer.material = winMaterial;
            EndEpisode();
        }
        else if (other.CompareTag("Wall"))
        {
            SetReward(-1.0f);
            if (floorRenderer != null) floorRenderer.material = loseMaterial;
            EndEpisode();
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
    }
}
