using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;
using System.Linq;

public class PortalManager : MonoBehaviour
{

    private ObjectInteractionManager objManager;

    public bool MasterClient = true;
    public GameObject player = null;

    //the set of all available portals
    private Dictionary<int, Portal> portalSet;

    // Use this for initialization
    void Awake()
    {
        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
        portalSet = new Dictionary<int, Portal>();
        PhotonNetwork.OnEventCall += OnEvent;
    }

    // Update is called once per frame
    void Update()
    {
    }


    /*
     * Instantiate and initialize the Portal Prefabs
     */
    public Portal MakePortal(Vector3 position, Vector3 forward, Vector3 up, Portal.ViewType vType = Portal.ViewType.VIRTUAL)
    {
        GameObject newPortal = objManager.InstantiateOwnedObject("Portal") as GameObject;
        newPortal.transform.position = position;
        newPortal.transform.rotation = Quaternion.LookRotation(forward, up);

        Portal p = newPortal.GetComponent<Portal>();
        p.Initialize(vType, player);

        return p;// newPortal.GetComponent<Portal>();
    }

    /*
     * Instantiate and initialize the Portal Prefabs
     */
    public Portal MakeCircPortal(Vector3 position, Vector3 forward, Vector3 up, Portal.ViewType vType = Portal.ViewType.VIRTUAL)
    {
        GameObject newPortal = objManager.InstantiateOwnedObject("CircularPortal") as GameObject;
        newPortal.transform.position = position;
        newPortal.transform.rotation = Quaternion.LookRotation(forward, up);

        Portal p = newPortal.GetComponent<Portal>();
        p.Initialize(vType, player);

        return p;// newPortal.GetComponent<Portal>();
    }

    /*
     * Portal System Action Requests
     * (Pre-Master Client Verification)
     */
    #region REQUESTS

