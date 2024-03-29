using UnityEngine;
using UnityEngine.InputSystem;

// NoteCallback.cs - This script shows how to define a callback to get notified
// on MIDI note-on/off events.


public class NoteCallback : MonoBehaviour
{

    public Oscillator m_Osc;
    public bool visualizeKeyPressed = true;
    public PianoKeyManager pianomngr;
    void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                // Note that you can't use note.velocity because the state
                // hasn't been updated yet (as this is "will" event). The note
                // object is only useful to specify the target note (note
                // number, channel number, device name, etc.) Use the velocity
                // argument as an input note velocity.
                Debug.Log(string.Format(
                    "Note On #{0} ({1}) vel:{2:0.00} ch:{3} dev:'{4}'",
                    note.noteNumber,
                    note.shortDisplayName,
                    velocity,
                    (note.device as Minis.MidiDevice)?.channel,
                    note.device.description.product
                ));
                if (visualizeKeyPressed)
                {
                    Transform t = pianomngr.GetKey(note.noteNumber - 21);
                    KeyAnimator.PressKey(t);
                }
                m_Osc?.PlayNote(new Note(note.noteNumber, velocity), velocity);
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                Debug.Log(string.Format(
                    "Note Off #{0} ({1}) ch:{2} dev:'{3}'",
                    note.noteNumber,
                    note.shortDisplayName,
                    (note.device as Minis.MidiDevice)?.channel,
                    note.device.description.product
                ));
                if (visualizeKeyPressed)
                {
                    Transform t = pianomngr.GetKey(note.noteNumber - 21);
                    KeyAnimator.ReleaseKey(t);
                }
                m_Osc?.ReleaseNote(new Note(note.noteNumber, 0));
            };
        };
    }
}