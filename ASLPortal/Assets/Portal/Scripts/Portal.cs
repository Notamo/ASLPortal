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
    public Material webCamMat = null;

    //if we are a camera portal
    private WebCamTexture webCamTexture = null;

    // Use this for initialization
    public void Start()
    {
        Debug.Assert(copyCameraPrefab != null);
        Debug.Assert(renderQuad != null);

        Debug.Assert(idleMat != null);
        Debug.Assert(copyCamMat != null);
        Debug.Assert(webCamMat != null);
    }

    public void Initialize(ViewType viewType, GameObject user)
    {
        this.viewType = viewType;

        switch (viewType)
        {
            case ViewType.VIRTUAL:
                break;
            case ViewType.PHYSICAL:
                InitWebCam();
                break;
            case ViewType.HYBRID:
                InitWebCam("USB2.0 Camera");
                break;
            default:
                Debug.LogError("Error: Cannot Initialize portal. Invalid ViewType for initialization!");
                return;
        }

        if (user != null)
            userCamera = user.GetComponent<PlayerAvatar>().userCamera;

        //set up the copy camera
        InitCopyCam();
    }

    private void InitWebCam(string preferredWebCam = "")
    {
        string selectedWebCam = "";
        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length == 0)
        {
            // No webcams, set to virtual portal type
            this.viewType = ViewType.VIRTUAL;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
            selectedWebCam = devices[i].name;
            if(devices[i].name == preferredWebCam) 
                break;
        }

        webCamTexture = new WebCamTexture();
        Debug.Log("WebCamTexture created");

        webCamTexture.Play();

        Debug.Log(webCamTexture.width + "x" + webCamTexture.height);
    }

    private void InitCopyCam()
    {
        if (userCamera == null)
            userCamera = Camera.main;

        if (copyCamera == null)
            copyCamera = Instantiate(copyCameraPrefab, transform).GetComponent<Camera>();

        if (copyCamera.targetTexture != null) copyCamera.targetTexture.Release();
        copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
    }

    public void LinkDestination(Portal other)
    {
        Debug.Log("Linking to Portal with ViewType: " + other.viewType);

        Material renderMat = null;
        Renderer renderer = renderQuad.GetComponent<Renderer>();
        switch (other.viewType)
        {
            case ViewType.VIRTUAL:
                if (copyCamera == null) InitCopyCam();
                renderMat = new Material(copyCamMat);
                renderMat.mainTexture = copyCamera.targetTexture;
                renderer.material = renderMat;
                break;
            case ViewType.PHYSICAL:
                renderMat = new Material(webCamMat);
                renderMat.mainTexture = other.webCamTexture;
                renderer.material = renderMat;
                break;
            case ViewType.HYBRID:
                renderMat = new Material(webCamMat);
                renderMat.mainTexture = other.webCamTexture;
                renderer.material = renderMat;
                break;
            default:
                Debug.LogError("Error: Cannot Link. Other portal not initialized!");
                return;
        }

        destinationPortal = other;
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
        Debug.Log("Source: " + GetComponent<PhotonView>().viewID.ToString());

        //Teleportation will only happen if:
        //1. a destination portal exists
        //2. It is not pure physical
        //3. the object is on the front side of the source portal
        //4. the object is moving towards the portal

        //1. Is there a destination portal?
        if (destinationPortal == null)
        {
            Debug.Log("No destination to teleport to, ignoring");
            return;
        }
        Debug.Log("Destination: " + destinationPortal.GetComponent<PhotonView>().viewID.ToString());

        //2. Is the destination not pure physical?
        if (destinationPortal.viewType == ViewType.PHYSICAL)
        {
            Debug.Log("Destination is physical only, ignoring");
            return;
        }

        //3. Is the object in front of the portal?
        Matrix4x4 m = transform.worldToLocalMatrix;
        Vector3 objectOffset = m.MultiplyPoint(go.transform.position);
        bool playerInFront = objectOffset.z > 0.0f;

        if (!playerInFront)
        {
            Debug.Log("Player not in front of portal, ignoring");
            return;
        }

        //4. Is the object moving towards the portal?
        Vector3 objVelocity = go.GetComponent<Rigidbody>().velocity;
        bool movingTowards = Vector3.Dot(transform.forward, objVelocity) < 0.0f;

        if (!(movingTowards || objVelocity == Vector3.zero))
        {
            Debug.Log("Not moving towards portal, ignoring");
            return;
        }

        //teleport the object
        TeleportEnter(go);
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