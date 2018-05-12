using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.PortalSystem;

/*
 * HubWorld - Extends the base "World" class
 * by addding a portalSelector. This allows us to cycle
 * through other worlds and travel to them
 * Note: Perhaps the name of this class should be changed, as
 *  more than just hubs use this functionality
 */ 
public class HubWorld : World {

    public PortalSelector selector = null;

    public override void Awake()
    {
        base.Awake();
        Debug.Assert(selector != null);
    }

	// Use this for initialization
	public override void Init () {
        base.Init();
        Debug.Log("HubWorld " + name);

        Camera cam = Camera.main;

        Debug.Assert(defaultPortal != null);
        selector.Initialize(cam, defaultPortal);
	}
}
