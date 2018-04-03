using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class MasterController : MonoBehaviour {

    
    public bool masterClient = false;

    public PortalManager portalManager = null;
    public Camera mainCamera = null;

    public GameObject playerAvatar = null;
    public string AvatarName = "MasterAvatar";
    public Vector3 SpawnPos = new Vector3(0, 1, 0);
    public int SpawnWorldIdx = 0;

    public WorldManager worldManager = null;
    public List<string> worldPrefabs;

    private ObjectInteractionManager objManager;

    private bool setupComplete = false;
    //UI
    public SourceDestPanel linkPanel = null;
    int src = -1;
    int dest = -1;

    // Use this for initialization
    void Start () {
        Debug.Assert(worldManager != null);
        Debug.Assert(portalManager != null);
        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
    }
	
	// Update is called once per frame
	void Update () {

        //Do Setup
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!setupComplete && PhotonNetwork.inRoom)
            {
                if (masterClient){
                    worldManager.CreateDefaultWorlds();
                }

                MakeAvatar();
                setupComplete = true;
            }
        }


        //Link Portal Source
        if (Input.GetKeyDown(KeyCode.T))
        {
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
            if (src != -1 && dest != -1)
            {
                portalManager.RequestLinkPortal(src, dest);
            }
        }
    }

    //Make Avatar
    private void MakeAvatar()
    {
        //make the user avatar/camera
        playerAvatar = objManager.InstantiateOwnedObject("UserAvatar") as GameObject;
        playerAvatar.name = AvatarName;

        World world = GameObject.Find("RedCubeWorld").GetComponent<World>();
        worldManager.AddToWorld(world, playerAvatar);

        playerAvatar.transform.localPosition = SpawnPos;
        mainCamera.transform.SetParent(playerAvatar.transform);
        mainCamera.transform.localPosition = .5f * playerAvatar.transform.up;

        //add the player controller after so other players can't manipulate it
        PlayerController pc = playerAvatar.AddComponent<PlayerController>() as PlayerController;
        pc.userCamera = mainCamera;
        pc.controller = this;

        portalManager.player = playerAvatar;
    }


    //PlayerCreatePortal
    //Try to create a portal where the player camera is looking at on the plane
    public void PlayerCreatePortal(Vector3 position, Vector3 forward)
    {
        portalManager.MakePortal(position, forward, Vector3.up);
    }

    //PlayerRegisterPortal
    //Try to register the portal with the PortalManager
    public void PlayerRegisterPortal(GameObject portalGO)
    {
        Portal portal = portalGO.GetComponent<Portal>();
        if (portal != null)
            portalManager.RequestRegisterPortal(portal);
        else
        {
            Debug.LogError("Object [" + portalGO.name + "] is not a portal! cannot register!");
        }
    }
}
