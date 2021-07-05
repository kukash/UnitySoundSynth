using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Oscillator))]
public class EnvelopeController : MonoBehaviour
{
    public List<GameObject> InstrumentObjects;
    public List<SubInstrument> subs;
    private Oscillator m_osc;

    public Scrollbar Attack;
    public Scrollbar Decay;
    public Scrollbar Release;
    public Scrollbar Amplitude;
    public Scrollbar StartAmp;
    void Start()
    {
        m_osc = GetComponent<Oscillator>();
        UpdateOsc();
       // subs.Add(new SubInstrument(Oscillator.WaveType.SINE, true, 1, 1, 0, 0));
    }
    public void UpdateOsc()
    {

        if (!Attack || !Decay || !Release || !Amplitude || !StartAmp) return;
        Envelope newEnv = new Envelope(Attack.value, Decay.value, StartAmp.value, Amplitude.value, Release.value);

        GetSubInstrument();
        m_osc.instrument = Instrument.OverwriteEnvelope(m_osc.instrument, newEnv, subs);

    }

    public void UpdateWave(int waveIndex)
    {
        m_osc.Wave = (Oscillator.WaveType)waveIndex;
    }
    private void GetSubInstrument()
    {
        Debug.Log("getting subs");
        subs = new List<SubInstrument>();
        //declare some variables
        Oscillator.WaveType wave = Oscillator.WaveType.SINE;
        bool active = false;
        float volume = 0, Frequency = 0, LFOFrequ = 0, LFOAmp = 0;
        //iterate all objects
        Debug.Log("iterating objects subs");

        foreach (GameObject currentObject in InstrumentObjects)
        {
            Debug.Log("iterating children subs");

            //iterate the children of the current object
            foreach (Transform child in currentObject.transform.transform)
            {
                Debug.Log("checking tags ");

                if (child.CompareTag("Untagged")) continue;
                //find Objects with the correct tag
                if (child.CompareTag("Active"))
                {
                    Debug.Log("found active");
                    active = child.GetComponent<Toggle>().isOn;
                    continue;
                }
                else if (child.CompareTag("Volume"))
                {
                    Debug.Log("found volume");
                    volume = child.GetComponent<Scrollbar>().value;
                    continue;
                }
                else if (child.CompareTag("Frequency"))
                {
                    Debug.Log("found Frequency");

                    Frequency = child.GetComponent<Scrollbar>().value;
                    Frequency *= 2.0f;
                    continue;
                }
                else if (child.CompareTag("LFOFrequency"))
                {
                    Debug.Log("found LFOFrequency");

                    LFOFrequ = child.GetComponent<Scrollbar>().value;
                    LFOFrequ *= 5.0f;
                    continue;
                }
                else if (child.CompareTag("LFOAmp"))
                {
                    Debug.Log("found LFOAmplitude");

                    LFOAmp = child.GetComponent<Scrollbar>().value;
                    LFOAmp *= 0.25f;
                    continue;
                }
                else if (child.CompareTag("Wave"))
                {
                    Debug.Log("found Wave");

                    wave = (Oscillator.WaveType)child.GetComponent<TMPro.TMP_Dropdown>().value;
                    continue;
                }

            }

            subs.Add(new SubInstrument(wave, active, volume, Frequency, LFOFrequ, LFOAmp));

        }

    }
}
