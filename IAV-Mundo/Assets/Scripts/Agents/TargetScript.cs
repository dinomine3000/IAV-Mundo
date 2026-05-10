using UnityEngine;

public class TargetScript : MonoBehaviour
{
    private bool moving = false;
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;
    public float arrivalDistance = 0.5f;
    public Vector2 arenaSize = new Vector2(16, 16);

    private CharacterController controller;
    private Vector3 targetPosition;
    private float verticalVelocity;
    public int maxHealth = 20;
    private float health = 20;

    public float GetHealth(){return health;}
    public bool Hurt(float dmg)
    {
        health -= dmg;
        if(health <= 0)
        {
            return true;
        }
        return false;
    }

    public void Reset()
    {
        health = maxHealth;
        CharacterController targetController = GetComponent<CharacterController>();
        targetController.enabled = false;
        transform.localPosition = new Vector3(
            Random.Range(-4f, 4f), 0.5f, Random.Range(0.5f, 5f));
        targetController.enabled = true;
        PickNewTarget();
    }
    public void SetMoving(bool val){moving = val;}
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        PickNewTarget();
    }

    void Update()
    {
        if(!moving) return;
        //chck dist
        float distanceToTarget = Vector3.Distance(new Vector3(transform.localPosition.x, 0, transform.localPosition.z), 
                                                 new Vector3(targetPosition.x, 0, targetPosition.z));

        if (distanceToTarget < arrivalDistance)
        {
            PickNewTarget();
        }

        //rotate
        Vector3 direction = (targetPosition - transform.localPosition).normalized;
        direction.y = 0; // Keep rotation horizontal
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        //gravity
        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity -= 9.81f * Time.deltaTime;

        //move
        Vector3 velocity = transform.forward * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void PickNewTarget()
    {
        float rx = Random.Range(-(arenaSize.x / 2), arenaSize.x / 2);
        float rz = Random.Range(-(arenaSize.y / 2), arenaSize.y / 2);
        targetPosition = new Vector3(rx, transform.position.y, rz);
    }
}