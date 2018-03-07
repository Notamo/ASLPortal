using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class PortalManager : MonoBehaviour
{

    private ObjectInteractionManager objManager;

    public bool MasterClient = true;
    public Camera mainCamera = null;
    public GameObject portalSourcePrefab = null;

    //the set of all available portals
    private Dictionary<int, GameObject> portalSet;

    // Use this for initialization
    void Awake()
    {
        Debug.Assert(mainCamera != null);
        Debug.Assert(portalSourcePrefab != null);
        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();

        //Should I use the OnEvent in ObjectManager?
        //PhotonNetwork.OnEventCall += OnEvent;
    }

    // Update is called once per frame
    void Update()
    {
    }


    /*
     * Instantiate and initialize the Portal Prefabs
     */
    public void MakePortal()
    {

    }

    /*
     * Portal System Action Requests
     * (Pre-Master Client Verification)
     */ 
    public void RequestRegisterPortal()
    {

    }

    /*
     * Portal System Actions
     * (Post-Master Client Verification)
     * --Only Called from OnEvent
     */
    private void RegisterPortal()
    {

    }

    private void UnregisterPortal()
    {

    }

    private void LinkPortal(Portal source, Portal destination)
    {

    }

    private void UnlinkPortal(Portal source)
    {

    }

    /*
    //Establish a new Portal as a destination
    public void EstablishNewPortal(Transform location)
    {
        Debug.Log("EstablishNewPortal!");
        GameObject g = objManager.InstantiateOwnedObject("PortalDestination");
        //GameObject g = objManager.Instantiate("PortalDestination", location.position + new Vector3(0, 1, 2), location.localRotation, location.localScale);
        //PortalDestination pd = g.GetComponent<PortalDestination>();
        //objManager.Instantiate(g, )
        //pd.setFollow(mainCamera.transform);

        g.transform.SetParent(location.transform);
        g.transform.localPosition = new Vector3(0, 1, 2);

        //set an ownership whitlist for the new portal (us only);
        UWBNetworkingPackage.NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        int myID = PhotonNetwork.player.ID;
        List<int> IDsToAdd = new List<int>();
        IDsToAdd.Add(myID);
        nm.WhiteListOwnership(g, IDsToAdd);


        //tell the primary to add this portal to its list
        Debug.Log("RAISING EVENTS");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG, 1, true, options);
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG, 2, true, options);
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, 3, true, options);
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK, 4, true, options);

        //ReceiveNewPortal(pd, true);
    }

    //A new portl has been detected, set up the proper resources and logic to recieve the info!
    public void ReceiveNewPortal(PortalDestination destination, bool mine = false)
    {
        int ownerID = destination.GetComponent<PhotonView>().ownerId;
        //rename the the portal to something that makes sense
        if (!mine) destination.name += (":" + ownerID);

        GameObject other;
        GameObject ours;

        if (mine)
        {
            other = Instantiate(portalSourcePrefab, destination.transform); // worldMgr.worlds[1].transform);
            ours = Instantiate(portalSourcePrefab, worldMgr.worlds[PhotonNetwork.player.ID].transform);
        }
        else
        {
            other = Instantiate(portalSourcePrefab, worldMgr.worlds[2].transform);
            ours = Instantiate(portalSourcePrefab, worldMgr.worlds[PhotonNetwork.player.ID].transform);
        }
        other.name = "other";
        ours.name = "ours";

        ours.transform.Translate(new Vector3(0, 1, 2));
        other.transform.Translate(new Vector3(0, 1, 2));

        Portal otherPortal = other.GetComponent<Portal>();
        Portal ourPortal = ours.GetComponent<Portal>();

        ourPortal.worldID = worldMgr.myWorld;
        otherPortal.worldID = 2;

        ourPortal.Initialize(otherPortal, mainCamera.gameObject, LayerMask.NameToLayer("MyWorld"));
        otherPortal.Initialize(ourPortal, mainCamera.gameObject, LayerMask.NameToLayer("OtherWorld"));

        worldMgr.setAppropriateLayer(ours, worldMgr.myWorld);
        worldMgr.setAppropriateLayer(other, 2);
    }

    public Portal MakeSoloPortal(Vector3 position, int worldID)
    {
        GameObject g = objManager.InstantiateOwnedObject("Portal");
        g.transform.parent = worldMgr.worlds[worldID].transform;
        g.transform.Translate(position);

        Portal portal = g.GetComponent<Portal>();

        portal.worldID = worldID;
        worldMgr.setAppropriateLayer(g, worldID);

        return portal;
    }

    public void LinkPortalPair(Portal portal1, Portal portal2)
    {
        portal1.Initialize(portal2, mainCamera.gameObject, LayerMask.NameToLayer("ActiveWorld"));
        portal2.Initialize(portal1, mainCamera.gameObject, LayerMask.NameToLayer("ActiveWorld"));

        portal1.SetOtherLayer();
        portal2.SetOtherLayer();
    }

    //handle events specifically related to portal stuff
    private void OnEvent(byte eventCode, object content, int senderID)
    {
        //handle events specifically related to portal stuff
        switch(eventCode)
        {
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG:
                Debug.Log("EV_REG: " + (int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG:
                Debug.Log("EV_UNREG: " + (int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK:
                Debug.Log("EV_LINK: " + (int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK:
                Debug.Log("EV_UNLINK: " + (int)content);
                break;
        }
    }

    private void RegisterPortal(int viewID)
    {
        PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views)
        {
            if (view.viewID == viewID)
            {
                portalSet.Add(view.viewID, view.gameObject);
                return;
            }
        }

        Debug.LogError("Unable to find portal in PhotonView List!");
    }

    private void UnregisterPortal(int viewID)
    {
        if(!portalSet.Remove(viewID))
        {
            Debug.LogError("Unable to find portal in portalSet! Cannot unregister!");
        }

    }

    private void LinkPortal(int srcViewID, int destViewID)
    {
        
    }

    private void UnlinkPortal(int srcViewID)
    {

    }
    */
}
