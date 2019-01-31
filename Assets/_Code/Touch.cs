using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

public class Touch : MonoBehaviour
{
    public enum Layers
    {
        None = 0,
        Piece = 10,
        Controller = 11,
    }
    public enum Mask
    {
        None = Layers.None,
        Piece = 1 << Layers.Piece,
        Controller = 1 << Layers.Controller,
    }

    public OVRInput.Controller Controller;
    public SphereCollider Grabber;

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
        // If not holding something
        if (GrabbedCollider == null)
        {
            // And we pressed the trigger
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller))
            {
                // Try grab a collider that we are touching
                int mask = (int)Mask.Piece; // + (int)Mask.Controller;
                float radius = Grabber.radius * Grabber.transform.localScale.x;
                Collider[] colliders = Physics.OverlapSphere(Grabber.transform.position, radius, mask);
                foreach (Collider collider in colliders)
                {
                    GrabbedCollider = collider;
                    collider.transform.parent = Grabber.transform;
                    break;
                }
            }
        }
        else
        {
            // Have released the trigger?
            if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller))
            {
                GrabbedCollider.transform.parent = null;
                GrabbedCollider = null;
            }
        }
    }
}
