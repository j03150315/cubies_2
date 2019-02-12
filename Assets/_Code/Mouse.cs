using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Mouse : Controller
{
    public float ScrollSpeed = 1f;
    public float RotateSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateInput();
    }

    void UpdateInput()
    {
        // Get mouse input
        float scroll = Input.mouseScrollDelta.y;
        bool leftButtonDown = Input.GetMouseButton(0);
        bool rightButtonDown = Input.GetMouseButton(1);

        // If not holding something
        if (GrabbedCollider == null)
        {
            // And we pressed the trigger
            if (leftButtonDown)
                Grab();
        }
        else
        {
            // Have released the trigger?
            if (!leftButtonDown)
            {
                Release();
            }
            else
            {
                // Check for rotating the object
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    RotateGrabbed(new Vector3(scroll, 0, 0));
                    scroll = 0;
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    RotateGrabbed(new Vector3(0, scroll, 0));
                    scroll = 0;
                }
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    RotateGrabbed(new Vector3(0, 0, scroll));
                    scroll = 0;
                }

                // Check for highlight
                Piece piece = GrabbedCollider.GetComponentInParent<Piece>();
                piece.TryHighlight();
            }
        }

        // Check for reaching with the sphere
        if (scroll != 0f)
        {
            Vector3 localPos = Grabber.transform.localPosition;
            localPos.z += scroll * ScrollSpeed;
            Grabber.transform.localPosition = localPos;
        }
    }

    void RotateGrabbed(Vector3 xyzRot)
    {
        Vector3 rot = GrabbedCollider.transform.localRotation.eulerAngles;
        rot += xyzRot * RotateSpeed;
        GrabbedCollider.transform.localRotation = Quaternion.Euler(rot);
    }
}