    //Request a prefab be registered with the portal management system
    //Done by sending
    public void RequestRegisterPortal(Portal portal)
    {
        Debug.Log("Requesting Portal Registration");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.MasterClient;

        
        //the data we're sending with the message (viewID of the portal)
        int viewID = portal.GetComponent<PhotonView>().viewID;
       
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG, viewID, true, options);
    }

    public void RequestUnregisterPortal(Portal portal)
    {
        Debug.Log("Requesting Portal Registration");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.MasterClient;

        int viewID = portal.GetComponent<PhotonView>().viewID;

        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG, viewID, true, options);
    }

    //Request a destination be set for a portal
    public void RequestLinkPortal(Portal source, Portal destination)
    {
        Debug.Log("Requesting Portal Registration");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.MasterClient;

        int sourceID = source.GetComponent<PhotonView>().viewID;
        int destID = destination.GetComponent<PhotonView>().viewID;

        int[] linkIDPair = new int[2];
        linkIDPair[0] = sourceID;
        linkIDPair[1] = destID;


        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
    }

    public void RequestLinkPortal(int source, int destination)
    {
        if (!IsIDRegistered(source) || !IsIDRegistered(destination))
        {
            Debug.Log("Bad call to ReqLinkPortal, nonregistered portals");
            return;
        }

        Debug.Log("Requesting Portal Link");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.MasterClient;


        int[] linkIDPair = new int[2];
        linkIDPair[0] = source;
        linkIDPair[1] = destination;


        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
    }

    public void RequestUnlinkPortal(Portal source)
    {
        Debug.Log("Requesting Portal Unlink");
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.MasterClient;

        int viewID = source.GetComponent<PhotonView>().viewID;

        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK, viewID, true, options);
    }
    #endregion


    /*
     * Portal System Actions
     * (Post-Master Client Verification)
     * --Only Called from OnEvent
     */

    //Register a portal by adding a Portal to the Dictionary
    private bool RegisterPortal(int portalID)
    {
        if(IsIDRegistered(portalID))
        {
            Debug.LogError("Unable to register portal! Portal already registered");
            return false;
        }

        PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in views)
        {
            if (view.viewID == portalID)
            {
                GameObject g = view.gameObject;
                Portal p = g.GetComponent<Portal>();
                if (p != null)
                {
                    portalSet.Add(view.viewID, p);
                    return true;
                }
                else
                {
                    Debug.LogError("Unable to register portal! Object associated with photonID [" + portalID + "] is not a portal!");
                    return false;
                }
            }
        }

        Debug.LogError("Unable to register portal! Could not find portal in PhotonView List!");
        return false;
    }

    private bool UnregisterPortal(int portalID)
    {
        //ignore for now
        return false;
    }

    //Link two portals together
    //As of now, it's a dual link all the time
    private bool LinkPortal(int sourceID, int destinationID)
    {
        if (IsIDRegistered(sourceID) && IsIDRegistered(destinationID))
        {
            if (portalSet[sourceID].GetDest() != null)
                UnlinkPortal(sourceID);

            //portalSet[sourceID].Initialize(portalSet[destinationID], player);
            portalSet[sourceID].LinkDestination(portalSet[destinationID]);
            return true;
        }
        else
        {
            Debug.Log("Cannot Link Portal! One or more portals is not registered");
            return false;
        }
    }

    private bool UnlinkPortal(int sourceID)
    {
        if (IsIDRegistered(sourceID))
        {
            if (portalSet[sourceID].GetDest() != null)
            {
                portalSet[sourceID].Close();
            }
            return true;
        }
        else
        {
            Debug.Log("Cannot Unlink Portal! Source not registered");
            return false;
        }
    }

    #region EVENT_PROCESSING
    //handle events specifically related to portal stuff
    private void OnEvent(byte eventCode, object content, int senderID)
    {
        //handle events specifically related to portal stuff
        switch (eventCode)
        {
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG:
                Debug.Log("EV_REG: " + (int)content);
                ProcessRegisterPortalEvent((int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG:
                Debug.Log("EV_UNREG: " + (int)content);
                ProcessUnregisterPortalEvent((int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK:
                int[] idPair = (int[])content;
                Debug.Log("EV_LINK: " + idPair);
                ProcessLinkPortalEvent(idPair);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK:
                ProcessUnlinkPortalEvent((int)content);
                Debug.Log("EV_UNLINK: " + (int)content);
                break;
        }
    }

    private void ProcessRegisterPortalEvent(int portalID)
    {
        if(MasterClient) //if we are master client, verify first
        {
            if(RegisterPortal(portalID))
            {
                Debug.Log("Portal Registration Request Approved");
                RaiseEventOptions options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG, portalID, true, options);
            }
            else
            {
                Debug.Log("Portal Registration Request Failed");
            }
        }
        else
        {
            RegisterPortal(portalID);
        }
    }

    private void ProcessUnregisterPortalEvent(int portalID)
    {
        if(MasterClient)
        {
            if(UnregisterPortal(portalID))
            {
                Debug.Log("Portal Unregistration Request Approved");
                RaiseEventOptions options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG, portalID, true, options);
            }
            else
            {
                Debug.Log("Portal Unregistration Request Failed");
            }
        }
        else
        {
            UnregisterPortal(portalID);
        }
    }

    private void ProcessLinkPortalEvent(int[] linkIDPair)
    {
        if(MasterClient)
        {
            if(LinkPortal(linkIDPair[0], linkIDPair[1]))
            {
                Debug.Log("Portal Link Request Approved");
                RaiseEventOptions options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
            }
            else
            {
                Debug.Log("Portal Link Request Failed");
            }
        }
        else
        {
            LinkPortal(linkIDPair[0], linkIDPair[1]);
        }
    }

    private void ProcessUnlinkPortalEvent(int sourceID)
    {
        if (MasterClient)
        {
            if (UnlinkPortal(sourceID))
            {
                Debug.Log("Portal Unlink Request Approved");
                RaiseEventOptions options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, sourceID, true, options);
            }
            else
            {
                Debug.Log("Portal Unlink Request Failed");
            }
        }
    }
    #endregion

    //test if the id corresponds with a portal
    private bool VerifyViewIDPortal(int candID)
    {
        Debug.Log("Verifying View ID [" + candID + "]");
        PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in views)
        {
            if (view.viewID == candID)
            {
                GameObject g = view.gameObject;
                Portal p = g.GetComponent<Portal>();
                if (p != null)
                {
                    Debug.Log("ID [" + candID + "] is valid");
                    return true;
                }
                else
                {
                    Debug.LogError("Unable to verify portal! Object associated with photonID [" + candID + "] is not a portal!");
                    return false;
                }
            }
        }

        Debug.LogError("Unable to verify portal! Could not find ID [" + candID + "] in PhotonView List!");
        return false;
    }

    //Test if the id is registered as a portal
    private bool IsIDRegistered(int id)
    {
        return portalSet.ContainsKey(id);
    }

    public Portal GetPortal(int portalID)
    {
        return portalSet[portalID];
    }

    public Dictionary<int, Portal>.KeyCollection GetPortalIDs(int portalID)
    {
        return portalSet.Keys;
    }

    public int GetNextPortalId(int portalID)
    {
        IEnumerable<int> keys = portalSet.Keys;
        
        if (!portalSet.ContainsKey(portalID) || keys.Last() == portalID)
            return keys.First();

        IEnumerator<int> keyEnumerator = keys.GetEnumerator();
        while (keyEnumerator.Current != portalID)
        {
            keyEnumerator.MoveNext();
        }

        keyEnumerator.MoveNext();
        return keyEnumerator.Current;
    }
}
