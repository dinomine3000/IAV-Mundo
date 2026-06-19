using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestFile: MonoBehaviour
{
    private bool flag = false;
    void Update()
    {

        if (Keyboard.current.fKey.isPressed)
        {
            if(!flag)
                gameObject.GetComponent<PetAgent>().ReactTo(new Dictionary<string, float>{
                    ["flip"] = 1f
                });
            flag = true;
        } else if (Keyboard.current.eKey.isPressed)
        {
            if(!flag)
                gameObject.GetComponent<PetAgent>().ReactTo(new Dictionary<string, float>{
                    ["eat"] = 1f
                });
            flag = true;
            
        } else if (Keyboard.current.sKey.isPressed)
        {
            if(!flag)
                gameObject.GetComponent<PetAgent>().ReactTo(new Dictionary<string, float>{
                    ["sleep"] = 1f
                });
            flag = true;
            
        } else if (Keyboard.current.iKey.isPressed)
        {
            if(!flag)
                gameObject.GetComponent<PetAgent>().ReactTo(new Dictionary<string, float>{
                    ["sit"] = 1f
                });
            flag = true;

        } else 
            flag = false;
    }
}