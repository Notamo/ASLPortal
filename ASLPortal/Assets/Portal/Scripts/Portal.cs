using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera userCamera = null;
    public Portal destinationPortal = null;

    public GameObject renderQuad = null;
    public GameObject copyCameraPrefab = null;
    private Camera copyCamera = null;
    public Material copyCamMat = null;

    // Use this for initialization
    void Start()
    {
        Debug.Assert(copyCameraPrefab != null);
        Debug.Assert(renderQuad != null);
    }


    public void Initialize(Portal other, GameObject user)
    {
        destinationPortal = other;
        userCamera = user.GetComponent<PlayerController>().userCamera;

        //set up the copy camera
        if(copyCamera == null)
            copyCamera = Instantiate(copyCameraPrefab, transform).GetComponent<Camera>();

        if (copyCamera.targetTexture != null) copyCamera.targetTexture.Release();
        copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);


        //Set up the material to reference the copy camera's rendertexture
        Material camMat = new Material(copyCamMat);
        camMat.mainTexture = copyCamera.targetTexture;
        other.renderQuad.GetComponent<MeshRenderer>().material = camMat;
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
        Debug.Log("Source: " + this.GetComponent<PhotonView>().viewID.ToString());
        
        //teleportation will only happen if:
        //1. a destination portal exists
        //2. the object is on the front side of the source portal
        //4. the object is moving towards the portal
        if(destinationPortal != null)
        {
            Debug.Log("Destination: " + destinationPortal.GetComponent<PhotonView>().viewID.ToString());

            Matrix4x4 m = transform.worldToLocalMatrix;
            Vector3 objectOffset = m.MultiplyPoint(userCamera.transform.position);

            //Vector3 objectOffset = go.transform.position - transform.position;
            bool playerInFront = Vector3.Dot(transform.forward, objectOffset) < 0.0f;

            //is object moving towards the portal
            Vector3 objVelocity = go.GetComponent<Rigidbody>().velocity;
            bool movingTowards = Vector3.Dot(transform.forward, objVelocity) > 0.0f;

            //is object in front of the portal
            if (playerInFront)
            {
                if (movingTowards)
                {
                    TeleportEnter(go);
                }
                else
                {
                    Debug.Log("Not moving towards portal, ignoring");
                }
            }
            else
            {
                Debug.Log("Player not in front of portal, ignoring");
            }
        }
        else
        {
            Debug.Log("No destination to teleport to, ignoring");
        }
    }


    /*
     * Transforms for teleportation
     * Similar to camera trnsforms -- needs refactoring?
    */
    public void TeleportEnter(GameObject go)
    {
        Matrix4x4 m = transform.worldToLocalMatrix;

        Vector3 objectOffset =           m.MultiplyPoint(go.transform.position);  
        Vector3 relativeObjectFwd =      m.MultiplyVector(go.transform.forward);
        Vector3 relativeObjectUp =       m.MultiplyVector(go.transform.up);
        Vector3 relativeObjectVelocity = m.MultiplyVector(go.GetComponent<Rigidbody>().velocity);

        destinationPortal.TeleportExit(go,
                                       FlipLocalVector(objectOffset),
                                       FlipLocalVector(relativeObjectFwd),
                                       FlipLocalVector(relativeObjectUp),
                                       FlipLocalVector(relativeObjectVelocity));    //I think we actually need to stick with flip local vector for this one

    }

    public void TeleportExit(GameObject go, 
                            Vector3 relativePosition, 
                            Vector3 relativeFwd, 
                            Vector3 relativeUp, 
                            Vector3 relativeVelocity)
    {
        Matrix4x4 m = transform.localToWorldMatrix;
        go.transform.position = m.MultiplyPoint(relativePosition);
        go.transform.rotation = Quaternion.LookRotation(m.MultiplyVector(relativeFwd),
                                              m.MultiplyVector(relativeUp));
        go.GetComponent<Rigidbody>().velocity = m.MultiplyVector(relativeVelocity);
    }

}
