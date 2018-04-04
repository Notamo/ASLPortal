using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSelector : MonoBehaviour {

    public GameObject button = null;
    public Portal sourcePortal = null;
    private int sourcePortalID = -1;
    private int destPortalID = -1;
    public Camera playerCam = null;         //for raycasting select
    public PortalManager portalManager = null;
    private Dictionary<int, Portal>.Enumerator portalEnumerator;

    private bool initialized = false;

    void Awake()
    {
        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        Debug.Assert(portalManager != null);
        Debug.Assert(button != null);
    }

	// Use this for initialization
	void Start () {
		
	}

    public void Initialize(Camera playerCam, Portal sourcePortal)
    {
        this.playerCam = playerCam;
        this.sourcePortal = sourcePortal;
        sourcePortalID = this.sourcePortal.GetComponent<PhotonView>().viewID;
        destPortalID = sourcePortalID;
        //ChangeDestination();
    }
	
	// Update is called once per frame
	void Update () {
        if (sourcePortal != null && playerCam != null)
        {
            //left mouse click
            if(Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit, 100f);
                if (hit.collider.gameObject == button)
                {
                    Debug.Log("Ray hit button!");
                    ChangeDestination();
                }
            }
        }
	}

    private void ChangeDestination()
    {
        Debug.Log("Click!");
        Debug.Log("src: " + sourcePortalID);
        Debug.Log("dest: " + destPortalID);

        destPortalID = portalManager.GetNextPortalId(destPortalID);
        portalManager.RequestLinkPortal(sourcePortalID, destPortalID);
          
    }
}
