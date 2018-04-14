using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;
    public float acceleration = 10.0f;

    private Rigidbody rigidBody = null;

    public Camera userCamera = null;
    public MasterController controller = null;

    private UserCursor mCursor = null;
    int src = -1;
    int dest = -1;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        Debug.Assert(rigidBody != null);
    }

    public void SetColor(Color toSet)
    {
        GetComponent<MeshRenderer>().material.color = toSet;
    }

    public void SetCursor(GameObject cursorPrefab)
    {
        mCursor = Instantiate(cursorPrefab, transform).GetComponent<UserCursor>();
    }

    // Update is called once per frame
    void Update () {
        PlayerMovementControls();

        PlayerPortalControls();
    }
    private void PlayerPortalControls()
    {
        // Toggle cursor and controls
        if (Input.GetKeyDown(KeyCode.P))
        {
            mCursor.HideCursor(!mCursor.IsHidden());
        }
        //Create/Register/Link Portals
        if (!mCursor.IsHidden())
        {
            mCursor.UpdateCursor(transform.rotation);

            if (controller != null)
            {
                //Create Portal
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 pos = mCursor.transform.position;
                    controller.PlayerCreatePortal(pos, -mCursor.transform.forward, mCursor.transform.up);
                }
                //Register Portal
                if (Input.GetKeyDown(KeyCode.R))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        controller.PlayerRegisterPortal(portalObj);
                    }
                }
                //Link Portal Source
                if (Input.GetKeyDown(KeyCode.T))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        src = portalObj.GetComponent<PhotonView>().viewID;
                    }
                    controller.linkPanel.setSourceID(src);
                }
                //Link Portal Destination
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        dest = portalObj.GetComponent<PhotonView>().viewID;
                    }
                    controller.linkPanel.setDestID(dest);
                }
                //Link Portal
                if (Input.GetKeyDown(KeyCode.U))
                {
                    if (src != -1 && dest != -1)
                    {
                        controller.portalManager.RequestLinkPortal(src, dest);
                        src = -1;
                        dest = -1;
                    }
                }
                //UnLink Portal
                if (Input.GetKeyDown(KeyCode.X))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        controller.portalManager.RequestUnlinkPortal(portalObj.GetComponent<Portal>());
                    }
                }
            }
            else
            {
                Debug.LogError("No World Set!");
            }
        }
    }

    private void PlayerMovementControls()
    {

        #region WASD
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            //transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity += transform.forward * acceleration;
            
        }        

        if (Input.GetKey(KeyCode.A))
        {
            //transform.localPosition -= transform.right * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.right * movementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //transform.localPosition -= transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.forward * movementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //transform.localPosition += transform.right * Time.deltaTime * movementSpeed;
            moveVelocity += transform.right * movementSpeed;
        }
        rigidBody.velocity = moveVelocity;



        #endregion

        #region CAM_ROTATE
        if (Input.GetMouseButton(1))
        {
            float deltaMouseX = Input.GetAxis("Mouse X");
            float deltaMouseY = Input.GetAxis("Mouse Y");
            
            transform.Rotate(Vector3.up, deltaMouseX * rotateSpeed);
            userCamera.transform.Rotate(Vector3.right, -deltaMouseY * rotateSpeed);
        }
        #endregion
    }
}
