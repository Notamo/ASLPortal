using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;

    public Camera playerCamera = null;
    public MyWorld myWorld = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        PlayerMovementControls();

        //Create a portal
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(myWorld == null)
            {
                Debug.LogError("No MyWorld Set!");
            }
            else
            {
                RaycastHit hit;
                Physics.Raycast(new Ray(playerCamera.transform.position, playerCamera.transform.forward), out hit);

                if(hit.collider != null && hit.collider.gameObject.name == "GreenPlane")
                {
                    Debug.Log("Plane Hit!");
                    if(myWorld != null)
                    {
                        Vector3 portalForward = playerCamera.transform.forward;
                        portalForward.y = 0;
                        myWorld.PlayerCreatePortal(hit.point, portalForward);
                    }
                }
            }
        }
        
    }

    private void PlayerMovementControls()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.localPosition -= transform.right * Time.deltaTime * movementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.localPosition -= transform.forward * Time.deltaTime * movementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.localPosition += transform.right * Time.deltaTime * movementSpeed;
        }

        if (Input.GetMouseButton(1))
        {
            float deltaMouseX = Input.GetAxis("Mouse X");
            float deltaMouseY = Input.GetAxis("Mouse Y");
            
            transform.Rotate(Vector3.up, deltaMouseX * rotateSpeed);
            playerCamera.transform.Rotate(Vector3.right, -deltaMouseY * rotateSpeed);
        }
    }
}
