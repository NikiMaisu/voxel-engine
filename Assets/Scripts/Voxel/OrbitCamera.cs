using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    Vector2 mouseDirection = Vector2.zero;
    public float lookSpeed = 600;
    private float orbitRadius = 25f;
    private Transform playerBody;
    
    void Start()
    {
        playerBody = this.transform.parent.transform;
        Cursor.lockState = CursorLockMode.Locked;
        transform.Translate(Vector3.forward * orbitRadius);
    }

    void Update()
    {
        //zoom stuff
        transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel"));

        transform.RotateAround(playerBody.transform.position, new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")), lookSpeed * Time.deltaTime);
        
        // if (Input.GetKey(KeyCode.Escape)) {Cursor.lockState = CursorLockMode.None;}
        //
        // Vector2 mouseChange = new Vector2(Input.GetAxisRaw("Mouse X") * lookSpeed * Time.deltaTime, Input.GetAxisRaw("Mouse Y") * lookSpeed * Time.deltaTime);
        //
        // mouseDirection += mouseChange;
        // mouseDirection.y = Mathf.Clamp(mouseDirection.y, -90f, 90f);
        //
        // this.transform.localRotation = Quaternion.AngleAxis(-mouseDirection.y, Vector3.right);
        //
        // playerBody.localRotation = Quaternion.AngleAxis(mouseDirection.x, Vector3.up);
        //
        transform.LookAt(playerBody);

    }
}
