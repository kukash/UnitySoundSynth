using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Helper;
using System.Threading;
[System.Serializable]
public struct SubInstrument
{
    public Oscillator.WaveType Wave;

    public bool Active;
    public float Volume;
    public float FrequencyAmplifier;
    public float LFOFreq;
    public float LFOAmp;
    public SubInstrument(Oscillator.WaveType newWave, bool isActive, float pVolume, float pfrequencyAmplifier, float pLFOFrequency, float pLFOAmplifier)
    {
        Wave = newWave;
        Active = isActive;
        Volume = pVolume;
        FrequencyAmplifier = pfrequencyAmplifier;
        LFOAmp = pLFOAmplifier;
        LFOFreq = pLFOFrequency;

    }
}
[System.Serializable]
public struct Instrument
{
    public double Volume;
    public Envelope Envelope;
    public List<SubInstrument> SubInstruments;
    public static Instrument OverwriteEnvelope(Instrument instr, Envelope newEnv, List<SubInstrument> subs)
    {
        Instrument ret = instr;
        ret.Envelope = newEnv;
        ret.SubInstruments = subs;
        return ret;
    }
    public static float MakeNoise(Instrument i, Note note, double t = 0)
    {
        double frequency = Oscillator.GetNoteFrequency(note.noteNumber);
        float value = 0;
        //iterate all sub instruments
        foreach (SubInstrument s in i.SubInstruments)
        {
            //create the sound based on the sub instruments parameters
            if (!s.Active) continue;
            value += (float)Oscillator.Osc(frequency * s.FrequencyAmplifier, t, s.Wave, s.LFOFreq, s.LFOAmp);
        }
        return value;
    }


}


[System.Serializable]
public struct Envelope
{
    public double Attack;
    public double Decay;

    public double Release;
    public double Amplitude;
    public double StartAmp;

    public double timePressed;
    public double timeReleased;
    public bool IsOn;

    public Envelope(double pAttack = 0.1, double pDecay = 0.1, double pStartAmp = 1.0, double pAmp = 0.8, double pRelease = 0.2, bool pOn = false, double pPressed = 0, double pReleased = 0)
    {
        Attack = pAttack;
        Decay = pDecay;
        StartAmp = pStartAmp;
        Amplitude = pAmp;
        Release = pRelease;

        timePressed = pPressed;
        timeReleased = pReleased;
        IsOn = pOn;
    }


    public static Envelope MakeOn(Envelope copyEnvelope, double oT)
    {
        Envelope ret = copyEnvelope;
        if (oT > 0)
        {
            ret.IsOn = true;
            ret.timePressed = oT;
        }

        return ret;
    }

    public static Envelope MakeOff(Envelope copyEnvelope, double rT)
    {
        Envelope ret = copyEnvelope;
        if (rT > 0)
        {
            ret.IsOn = false;
            ret.timeReleased = rT;
        }

        return ret;
    }

    public double GetAmplitude(double t, double timeOn, double TimeOff)
    {
        double returnAmp = 0;
        double releaseAmp = 0;
        //note is on
        if (timeOn > TimeOff)
        {
            //calculate current time
            double lifetime = t - timeOn;
            if (lifetime <= Attack)
                returnAmp = (lifetime / Attack) * StartAmp + StartAmp * 0.001f;
            //decay, gradient from attack and to decay end 
            else if (lifetime <= Decay + Attack)
                returnAmp = ((lifetime - Attack) / Decay) * (Amplitude - StartAmp) + StartAmp;
            //sustain, constant amplitude
            else
                returnAmp = Amplitude;
        }
        //note is off
        else
        {
            double endTime = TimeOff - timeOn;
            if (endTime <= Attack)
                releaseAmp = endTime / Attack * StartAmp;
            else if (endTime > Attack && endTime <= Attack + Decay)
                releaseAmp = ((endTime - Attack) / Decay) * (Amplitude - StartAmp) + StartAmp;
            else
                releaseAmp = Amplitude;

            //0-1, 0 on key release, 1 on completely faded
            double r = (t - TimeOff) / Release;
            //if 0 amplitude stays the same, as r reaches 1 amplitude decreases
            returnAmp = releaseAmp * (1 - r);
        }
        if (returnAmp <= 0.001f)
        {
            returnAmp = 0;
        }
        return returnAmp;
    }
}


public class Oscillator : MonoBehaviour
{

    public Instrument instrument;
    private Mutex m_mut = new Mutex();
    [Range(0, 1)] public static double noise_gain = 0.1f;
    public enum WaveType
    {
        TRIANGLE = 0,
        SQUARE,
        SINE,
        SAW,
        NOISE_UNITY,
        NOISE_SYSTEM
    }
    private List<Note> m_activeNotes;
    public WaveType Wave = WaveType.SINE;
    [SerializeField] private double m_frequency = 440.0;
    [SerializeField] [Range(0, 1.0f)] float m_baseGain = 0.1f;
    [SerializeField] float m_LFOAmplitude = 0.01f;
    [SerializeField] float m_LFOFrequ = 0.1f;

    private double increment;
    private double m_phase;
    private double m_sampling_frequency = 48000.0;

