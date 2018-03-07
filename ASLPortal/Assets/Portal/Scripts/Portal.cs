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

    public GameObject[] borders;

    public int worldID = -1;

    // Use this for initialization
    void Start()
    {
        Debug.Assert(destinationPortal != null);
        Debug.Assert(copyCamera != null);
        Debug.Assert(user != null);
        Debug.Assert(renderQuad != null);
        Debug.Assert(teleporter != null);
    }


    public void Initialize(Portal other, GameObject user, int layer)
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

        //set the appropriate layer
        gameObject.layer = layer;
        renderQuad.layer = layer;
        foreach (GameObject g in borders)
            g.layer = layer;


        //set up the teleporter if there is one
        teleporter.enterPortal = this;
        
    }

    public void SetOtherLayer()
    {
        int dest = LayerMask.NameToLayer("ActiveWorld");
        int mask = dest << dest;
        copyCamera.cullingMask = ~mask; 
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
        if (copyCamera != null)
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger enter");
        //other.gameObject.transform.position = destinationPortal.transform.position + destinationPortal.transform.forward * 1.0f;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("trigger exit");
    }

    //Teleport the object to the destination portal
    //(if there is one)
    public void TeleportObject(GameObject go)
    {
        Debug.Log("teleportObject! [" + go.name + "]");
        //return;

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

                Debug.Log(go.transform.position);

                Debug.Log("dest portal layer: " + LayerMask.LayerToName(destinationPortal.gameObject.layer));

                //only change the visible world if we're the ones going through the portal
                if (go.GetComponent<PlayerController>() != null)
                {
                    WorldManager wm = GameObject.Find("WorldManager").GetComponent<WorldManager>();
                    wm.SetVisibleWorld(destinationPortal.worldID);
                }

                Debug.Log("dest portal layer: " + LayerMask.LayerToName(destinationPortal.gameObject.layer));
                go.layer = destinationPortal.gameObject.layer;
            }
        }
    }
}
