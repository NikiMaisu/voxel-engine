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
        transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel"));
        transform.RotateAround(playerBody.position, Vector3.up, Input.GetAxisRaw("Mouse X") * lookSpeed * Time.deltaTime);
        transform.RotateAround(playerBody.position, transform.right, -Input.GetAxisRaw("Mouse Y") * lookSpeed * Time.deltaTime);
        transform.LookAt(playerBody);
    }
}
