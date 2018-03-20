using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class MyWorld : MonoBehaviour {
    public bool MasterClient = false;

    public PortalManager portalMgr = null;
    public Camera mainCamera = null;

    private GameObject playerAvatar = null;
    public string AvatarName = "MasterAvatar";
    public Vector3 SpawnPos = new Vector3(0, 1, 0);

    private GameObject Plane = null;
    public string PlaneName = "GreenPlane";

    private bool portalMade = false;
    private bool worldMade = false;
    private bool avatarMade = false;
    private bool pairMade = false;
    private ObjectInteractionManager objManager;


    //UI
    public SourceDestPanel linkPanel = null;
    int src = -1;
    int dest = -1;

    // Use this for initialization
    void Awake () {
        Debug.Assert(portalMgr != null);
        Debug.Assert(mainCamera != null);
        Debug.Assert(linkPanel != null);

        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
    }

    private void Start()
    {
        //worldMgr.AddWorld(PhotonNetwork.player.ID, gameObject);
    }

    // Update is called once per frame
    void Update () {

        if(Input.GetKeyDown(KeyCode.M))
        {
            if(!avatarMade && PhotonNetwork.inRoom)
            {
                MakeAvatar();
                avatarMade = true;
            }
        }
        //only the master Client can make the world resources
        if (Input.GetKeyDown(KeyCode.L) && MasterClient)
        {
            if (!worldMade && PhotonNetwork.inRoom)  //maybe we can trigger this instead
            {
                MakeWorld();
                worldMade = true;
            }
        }

        //Link Portal Source
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Testing for portal hit src");
            RaycastHit hit;
            Physics.Raycast(new Ray(mainCamera.transform.position, mainCamera.transform.forward), out hit);

            if (hit.collider != null && hit.collider.gameObject.name == "Portal")
            {
                src = hit.collider.gameObject.GetComponent<PhotonView>().viewID;
            }
            linkPanel.setSourceID(src);
        }

        //Link Portal Dest
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Testing for portal hit dest");
            RaycastHit hit;
            Physics.Raycast(new Ray(mainCamera.transform.position, mainCamera.transform.forward), out hit);

            if (hit.collider != null && hit.collider.gameObject.name == "Portal")
            {
                dest = hit.collider.gameObject.GetComponent<PhotonView>().viewID;
            }
            linkPanel.setDestID(dest);
        }

        //Link Portal
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("u pressed");
            if (src != -1 && dest != -1) {
                Debug.Log("req link portal");
                portalMgr.RequestLinkPortal(src, dest);
            }
        }

    }

    private void MakeWorld()
    {

        //some reference objects
        Plane = objManager.InstantiateOwnedObject(PlaneName);
        //plane.transform.SetParent(transform);
        Plane.transform.localPosition = Vector3.zero;

        GameObject reference = objManager.InstantiateOwnedObject("Cube");
        //reference.transform.SetParent(transform);
        reference.transform.localPosition = new Vector3(0, 0.5f, 0);

        UWBNetworkingPackage.NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        int myID = PhotonNetwork.player.ID;
        List<int> IDsToAdd = new List<int>();
        IDsToAdd.Add(myID);
        nm.WhiteListOwnership(Plane, IDsToAdd);
    }

    private void MakeAvatar()
    {
        //make the user avatar/camera
        playerAvatar = objManager.InstantiateOwnedObject("UserAvatar") as GameObject;
        playerAvatar.name = AvatarName;
        

        playerAvatar.transform.localPosition = SpawnPos;
        mainCamera.transform.SetParent(playerAvatar.transform);
        mainCamera.transform.localPosition = .5f * playerAvatar.transform.up;

        //add the player controller after so other players can't manipulate this
        PlayerController pc = playerAvatar.AddComponent<PlayerController>() as PlayerController;
        pc.userCamera = mainCamera;
        pc.myWorld = this;

        portalMgr.player = playerAvatar;
    }

    //PlayerCreatePortal
    //Try to create a portal where the player camera is looking at on the plane
    public void PlayerCreatePortal(Vector3 position, Vector3 forward)
    {
        portalMgr.MakePortal(position, forward);
    }

    //PlayerRegisterPortal
    //Try to register the portal with the PortalManager
    public void PlayerRegisterPortal(GameObject portalGO)
    {
        Portal portal = portalGO.GetComponent<Portal>();
        if(portal != null)
            portalMgr.RequestRegisterPortal(portal);
        else
        {
            Debug.LogError("Object is not a portal! cannot register!");
        }
    }
}
