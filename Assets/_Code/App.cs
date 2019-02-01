using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
