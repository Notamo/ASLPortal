﻿using UnityEngine;

public class UserCursor : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private float rotation;

    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = false;
        rotation = 0.0f;
    }

    public void HideCursor(bool hide)
    {
        meshRenderer.enabled = !hide;
    }

    public bool IsHidden()
    {
        return !meshRenderer.enabled;
    }

    // Return the portal this is on, or null
    public GameObject GetPortal()
    {
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            if (hitInfo.collider.gameObject != null)
            {
                if (hitInfo.collider.gameObject.name == "Portal")
                    return hitInfo.collider.gameObject;
                else if (hitInfo.collider.gameObject.name == "ColliderQuad")
                    return hitInfo.collider.transform.parent.gameObject;
            }
        }
        return null;
    }

    // Update is called once per frame
    public void UpdateCursor(Quaternion userRot)
    {
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram...
            // Display the cursor mesh.
            meshRenderer.enabled = true;

            // Move thecursor to the point where the raycast hit.
            this.transform.position = hitInfo.point;

            // Rotate the cursor to hug the surface of the hologram.
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rotation -= 45.0f;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rotation += 45.0f;
        }
        Vector3 f = Quaternion.AngleAxis(rotation, Vector3.up) * -Vector3.forward;
        this.transform.rotation *= Quaternion.FromToRotation(-Vector3.forward, f);
    }
}