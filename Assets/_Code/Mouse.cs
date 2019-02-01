using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Mouse : MonoBehaviour
{
    public SphereCollider Grabber;
    public float ScrollSpeed = 1f;
    public float RotateSpeed = 2f;
    Collider GrabbedCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
            // And we pressed the trigger
            if (leftButtonDown)
            {
                // Try grab a collider that we are touching
                int mask = (int)App.Mask.Piece; // + (int)Mask.Controller;
                float radius = Grabber.radius * Grabber.transform.localScale.x;
                Collider[] colliders = Physics.OverlapSphere(Grabber.transform.position, radius, mask);
                foreach (Collider collider in colliders)
                {
                    GrabbedCollider = collider;
                    collider.transform.parent.parent = Grabber.transform;

                    // Turn off physics
                    Rigidbody body = GrabbedCollider.GetComponentInParent<Rigidbody>();
                    body.isKinematic = true;
                    break;
                }
            }
        }
        else
        {
            // Have released the trigger?
            if (!leftButtonDown)
            {
                // Turn on physics
                Rigidbody body = GrabbedCollider.GetComponentInParent<Rigidbody>();
                body.isKinematic = false;

                // Drop the object
                GrabbedCollider.transform.parent.parent = null;
                GrabbedCollider = null;
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

    void RotateGrabbed(Vector3 amount)
    {
        Vector3 rot = GrabbedCollider.transform.localRotation.eulerAngles;
        rot += amount * RotateSpeed;
        GrabbedCollider.transform.localRotation = Quaternion.Euler(rot);
    }
}
