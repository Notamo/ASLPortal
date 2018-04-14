using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    //The type of view a portal provides in any connected
    //portal's renderQuad
    public enum ViewType
    {
        NONE,
        VIRTUAL,      //displays the unity scene
        PHYSICAL,     //displays a webcam feed
        HYBRID        //mix of the two
    };

    ViewType viewType = ViewType.VIRTUAL;


    public Camera userCamera = null;
    public Portal destinationPortal = null;

    public GameObject renderQuad = null;
    public GameObject copyCameraPrefab = null;
    private Camera copyCamera = null;

    public Material idleMat = null;     //material for idling
    public Material copyCamMat = null;      //material for using a copyCam
    public Material webcamMat = null;

    //if we are a camera portal
    private WebCamTexture webCamTexture = null;

    // Use this for initialization
    void Start()
    {
        Debug.Assert(copyCameraPrefab != null);
        Debug.Assert(renderQuad != null);

        Debug.Assert(idleMat != null);
        Debug.Assert(copyCamMat != null);
        Debug.Assert(webcamMat != null);
    }

    public void Initialize(ViewType viewType, GameObject user)
    {
        this.viewType = viewType;
        switch(viewType)
        {
            case ViewType.VIRTUAL:
                break;
            case ViewType.PHYSICAL:
                //webCamTexture = new WebCamTexture();
                //webCamTexture.Play();
                break;
            case ViewType.HYBRID:
                Debug.LogError("Error: Cannot Initialize portal. Hybrid view type not yet implemented!");
                return;
            default:
                Debug.LogError("Error: Cannot Initialize portal. Invalid ViewType for initialization!");
                return;
        }
        
        userCamera = user.GetComponent<PlayerController>().userCamera;

        //set up the copy camera
        if (copyCamera == null)
            copyCamera = Instantiate(copyCameraPrefab, transform).GetComponent<Camera>();

        if (copyCamera.targetTexture != null) copyCamera.targetTexture.Release();
        copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
    }

    public void LinkDestination(Portal other)
    {
        destinationPortal = other;

        Material renderMat = null;
        switch (other.viewType)
        {
            case ViewType.VIRTUAL:
                //Set up the material to reference the copy camera's rendertexture
                renderMat = new Material(copyCamMat);
                renderMat.mainTexture = copyCamera.targetTexture;
                renderQuad.GetComponent<MeshRenderer>().material = renderMat;
                break;
            case ViewType.PHYSICAL:
                Debug.Log("portal link physical view");
                /*Debug.Assert(other.webCamTexture != null);
                //set up the material to reference the other portals video feed
                renderMat = new Material(webcamMat);
                Renderer renderer = renderQuad.GetComponent<Renderer>();
                //renderer.material = renderMat;
                renderer.material.mainTexture = other.webCamTexture;
                
                other.webCamTexture.Play();*/

                WebCamDevice[] devices = WebCamTexture.devices;
                for (int i = 0; i < devices.Length; i++)
                {
                    Debug.Log(devices[i].name);
                }

                Debug.Assert(devices.Length >= 1);

                int activeCam = 0;

                WebCamTexture wct = new WebCamTexture("" + devices[activeCam].name);
                renderQuad.GetComponent<MeshRenderer>().material.mainTexture = wct;

                Debug.Log("Active Camera: " + devices[activeCam] + ", \"" + devices[activeCam].name);
                //Debug.Assert(wct != null);
                //Renderer renderer = renderQuad.GetComponent<Renderer>();
                //Debug.Assert(renderer != null);
                //renderer.material.mainTexture = wct;
                wct.Play();

                break;
            case ViewType.HYBRID:
                Debug.LogError("Error: Cannot Link. Hybrid view type not yet implemented!");
                return;
            default:
                Debug.LogError("Error: Cannot Link. Other portal not initialized!");
                return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //sent the user cam info to the destination portal if there is one
        //relative positions and orientations to the this portal
        if (destinationPortal != null && userCamera != null)
        {
            // Calculate matrix for world to local, reflected across portal
            Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
            Matrix4x4 worldToLocal = destinationFlipRotation * transform.worldToLocalMatrix;

            // Calculate portal cams pos and rot in this portal space
            Vector3 camPosInSourceSpace = worldToLocal.MultiplyPoint(userCamera.transform.position);
            Quaternion camRotInSourceSpace = Quaternion.LookRotation(worldToLocal.GetColumn(2), worldToLocal.GetColumn(1)) * userCamera.transform.rotation;

            // Set portal cam relative to destination portal
            UpdateCopyCamera(camPosInSourceSpace, camRotInSourceSpace);
        }
    }

    //flip a vector across the portal (ignores local y-axis)
    private Vector3 FlipLocalVector(Vector3 vec) { return new Vector3(-vec.x, vec.y, -vec.z); }

    //Update the copy camera to match the given
    //relative position and orientations
    public void UpdateCopyCamera(Vector3 pos, Quaternion rot)
    {
        if (copyCamera != null && destinationPortal != null)
        {
            // Transform position and rotation to this portal's space
            copyCamera.transform.position = destinationPortal.transform.TransformPoint(pos);
            copyCamera.transform.rotation = destinationPortal.transform.rotation * rot;

            // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
            Vector4 clipPlaneWorldSpace = new Vector4(destinationPortal.transform.forward.x, destinationPortal.transform.forward.y, destinationPortal.transform.forward.z, -Vector3.Dot(destinationPortal.transform.position, destinationPortal.transform.forward));
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(copyCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            // Update projection based on new clip plane
            copyCamera.projectionMatrix = userCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
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
            Vector3 objectOffset = m.MultiplyPoint(go.transform.position);

            //Vector3 objectOffset = go.transform.position - transform.position;
            bool playerInFront = objectOffset.z > 0.0f;

            //is object moving towards the portal
            Vector3 objVelocity = go.GetComponent<Rigidbody>().velocity;
            bool movingTowards = Vector3.Dot(transform.forward, objVelocity) < 0.0f;

            //is object in front of the portal
            if (playerInFront)
            {
                if (movingTowards || objVelocity == Vector3.zero)
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
        Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
        Matrix4x4 m = destinationFlipRotation * transform.worldToLocalMatrix;

        Vector3 posInSourceSpace = m.MultiplyPoint(go.transform.position);
        Quaternion rotInSourceSpace = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)) * go.transform.rotation;
        Vector3 velInSourceSpace = m.MultiplyVector(go.GetComponent<Rigidbody>().velocity);

        destinationPortal.TeleportExit(go,
                                       posInSourceSpace,
                                       rotInSourceSpace,
                                       velInSourceSpace);

    }

    public void TeleportExit(GameObject go,
                            Vector3 relativePosition,
                            Quaternion relativeRotation,
                            Vector3 relativeVelocity)
    {
        Matrix4x4 m = transform.localToWorldMatrix;
        go.transform.position = m.MultiplyPoint(relativePosition);
        go.transform.rotation = transform.rotation * relativeRotation;
        go.GetComponent<Rigidbody>().velocity = m.MultiplyVector(relativeVelocity);
    }

    public void Close()
    {
        // Unlink from dest portal
        destinationPortal = null;

        // Release render texture reference
        renderQuad.GetComponent<MeshRenderer>().material = idleMat;
    }
}
