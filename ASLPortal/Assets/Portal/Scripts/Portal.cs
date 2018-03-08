using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public GameObject user = null;
    public Portal destinationPortal = null;
    public PortalTeleporter teleporter = null;

    public GameObject renderQuad = null;
    public Camera copyCamera = null;
    public Material copyCamMat = null;

    // Use this for initialization
    void Start()
    {
        Debug.Assert(copyCamera != null);
        Debug.Assert(renderQuad != null);
        Debug.Assert(teleporter != null);
    }


    public void Initialize(Portal other, GameObject user)
    {
        destinationPortal = other;
        this.user = user;

        if (copyCamera.targetTexture != null)
        {
            copyCamera.targetTexture.Release();
        }
        copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);

        Material camMat = new Material(copyCamMat);
        camMat.mainTexture = copyCamera.targetTexture;
        other.renderQuad.GetComponent<MeshRenderer>().material = camMat;

        //set up the teleporter if there is one
        teleporter.enterPortal = this;
    }

    // Update is called once per frame
    void Update()
    {
        Matrix4x4 m = transform.worldToLocalMatrix;
        if (user != null)
        {
            Vector3 relativePos = m.MultiplyPoint(user.transform.position);
            destinationPortal.UpdateCamera(-relativePos);
        }
    }

    public void UpdateCamera(Vector3 relativePos)
    {
        if (copyCamera != null && destinationPortal != null)
        {
            copyCamera.transform.localPosition = new Vector3(relativePos.x, -relativePos.y, relativePos.z);
            copyCamera.transform.LookAt(transform);

            float angularDiferenceBetweenPortalRotations =
                Quaternion.Angle(destinationPortal.transform.rotation, transform.rotation);

            Quaternion portalRotationalDifference =
                Quaternion.AngleAxis(180 + angularDiferenceBetweenPortalRotations, Vector3.up);

            Vector3 newCameraDirection = portalRotationalDifference * user.transform.forward;
            copyCamera.transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up);
        }
    }

    //Teleport the object to the destination portal
    //(if there is one)
    public void TeleportObject(GameObject go)
    {
        Debug.Log("teleportObject! [" + go.name + "]");

        if(destinationPortal != null)
        {
            //do the teleporting
            Debug.Log("DestinationPortal != null");

            Vector3 portalToPlayer = go.transform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.forward, portalToPlayer);
            Debug.Log("ptp: " + portalToPlayer);
            Debug.Log("fwd: " + transform.forward);
            Debug.Log("dot: " + dotProduct);

            if (dotProduct < 0f)
            {
                Debug.Log("dotProduct < 0");
                Debug.Log(go.transform.position);
                float rotationDiff = -Quaternion.Angle(transform.rotation, destinationPortal.transform.rotation);
                rotationDiff += 180;
                go.transform.Rotate(Vector3.up, rotationDiff);

                Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
                go.transform.position = destinationPortal.transform.position + positionOffset;
            }
        }
    }
}
