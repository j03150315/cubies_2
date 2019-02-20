using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Touch : Controller
{
    public OVRInput.Controller Controller;
    public float RotateSpeed = 1f;
    public bool Teleporting = false;

    // Start is called before the first frame update
    private void Start()
    {
        base.Init();
        UnityEngine.XR.XRSettings.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller);
        // If not holding something
        if (GrabbedCollider == null)
        {
            // Project the sphere
            Ray ray = new Ray(transform.position, transform.forward);
            ProjectGrabber(ray);

            // And we pressed the trigger
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Controller))
                Grab();
        }
        else
        {
            // Have released the trigger?
            if (!OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Controller))
            {
                Release();
            }
            else
            {
                // Pull object closer
                PullGrabbedObject();

                // Check for rotation
                //RotateGrabbed(new Vector3(stick.x, stick.y, 0));

                // Check for highlight
                Piece piece = GrabbedCollider.GetComponentInParent<Piece>();
                piece.TryHighlight();
            }
        }

        // -------- MOVEMENT -----------
        if (OVRInput.Get(OVRInput.Button.One, Controller))
        {
            if (!Teleporting)
            {
                Vector3 dest = Grabber.transform.position;
                Teleport(dest);
                Teleporting = true;
            }
        }
        else
        {
            Teleporting = false;
        }

        //RotateCubie(new Vector3(0, stick.x, 0));
    }

    void RotateGrabbed(Vector3 xyzRot)
    {
        Vector3 rot = GrabbedCollider.transform.parent.localRotation.eulerAngles;
        rot += xyzRot * RotateSpeed;
        GrabbedCollider.transform.parent.localRotation = Quaternion.Euler(rot);
    }

    void RotateCubie(Vector3 xyzRot)
    {
        Transform tx = App.Inst.CurrentCubie.transform;
        Vector3 rot = tx.localRotation.eulerAngles;
        rot += xyzRot * RotateSpeed;
        tx.localRotation = Quaternion.Euler(rot);
    }

}
