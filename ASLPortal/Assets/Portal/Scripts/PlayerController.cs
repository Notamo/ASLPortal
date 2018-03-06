using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;

    public Camera playerCamera = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.W))
        {
            transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
        }

        if(Input.GetKey(KeyCode.A))
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
            transform.Rotate(transform.up, deltaMouseX * rotateSpeed);
        }
    }

    void SetWorld(int worldID)
    {

    }
}
