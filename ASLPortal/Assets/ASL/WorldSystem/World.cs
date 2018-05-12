using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.PortalSystem;

/*
 * The primary World class
 * This class manages all the 
 * Each world contains one portal by default, 
 * so we can travel to it from other worlds
 */ 
public class World : MonoBehaviour {
    public UWBNetworkingPackage.NetworkManager network = null;
    public PortalManager portalManager = null;
    public WorldManager worldManager = null;
    public Transform defaultPortalXform = null;
    public Portal defaultPortal = null;
    public Portal.ViewType defaultPortalViewType = Portal.ViewType.VIRTUAL;

    public virtual void Awake()
    {
        network = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        Debug.Assert(defaultPortalXform != null);
        Debug.Assert(portalManager != null);
        Debug.Assert(worldManager != null);
    }
	// Use this for initialization
	public virtual void Init () {
        //instantiate a portal as well if we are the master client
        if(network.MasterClient &&
            PhotonNetwork.inRoom &&
           defaultPortalXform != null)
        {
            defaultPortal = portalManager.MakePortal(defaultPortalXform.position, defaultPortalXform.forward, defaultPortalXform.up, defaultPortalViewType);
            portalManager.RequestRegisterPortal(defaultPortal);
            worldManager.AddToWorld(this, defaultPortal.gameObject);
        }
        else if(PhotonNetwork.inRoom && defaultPortalXform != null)
        {
            Portal portal = GetComponentInChildren<Portal>();
            if (portal != null)
            {
                defaultPortal = portal;
            }
            else
            {
                Debug.Log("No default portal found for World: " + gameObject.name);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
