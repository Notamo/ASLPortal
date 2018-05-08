using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSelector : MonoBehaviour {

    public GameObject button = null;                //button child on selector
    private PortalManager portalManager = null;     //for linking/unlinking portal
    private Portal sourcePortal = null;             //portal to control
    private Camera playerCam = null;                //for raycasting select

    private int sourcePortalID = -1;
    private int destPortalID = -1;
    private bool initialized = false;

    // Use for instantiation
    private void Awake()
    {
        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        Debug.Assert(portalManager != null);
        Debug.Assert(button != null);
    }

    // Update is called once per frame
    void Update()
    {
        //need portal to control and cam for click raycast
        if (sourcePortal != null && playerCam != null)
        {
            //make sure position is on left side of portal, facing same direction
            transform.position = sourcePortal.transform.position + 1.5f * sourcePortal.transform.right;
            transform.forward = sourcePortal.transform.forward;

            //left mouse click
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit, 100f);

                //change destination on button click
                if (hit.collider != null && hit.collider.gameObject == button)
                {
                    ChangeDestination();
                }
            }
        }
    }

    /*
     * Initialize required properties for portal control
     */
    public void Initialize(Camera playerCam, Portal sourcePortal)
    {
        this.playerCam = playerCam;
        this.sourcePortal = sourcePortal;
        sourcePortalID = this.sourcePortal.GetComponent<PhotonView>().viewID;
        destPortalID = sourcePortalID;
    }

    /*
     * Link the controlled portal to the next available portal
     */
    private void ChangeDestination()
    {
        destPortalID = portalManager.GetNextPortalId(destPortalID);
        portalManager.RequestLinkPortal(sourcePortalID, destPortalID);
    }
}
