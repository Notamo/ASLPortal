using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera userCamera = null;
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
        userCamera = user.GetComponent<PlayerController>().userCamera;

        if (copyCamera.targetTexture != null) copyCamera.targetTexture.Release();
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
        //sent the user cam info to the destination portal if there is one
        //relative positions and orientations to the this portal
        if (destinationPortal != null && userCamera != null)
        {
            Matrix4x4 m = transform.worldToLocalMatrix;
            Vector3 playerOffset = m.MultiplyPoint(userCamera.transform.position);
            Vector3 relativePlayerFwd = m.MultiplyVector(userCamera.transform.forward);
            Vector3 relativePlayerUp = m.MultiplyVector(userCamera.transform.up);

            destinationPortal.UpdateCopyCamera(FlipLocalVector(playerOffset), 
                                               FlipLocalVector(relativePlayerFwd), 
                                               FlipLocalVector(relativePlayerUp));
        }
    }

    //flip a vector across the portal (ignores local y-axis)
    private Vector3 FlipLocalVector(Vector3 vec) { return new Vector3(-vec.x, vec.y, -vec.z); }

    //Update the copy camera to match the given
    //relative position and orientations
    public void UpdateCopyCamera(Vector3 cameraOffset, Vector3 relativeCameraFwd, Vector3 relativeCameraUp)
    {
        if (copyCamera != null && destinationPortal != null)
        {
            Matrix4x4 m = transform.localToWorldMatrix;

            copyCamera.transform.position = m.MultiplyPoint(cameraOffset);
            copyCamera.transform.rotation = 
                Quaternion.LookRotation(m.MultiplyVector(relativeCameraFwd),
                                        m.MultiplyVector(relativeCameraUp));
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
