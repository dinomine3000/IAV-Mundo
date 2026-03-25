using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 input = Vector3.zero;
        if(Keyboard.current.aKey.isPressed) input += Vector3.left;
        if(Keyboard.current.dKey.isPressed) input += Vector3.right;
        if(Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if(Keyboard.current.sKey.isPressed) input += Vector3.back;
        if(Mouse.current.scroll.up.IsPressed()) input += Vector3.up;
        if(Mouse.current.scroll.down.IsPressed()) input += Vector3.down;
        transform.Translate(input * Time.deltaTime * speed);
    }
}
