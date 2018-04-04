using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public MasterController controller = null;
    public PortalManager portalManager = null;
    public WorldManager worldManager = null;
    public Transform defaultPortalXform = null;
    public Portal defaultPortal = null;

    public virtual void Awake()
    {
        controller = GameObject.Find("MasterController").GetComponent<MasterController>();
        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        Debug.Assert(defaultPortalXform != null);
        Debug.Assert(portalManager != null);
        Debug.Assert(worldManager != null);
    }
	// Use this for initialization
	public virtual void Start () {
        //instantiate a portal as well if we are the master client
        if(controller.masterClient &&
            PhotonNetwork.inRoom &&
           defaultPortalXform != null)
        {
            defaultPortal = portalManager.MakePortal(defaultPortalXform.position, defaultPortalXform.forward, defaultPortalXform.up);
            portalManager.RequestRegisterPortal(defaultPortal);
            worldManager.AddToWorld(this, defaultPortal.gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
