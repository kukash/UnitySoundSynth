using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Note
{
    public Note (int index, float vel)
    {

        noteNumber = index;
        Velocity = vel;
        On = 0;
        Off = 0;
        Active = true;
    }
    public int noteNumber;
    public float Velocity;
    //Time when note was played
    public double On;
    public double Off;
    public bool Active;
}
