using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class MyWorld : MonoBehaviour {

    public WorldManager worldMgr = null;
    public PortalManager portalMgr = null;
    public Camera mainCamera = null;

    bool portalMade = false;
    bool worldMade = false;
    bool pairMade = false;
    private ObjectInteractionManager objManager;

	// Use this for initialization
	void Awake () {
        Debug.Assert(worldMgr != null);
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

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!worldMade && PhotonNetwork.inRoom)  //maybe we can trigger this instead
            {
                MakeWorld();
                worldMade = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.P) && !portalMade)
        {
            portalMgr.EstablishNewPortal(transform);
            portalMade = true;
        }

        if(Input.GetKeyDown(KeyCode.O) && !pairMade)
        {
            Debug.Log(PhotonNetwork.player.ID);
            Portal portal1 = portalMgr.MakeSoloPortal(new Vector3(-2, 1, 10), PhotonNetwork.player.ID);
            Portal portal2 = portalMgr.MakeSoloPortal(new Vector3(2, 1, 10), PhotonNetwork.player.ID);

            portalMgr.LinkPortalPair(portal1, portal2);
            pairMade = true;
        }

        //
	}

    private void MakeWorld()
    {
        Debug.Log("Making World");
        worldMgr.AddWorld(PhotonNetwork.player.ID, gameObject);

        //make the user avatar/camera
        GameObject user = objManager.InstantiateOwnedObject("UserAvatar");
        user.name = "MyAvatar";
        user.transform.SetParent(transform);
        user.transform.localPosition = new Vector3(0, 1, -2);
        mainCamera.transform.SetParent(user.transform);
        mainCamera.transform.localPosition = .5f * user.transform.up;
        PlayerController pc = user.AddComponent<PlayerController>() as PlayerController;
        
        

        //some reference objects
        GameObject g = objManager.InstantiateOwnedObject("GreenPlane");
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;

        GameObject reference = objManager.InstantiateOwnedObject("Cube");
        reference.transform.SetParent(transform);
        reference.transform.localPosition = new Vector3(0, 0.5f, 0);

        UWBNetworkingPackage.NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        int myID = PhotonNetwork.player.ID;
        List<int> IDsToAdd = new List<int>();
        IDsToAdd.Add(myID);
        nm.WhiteListOwnership(g, IDsToAdd);

        worldMgr.SetVisibleWorld(PhotonNetwork.player.ID);
    }
}
