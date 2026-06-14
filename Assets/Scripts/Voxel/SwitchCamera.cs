using UnityEngine;
using UnityEngine.InputSystem;


public class SwitchCamera : MonoBehaviour
{
    public GameObject FirstPersonCamera;
    public GameObject ThirdPersonCamera;
    public int Manager;

    void ManageCamera()
    {
        if (Manager == 0)
        {
            Fpc();
            Manager = 1;
        }
        else
        {
            Tpc();
            Manager = 0;
        }
    }
    void Fpc()
    {
        ThirdPersonCamera.tag = "Untagged";
        FirstPersonCamera.tag = "MainCamera";
        FirstPersonCamera.SetActive(true);
        ThirdPersonCamera.SetActive(false);
    }

    void Tpc()
    {
        FirstPersonCamera.tag = "Untagged";
        ThirdPersonCamera.tag = "MainCamera";
        FirstPersonCamera.SetActive(false);
        ThirdPersonCamera.SetActive(true);
    }

    void Start()
    {
        FirstPersonCamera = GameObject.Find("FirstPersonCamera");
        ThirdPersonCamera = GameObject.Find("ThirdPersonCamera");
        FirstPersonCamera.tag = "MainCamera";
    }
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            ManageCamera();
        }
    }
}
