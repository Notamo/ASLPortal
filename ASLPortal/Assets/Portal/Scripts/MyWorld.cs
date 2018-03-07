using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class MyWorld : MonoBehaviour {
    public bool MasterClient = false;

    public PortalManager portalMgr = null;
    public Camera mainCamera = null;

    private GameObject Avatar = null;
    public string AvatarName = "MasterAvatar";
    public Vector3 SpawnPos = new Vector3(0, 1, 0);

    public string PlaneName = "GreenPlane";

    private bool portalMade = false;
    private bool worldMade = false;
    private bool avatarMade = false;
    private bool pairMade = false;
    private ObjectInteractionManager objManager;

	// Use this for initialization
	void Awake () {
        Debug.Assert(portalMgr != null);
        Debug.Assert(mainCamera != null);

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

       /* if (Input.GetKeyDown(KeyCode.P) && !portalMade)
        {
            portalMgr.EstablishNewPortal(transform);
            portalMade = true;
        }*/

        /*if(Input.GetKeyDown(KeyCode.O) && !pairMade)
        {
            Debug.Log(PhotonNetwork.player.ID);
            Portal portal1 = portalMgr.MakeSoloPortal(new Vector3(-2, 1, 10), PhotonNetwork.player.ID);
            Portal portal2 = portalMgr.MakeSoloPortal(new Vector3(2, 1, 10), PhotonNetwork.player.ID);

            portalMgr.LinkPortalPair(portal1, portal2);
            pairMade = true;
        }*/

        //
	}

    private void MakeWorld()
    {

        //some reference objects
        GameObject plane = objManager.InstantiateOwnedObject(PlaneName);
        //plane.transform.SetParent(transform);
        plane.transform.localPosition = Vector3.zero;

        GameObject reference = objManager.InstantiateOwnedObject("Cube");
        //reference.transform.SetParent(transform);
        reference.transform.localPosition = new Vector3(0, 0.5f, 0);

        UWBNetworkingPackage.NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        int myID = PhotonNetwork.player.ID;
        List<int> IDsToAdd = new List<int>();
        IDsToAdd.Add(myID);
        nm.WhiteListOwnership(plane, IDsToAdd);
    }

    private void MakeAvatar()
    {
        //make the user avatar/camera
        Avatar = objManager.InstantiateOwnedObject("UserAvatar") as GameObject;
        Avatar.name = AvatarName;
        Avatar.transform.localPosition = SpawnPos;
        mainCamera.transform.SetParent(Avatar.transform);
        mainCamera.transform.localPosition = .5f * Avatar.transform.up;

        //add the player controller after so other players can't manipulate this
        PlayerController pc = Avatar.AddComponent<PlayerController>() as PlayerController;
    }
}
