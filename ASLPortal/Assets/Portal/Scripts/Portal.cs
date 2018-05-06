using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // The type of view this portal uses as a destination
    public enum ViewType
    {
        NONE,
        VIRTUAL,      //displays virtual destination
        PHYSICAL,     //displays a webcam feed of physical destination
        HYBRID        //displays a webcam feed for virtual destination
    };
    ViewType viewType = ViewType.VIRTUAL;
    private Portal destinationPortal = null;    //destination portal ref

    public GameObject renderQuad = null;        //quad child in portal prefab
    public GameObject copyCameraPrefab = null;  //portal cam prefab
    private Camera copyCamera = null;           //portal cam ref
    private Camera userCamera = null;           //user cam ref

    public Material idleMat = null;             //material for idling
    public Material copyCamMat = null;          //material for using a portal cam
    public Material webCamMat = null;           //material for using a webcam
    private WebCamTexture webCamTexture = null; //texure for using a webcam

    #region INITIALIZATION
    // Use this for initialization
    public void Start()
    {
        Debug.Assert(copyCameraPrefab != null);
        Debug.Assert(renderQuad != null);

        Debug.Assert(idleMat != null);
        Debug.Assert(copyCamMat != null);
        Debug.Assert(webCamMat != null);
    }

    /*
     * Initialize this portal to given view type,
     * and try to get user's camera
     */
    public void Initialize(ViewType viewType, GameObject user)
    {
        // Set view type and initialize accordingly 
        this.viewType = viewType;
        switch (viewType)
        {
            case ViewType.VIRTUAL:
                break;
            case ViewType.PHYSICAL:
            case ViewType.HYBRID:
                InitWebCam();       //need to query user for preferred device name
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

    /*
     * Initialize webcam to preferred device
     */
    private void InitWebCam(string preferredWebCam = "")
    {
        // Get all potential webcams, check if any exist
        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length == 0)
        {
            // No webcams, set to virtual portal type
            this.viewType = ViewType.VIRTUAL;
            return;
        }

        // Try finding the preferred webcam
        string selectedWebCam = "";
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
            selectedWebCam = devices[i].name;
            if(devices[i].name == preferredWebCam) 
                break;
        }

        // Create webcam texture using preferred device or default
        if (selectedWebCam != "")
            webCamTexture = new WebCamTexture(selectedWebCam);
        else
            webCamTexture = new WebCamTexture();

        // Start webcam 
        webCamTexture.Play();
    }

    /*
     * Initialize portal camera
     */
    private void InitCopyCam()
    {
        // If no user, try to use main camera
        if (userCamera == null)
            userCamera = Camera.main;

        // Instantiate portal camera using prefab, and save ref to it
        if (copyCamera == null)
            copyCamera = Instantiate(copyCameraPrefab, transform).GetComponent<Camera>();

        // Set target texture to new render texture at current screen size
        if (copyCamera.targetTexture != null)
            copyCamera.targetTexture.Release();
        copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
    }
    #endregion

    #region PORTAL_LINKING
    /*
     * Try linking this portal to a destination portal
     * depending on destination portal's type
     */
    public void LinkDestination(Portal other)
    {
        Debug.Log("Linking to Portal with ViewType: " + other.viewType);

        // Create new material for render quad on successful link
        Material renderMat = null;
        Renderer renderer = renderQuad.GetComponent<Renderer>();

        switch (other.viewType)
        {
            // Virtual link requires portal cam to render to this portal
            case ViewType.VIRTUAL:
                if (copyCamera == null) InitCopyCam();
                renderMat = new Material(copyCamMat) { mainTexture = copyCamera.targetTexture };
                renderer.material = renderMat;
                break;
            // Physical/Hybrid link requires webcam to render to this portal
            case ViewType.PHYSICAL:
            case ViewType.HYBRID:
                renderMat = new Material(webCamMat) { mainTexture = other.webCamTexture };
                renderer.material = renderMat;
                break;
            default:
                Debug.LogError("Error: Cannot Link. Other portal not initialized!");
                return;
        }

        // Set destination portal reference
        destinationPortal = other;
    }

    /*
     * Remove link to destination portal
     */
    public void Close()
    {
        // Unlink from dest portal
        destinationPortal = null;

        // Release render texture reference
        renderQuad.GetComponent<MeshRenderer>().material = idleMat;

        // Destroy portal cam
        Destroy(copyCamera);
    }

    /*
     * Get current destination portal
     */
    public Portal GetDest()
    {
        return destinationPortal;
    }
    #endregion

    #region UPDATE
    /*
     * Update portal cam when linked to destination portal
     * using relative transform of user
     */
    void Update()
    {
        if (destinationPortal != null && copyCamera != null && userCamera != null)
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

    /*
     * Update the copy camera to match the given
     * relative position and orientation
     */
    private void UpdateCopyCamera(Vector3 pos, Quaternion rot)
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
    #endregion

    #region TELEPORTATION
    /*
     * Teleport gameobject if:
     * 1. a destination portal exists
     * 2. It is not pure physical
     * 3. the object is on the front side of the source portal
     * 4. the object is moving towards the portal
     */
    public void TeleportObject(GameObject go)
    {
        Debug.Log("teleportObject! [" + go.name + "]");
        Debug.Log("Source: " + GetComponent<PhotonView>().viewID.ToString());

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
     * Attempt teleportation on collision with portal
     */
    void OnTriggerEnter(Collider other)
    {
        TeleportObject(other.gameObject);
    }

    /*
     * Prepare a gameobject for teleportation to the destination portal
     */
    public void TeleportEnter(GameObject go)
    {
        // Create world to local, flipped around portal transformation
        Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
        Matrix4x4 m = destinationFlipRotation * transform.worldToLocalMatrix;

        // Calculate go transform and velocity in source portal space
        Vector3 posInSourceSpace = m.MultiplyPoint(go.transform.position);
        Quaternion rotInSourceSpace = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)) * go.transform.rotation;
        Vector3 velInSourceSpace = m.MultiplyVector(go.GetComponent<Rigidbody>().velocity);

        // Send go and relative transform/velocity to destination portal
        destinationPortal.TeleportExit(go,
                                       posInSourceSpace,
                                       rotInSourceSpace,
                                       velInSourceSpace);
    }

    /*
     * Transform gameobject into destination portal space
     */
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
    #endregion
}