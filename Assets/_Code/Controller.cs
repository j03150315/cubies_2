using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public SphereCollider Grabber;
    public Collider GrabbedCollider;
    public Material RedMat;
    public Material GreenMat;
    public Material BlueMat;
    Renderer GrabberRenderer = null;

    private void Start()
    {
        GrabberRenderer = Grabber.GetComponent<Renderer>();
    }

    public void ProjectGrabber(Ray ray)
    {
        RaycastHit hit;
        int mask = (int)App.Mask.Piece + (int)App.Mask.Default;
        if (Physics.Raycast(ray, out hit, 1000f, mask))
        {
            Piece piece = hit.transform.GetComponent<Piece>();
            if (piece == null)
                GrabberRenderer.material = BlueMat;
            else if (piece.Placed)
                GrabberRenderer.material = RedMat;
            else
                GrabberRenderer.material = GreenMat;

            // Move the grabber
            Grabber.transform.position = hit.point;
        }
    }

    public void Grab()
    {
        // Try grab a collider that we are touching
        int mask = (int)App.Mask.Piece; // + (int)Mask.Controller;
        float radius = Grabber.radius * Grabber.transform.localScale.x;
        Collider[] colliders = Physics.OverlapSphere(Grabber.transform.position, radius, mask);
        foreach (Collider collider in colliders)
        {
            // Don't grab placed pieces
            Piece piece = collider.GetComponent<Piece>();
            if (piece == null || piece.Placed)
                continue;

            GrabbedCollider = collider;
            //collider.transform.parent = Grabber.transform;
            collider.transform.parent = Grabber.transform;

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
        GrabbedCollider.transform.parent = null;
        GrabbedCollider = null;

        piece.TrySnap();
    }
}
