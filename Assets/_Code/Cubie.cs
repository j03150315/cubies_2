using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubie : MonoBehaviour
{
    public List<Piece> Pieces = new List<Piece>();
    public Transform Base;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetupPrefabCubie()
    {
        Piece[] all = GetComponentsInChildren<Piece>();
        foreach (Piece piece in all)
        {
            Pieces.Add(piece);
        }
        Pieces[0].SetPlaced();

        Base = transform.Find("Base");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
