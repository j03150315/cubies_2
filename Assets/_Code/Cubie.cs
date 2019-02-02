using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubie : MonoBehaviour
{
    public Piece[] Pieces;
    public Transform Base;

    // Start is called before the first frame update
    void Start()
    {
        Pieces = GetComponentsInChildren<Piece>();
        Pieces[0].SetPlaced();

        Base = transform.Find("Base");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
