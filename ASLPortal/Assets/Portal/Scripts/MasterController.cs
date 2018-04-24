using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class MasterController : MonoBehaviour
{
    //are we the master client?
    public bool masterClient = false;

    //core components
    public ObjectInteractionManager objManager = null;
    public PortalManager portalManager = null;
    public WorldManager worldManager = null;

    //the main camera (for raycasting, and attaching to the player)
    public Camera mainCamera = null;

    //Player
    public GameObject playerAvatar = null;
    public string AvatarName = "MasterAvatar";
    public Color AvatarColor = Color.white;
    public Vector3 SpawnPosition = new Vector3(0, 1, 0);
    public GameObject mCursorPrefab = null;

    //Worlds
    public List<string> worldPrefabs;

    //UI
    public SourceDestPanel linkPanel = null;

    //Setup State
    private bool setupComplete = false;

    // Use this for initialization
    void Start()
    {
        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
        PhotonNetwork.OnEventCall += OnEvent;

        Debug.Assert(objManager != null);
        Debug.Assert(worldManager != null);
        Debug.Assert(portalManager != null);
    }

    // Update is called once per frame
    void Update()
    {
        //Do Setup
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!setupComplete && PhotonNetwork.inRoom)
            {
                MakeAvatar();

                if (masterClient)
                {
                    CreateDefaultWorlds();
                    Debug.Log("Sending Master load event");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_MASTER_LOAD, 1, true, options);
                }
                else
                {
                    worldManager.FindWorlds();
                }

                World world = worldManager.getWorldByName("HubWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }

                setupComplete = true;
            }
            else if (playerAvatar == null && PhotonNetwork.inRoom)
            {
                MakeAvatar();
                World world = worldManager.getWorldByName("HubWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }
            }
        }
    }

    //Create the Worlds that will exist from the outset
    private void CreateDefaultWorlds()
    {
        foreach (string worldPrefab in worldPrefabs)
        {
            worldManager.CreateWorld(worldPrefab);
        }
        worldManager.InitializeAll();
    }

    //Make Avatar
    private void MakeAvatar()
    {
        //make the user avatar/camera
        playerAvatar = objManager.InstantiateOwnedObject("UserAvatar") as GameObject;
        playerAvatar.name = AvatarName;

        playerAvatar.transform.localPosition = SpawnPosition;
        mainCamera.transform.SetParent(playerAvatar.transform);
        mainCamera.transform.localPosition = .5f * playerAvatar.transform.up;

        //add the player controller after so other players can't manipulate it
        PlayerController pc = playerAvatar.AddComponent<PlayerController>() as PlayerController;
        pc.userCamera = mainCamera;
        pc.controller = this;
        pc.SetColor(AvatarColor);
        pc.SetCursor(mCursorPrefab);

        portalManager.player = playerAvatar;
    }


    //PlayerCreatePortal
    //Try to create a portal where the player camera is looking at on the plane
    public void PlayerCreatePortal(Vector3 position, Vector3 forward, Vector3 up, Portal.ViewType vType = Portal.ViewType.VIRTUAL)
    {
        portalManager.MakePortal(position, forward, up, vType);
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

    #region EVENT_PROCESSING
    //handle events specifically related to portal stuff
    private void OnEvent(byte eventCode, object content, int senderID)
    {
        //handle events specifically related to portal stuff
        switch (eventCode)
        {
            case UWBNetworkingPackage.ASLEventCode.EV_MASTER_LOAD:
                Debug.Log("EV_REG: " + (int)content);
                ProcessMasterLoad();
                break;
        }
    }

    private void ProcessMasterLoad()
    {
        if (!masterClient)
        {
            worldManager.FindWorlds();

            if (playerAvatar != null)
            {
                World world = worldManager.getWorldByName("HubWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }
            }

            setupComplete = true;
        }
    }
    #endregion
}