    private static Unity.Mathematics.Random m_Mrand;
    private static System.Random m_Srand;
    private List<Note> m_noteReleaseBuffer;
    public Oscillator()
    {
        m_mut = new Mutex();
    }
    ~Oscillator()
    {
        m_mut.Dispose();
    }
    private void Awake()
    {
        m_noteReleaseBuffer = new List<Note>();
        m_activeNotes = new List<Note>();
        m_phase = 0;
        m_Mrand = new Unity.Mathematics.Random(1);
        m_Srand = new System.Random();
        m_sampling_frequency = AudioSettings.outputSampleRate;
    }
    public void PlayNote(Note note, float velocity)
    {
        Debug.Log(note.noteNumber);
        Debug.Log(GetNoteFrequency(note.noteNumber));
        //Find note index
        int index = m_activeNotes.FindIndex(x => x.noteNumber == note.noteNumber);
        note.On = AudioSettings.dspTime;

        //no note match was found, add as new note
        if (index < 0)
        {
            m_mut.WaitOne();
            m_activeNotes.Add(note);
            m_mut.ReleaseMutex();
        }
        else
        {
            //note was already in list, add new note
            m_mut.WaitOne();
            //write new note
            m_activeNotes[index] = note;
            m_mut.ReleaseMutex();
        }

    }
    public void ReleaseNote(Note note)
    {
        //find note
        int index = m_activeNotes.FindIndex(x => x.noteNumber == note.noteNumber);
        //no note was found
        if (index < 0)
        {
            Debug.LogWarning("could not find note to release");
            return;
        }
        Note newNote = m_activeNotes[index];
        //change note data
        newNote.Off = AudioSettings.dspTime;
        //write new data
        m_mut.WaitOne();
        m_activeNotes[index] = newNote;
        m_mut.ReleaseMutex();
    }
    public static double GetNoteFrequency(int index)
    {
        //a4 is at frequency 444.0 with key index 69
        int deltaA4 = index - 69;
        return 440.0 * math.pow(1.059463094359, deltaA4);
    }

    public static double Osc(double frequency, double time, WaveType waveType, double LFOfreq = 0.0, double LFOAmp = 0.0)
    {
        double baseFreq = HzToVel(frequency) * time + LFOAmp * frequency * math.sin(HzToVel(LFOfreq) * time);
        switch (waveType)
        {
            case WaveType.SINE:
                {
                    return math.sin(baseFreq);
                }
            case WaveType.TRIANGLE:
                {
                    return math.asin(math.sin(baseFreq)) * 2.0 / math.PI_DBL;
                }
            case WaveType.SQUARE:
                {
                    if (math.sin(baseFreq) >= 0) return 0.6f;
                    else return -0.6f;
                }
            case WaveType.SAW:
                {
                    return (2.0 / math.PI_DBL) * (baseFreq * math.PI_DBL * math.fmod(time, 1.0 / baseFreq) - (math.PI_DBL / 2.0));
                }
            case WaveType.NOISE_UNITY:
                {
                    return (2.0 * m_Mrand.NextDouble() - 1.0) * noise_gain;
                }
            case WaveType.NOISE_SYSTEM:
                {
                    return (2.0 * m_Srand.NextDouble() - 1.0) * noise_gain;
                }
        }
        return 0;

    }
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (m_baseGain <= 0) return;
        if (m_activeNotes.Count <= 0) return;

        int noteCount = m_activeNotes.Count;
        //iterate active notes
        double time = AudioSettings.dspTime;
        m_mut.WaitOne();
        increment = 1.0 / m_sampling_frequency;
        for (int i = 0; i < data.Length; i += channels)
        {
            m_phase += increment;

            data[i] += MakeNoise(time);
            if (channels == 2) data[i + 1] += MakeNoise(time);
            if (m_phase > math.PI_DBL * 2) m_phase = 0;
            //data[i] += 0.6f * math.sin(2.0f * math.PI * (float)(time + m_phase));
            //if (channels == 2) data[i + 1] += 0.6f * math.sin(2.0f * math.PI * (float)(time + m_phase));
            //if (m_phase > math.PI_DBL * 2) m_phase = 0;

        }

        //After sampling sound we can remove all notes that expired
        foreach (Note n in m_noteReleaseBuffer) m_activeNotes.Remove(n);
        m_mut.ReleaseMutex();

        //make sure the release buffer is clear
        m_noteReleaseBuffer.Clear();
    }

    float MakeNoise(double t = 0)
    {
        if (m_activeNotes.Count <= 0) return 0;

        float output = 0;
        for (int i = 0; i < m_activeNotes.Count; i++)
        {
            Note note = m_activeNotes[i];
            double frequency = GetNoteFrequency(note.noteNumber);

            float amp = (float)instrument.Envelope.GetAmplitude(t, note.On, note.Off);
            //check if note sound was finished, if note is not active and sound has decayed this is true
            if (note.Off > note.On && amp <= 0.001)
            {
                note.Active = false;
                m_activeNotes[i] = note;
                m_noteReleaseBuffer.Add(note);
            }
            float val = Instrument.MakeNoise(instrument, note, m_phase);
            output += amp * val;
        }
        //clear notes
        foreach (Note n in m_noteReleaseBuffer) m_activeNotes.Remove(n);
        m_noteReleaseBuffer.Clear();

        //  output *= m_baseGain;
        return output;




        //(float)(m_gain *
        //((amp * Osc(frequency, m_phase, m_waveType, m_LFOFrequ, m_LFOAmplitude))
        //+ amp * 0.5 * Osc(frequency * 1.5, m_phase, m_waveType)
        //+ amp * 0.25 * Osc(frequency * 2, m_phase, m_waveType)
        //+ amp * 0.05 * Osc(frequency, m_phase, WaveType.NOISE_UNITY)))
        //;

    }


    private static double HzToVel(double Hz) => 2.0 * math.PI_DBL * Hz;
    private float HzToVel(float Hz) => 2.0f * math.PI * Hz;

}
