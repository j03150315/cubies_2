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

    public static App Inst;

    public Cubie CurrentCubie;
    public float SnapDistance = 0.1f;
    public float SnapAngle = 45f;
    public Material HighlightMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        Inst = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
