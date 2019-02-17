using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public SphereCollider Grabber;
    public Collider GrabbedCollider;

    public void ProjectGrabber(Ray ray)
    {
        RaycastHit hit;
        int mask = (int)App.Mask.Piece + (int)App.Mask.Default;
        if (Physics.Raycast(ray, out hit, 1000f, mask))
        {
            Piece piece = hit.transform.GetComponent<Piece>();
            if (piece != null)
                HighlightGrabber(true);
            else
                HighlightGrabber(false);

            // Move the grabber
            Grabber.transform.position = hit.point;
        }
    }

    public void HighlightGrabber(bool on)
    {

    }

    public void Grab()
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

    public void Release()
    {
        Piece piece = GrabbedCollider.GetComponentInParent<Piece>();

        // Turn on physics
        Rigidbody body = GrabbedCollider.GetComponentInParent<Rigidbody>();
        body.isKinematic = false;

        // Drop the object
        GrabbedCollider.transform.parent.parent = null;
        GrabbedCollider = null;

        piece.TrySnap();
    }
}
