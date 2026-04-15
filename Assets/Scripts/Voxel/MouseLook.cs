using UnityEngine;

public class MouseLook : MonoBehaviour
{
    Vector2 mouseDirection = Vector2.zero;
    public float lookSpeed = 1200;

    private Transform playerBody;
    
    void Start()
    {
        playerBody = this.transform.parent.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) {Cursor.lockState = CursorLockMode.None;}
        
        Vector2 mouseChange = new Vector2(Input.GetAxisRaw("Mouse X") * lookSpeed * Time.deltaTime, Input.GetAxisRaw("Mouse Y") * lookSpeed * Time.deltaTime);

        mouseDirection += mouseChange;
        mouseDirection.y = Mathf.Clamp(mouseDirection.y, -90f, 90f);
        
        this.transform.localRotation = Quaternion.AngleAxis(-mouseDirection.y, Vector3.right);

        playerBody.localRotation = Quaternion.AngleAxis(mouseDirection.x, Vector3.up);

    }
}
