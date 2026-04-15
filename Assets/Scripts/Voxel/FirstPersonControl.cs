using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonControl : MonoBehaviour
{

    public float forwardSpeed = 11f;
    
    void Start()
    {
    }

    void Update()
    {
        if (Keyboard.current.wKey.isPressed) {
            transform.transform.Translate(new Vector3(0, 0, (forwardSpeed * Time.deltaTime)));
        }
        if (Keyboard.current.sKey.isPressed) {
            transform.transform.Translate(new Vector3(0, 0, (forwardSpeed * Time.deltaTime) * -1f));
        }
        if (Keyboard.current.aKey.isPressed) {
            transform.transform.Translate(new Vector3((forwardSpeed * Time.deltaTime) * -1f, 0, 0));
        }
        if (Keyboard.current.dKey.isPressed) {
            transform.transform.Translate(new Vector3((forwardSpeed * Time.deltaTime), 0, 0));
        }
        if (Keyboard.current.spaceKey.isPressed) {
            transform.transform.Translate(new Vector3(0, (forwardSpeed * Time.deltaTime), 0));
        }
        if (Keyboard.current.shiftKey.isPressed) {
            transform.transform.Translate(new Vector3(0, (forwardSpeed * Time.deltaTime) * -1, 0));
        }
        
        
    }
}
