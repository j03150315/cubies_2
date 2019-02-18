using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Mouse : Controller
{
    public float ScrollSpeed = 1f;
    public float RotateSpeed = 2f;
    public float GrabDistance = 2f;
    public float GrabSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
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
            // Project the sphere
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            ProjectGrabber(ray);

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
                // Pull object closer
                Transform gtx = Grabber.transform;
                float scale = gtx.localScale.z;
                //Vector3 vdist = Vector3.Scale(gtx.localPosition, gtx.localScale);
                Vector3 vdist = gtx.localPosition;
                float dist = vdist.magnitude;
                if (dist > GrabDistance * scale)
                {
                    dist -= Time.deltaTime * GrabSpeed;
                    Vector3 vdir = vdist.normalized;
                    gtx.localPosition = vdir * dist;
                }

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
