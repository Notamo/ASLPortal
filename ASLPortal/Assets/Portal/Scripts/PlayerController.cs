using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;
    public float acceleration = 10.0f;

    private Rigidbody rigidBody = null;

    public Camera userCamera = null;
    public MasterController controller = null;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody>();
        Debug.Assert(rigidBody != null);
	}

    public void SetColor(Color toSet)
    {
        GetComponent<MeshRenderer>().material.color = toSet;
    }
	
	// Update is called once per frame
	void Update () {
        PlayerMovementControls();

        //Create Portal
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(controller != null)
            {
                RaycastHit hit;
                Physics.Raycast(new Ray(userCamera.transform.position, userCamera.transform.forward), out hit);

                if(hit.collider != null && hit.collider.gameObject.name == "GreenPlane")
                {
                    Debug.Log("Plane Hit!");

                    Vector3 portalForward = userCamera.transform.forward;
                    portalForward.y = 0;
                    controller.PlayerCreatePortal(hit.point, portalForward);

                }
            }
            else
            {
                Debug.LogError("No World Set!");
            }
        }

        //Register Portal
        if(Input.GetKeyDown(KeyCode.R))
        {
            if (controller != null)
            {
                RaycastHit hit;
                var mask = 1 << LayerMask.NameToLayer("Portals");
                Physics.Raycast(new Ray(userCamera.transform.position, userCamera.transform.forward), out hit, mask);

                if (hit.collider != null)
                {
                    controller.PlayerRegisterPortal(hit.collider.gameObject);
                }
            }
        }


        
    }

    private void PlayerMovementControls()
    {

        #region WASD
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            //transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity += transform.forward * acceleration;
            
        }        

        if (Input.GetKey(KeyCode.A))
        {
            //transform.localPosition -= transform.right * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.right * movementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //transform.localPosition -= transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.forward * movementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //transform.localPosition += transform.right * Time.deltaTime * movementSpeed;
            moveVelocity += transform.right * movementSpeed;
        }
        rigidBody.velocity = moveVelocity;



        #endregion

        #region CAM_ROTATE
        if (Input.GetMouseButton(1))
        {
            float deltaMouseX = Input.GetAxis("Mouse X");
            float deltaMouseY = Input.GetAxis("Mouse Y");
            
            transform.Rotate(Vector3.up, deltaMouseX * rotateSpeed);
            userCamera.transform.Rotate(Vector3.right, -deltaMouseY * rotateSpeed);
        }
        #endregion
    }
}
