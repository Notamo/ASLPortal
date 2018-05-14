using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.PortalSystem;

 /// <summary>
 /// The HubWorld class extends the base World class by adding a portal selector
 /// This allows us to cycle through other worlds and travel to them.
 /// </summary>
public class HubWorld : World {

    public PortalSelector selector = null;

    public override void Awake()
    {
        base.Awake();
        Debug.Assert(selector != null);
    }

	/// <summary>
    /// Initialie the HubWorld. First call the base version, then initializes
    /// the portal selector
    /// </summary>
	public override void Init () {
        base.Init();
        Debug.Log("HubWorld " + name);

        Camera cam = Camera.main;

        Debug.Assert(defaultPortal != null);
        selector.Initialize(cam, defaultPortal);
	}
}
