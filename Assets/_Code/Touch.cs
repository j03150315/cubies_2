using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Touch : Controller
{
    public OVRInput.Controller Controller;
    public float RotateSpeed = 1f;

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
        // If not holding something
        if (GrabbedCollider == null)
        {
            // And we pressed the trigger
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller))
                Grab();
        }
        else
        {
            // Have released the trigger?
            if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller))
            {
                Release();
            }
            else
            {
                // Check for rotation
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller);
                RotateGrabbed(new Vector3(stick.x, stick.y, 0));

                // Check for highlight
                Piece piece = GrabbedCollider.GetComponentInParent<Piece>();
                piece.TryHighlight();
            }
        }
    }

    void RotateGrabbed(Vector3 xyzRot)
    {
        Vector3 rot = GrabbedCollider.transform.parent.localRotation.eulerAngles;
        rot += xyzRot * RotateSpeed;
        GrabbedCollider.transform.parent.localRotation = Quaternion.Euler(rot);
    }

}
