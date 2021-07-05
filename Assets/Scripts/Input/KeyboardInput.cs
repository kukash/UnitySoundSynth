using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class KeyboardInput : MonoBehaviour
{
    [SerializeField] private Oscillator m_Osc;
    [SerializeField] private int m_Octave = 4;
    public Text m_text;
    [SerializeField] private PianoKeyManager pianoMngr;
    // Update is called once per frame
    void Update()
    {
        KeyDown();
        KeyUp();
    }
    public void KeyDown()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayNote(0);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            PlayNote(1);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlayNote(2);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayNote(3);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayNote(4);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayNote(5);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayNote(6);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayNote(7);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            PlayNote(8);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayNote(9);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            PlayNote(10);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayNote(11);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayNote(12);
        }


    }
    public void KeyUp()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            ReleaseNote(0);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            ReleaseNote(1);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            ReleaseNote(2);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            ReleaseNote(3);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            ReleaseNote(4);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            ReleaseNote(5);
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            ReleaseNote(6);
        }
        if (Input.GetKeyUp(KeyCode.G))
        {
            ReleaseNote(7);
        }
        if (Input.GetKeyUp(KeyCode.Y))
        {
            ReleaseNote(8);
        }
        if (Input.GetKeyUp(KeyCode.H))
        {
            ReleaseNote(9);
        }
        if (Input.GetKeyUp(KeyCode.U))
        {
            ReleaseNote(10);
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            ReleaseNote(11);
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            ReleaseNote(12);
        }

    }
    public void PlayNote(int index)
    {
        int NoteIndex = (m_Octave + 2) * 12 + index;
        m_Osc?.PlayNote(new Note(NoteIndex, 1), 1);

        m_text.text = index.ToString();
        KeyAnimator.PressKey(pianoMngr?.GetKey(NoteIndex));
        Debug.Log(NoteIndex);
    }
    public void ReleaseNote(int index)
    {
        int NoteIndex = (m_Octave + 2) * 12 + index;
        m_Osc?.ReleaseNote(new Note(NoteIndex, 1));
        KeyAnimator.ReleaseKey(pianoMngr?.GetKey(NoteIndex));
    }
}
