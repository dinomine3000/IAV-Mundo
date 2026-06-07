using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 2f;

    public float mouseSensitivity = 100f;
    public Camera playerCamera;
    public float interactionDistance = 5f;
    public LayerMask uiLayer;

    [Header("UI Interaction Settings")]
    [Tooltip("Time in seconds required between button clicks.")]
    public float clickCooldown = 0.5f;
    private float nextAllowableClickTime = 0f;

    private float xRotation = 0f;
    private bool locked = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Rotation();
        
        // Input System '.isPressed' registers every frame. Added 'wasPressedThisFrame' 
        // to prevent unintentional spamming while holding the click down.
        if (Mouse.current.leftButton.wasPressedThisFrame) ExecuteCrosshairRaycast();
        
        if (Keyboard.current.escapeKey.isPressed || Keyboard.current.enterKey.isPressed) locked = false;
    }

    void Movement()
    {
        if (locked) return;
        Vector3 input = Vector3.zero;
        if (Keyboard.current.aKey.isPressed) input += Vector3.left;
        if (Keyboard.current.dKey.isPressed) input += Vector3.right;
        if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) input += Vector3.back;
        transform.Translate(input * Time.deltaTime * speed);
    }

    void Rotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.03f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.03f;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void ExecuteCrosshairRaycast()
    {
        // Ray from the center of the screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, uiLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // 1. Check for a Button component
            Button button = hitObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                // Check if the global game time has surpassed the cooldown threshold
                if (Time.time >= nextAllowableClickTime)
                {
                    button.onClick.Invoke();
                    
                    // Set the timestamp for when the player can next click
                    nextAllowableClickTime = Time.time + clickCooldown;
                }
                return;
            }

            // 2. Check for an Input Field component
            TMP_InputField inputField = hitObject.GetComponent<TMP_InputField>();
            if (inputField != null && inputField.interactable)
            {
                inputField.ActivateInputField();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                locked = true;
                return;
            }
        }
    }
}