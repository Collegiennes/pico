using System;
using System.Collections.Generic;
using UnityEngine;

public class ReceiverBehaviour : MonoBehaviour
{
    float FallDownSpeed = UnityEngine.Random.Range(0.25f, 2);

    public bool FallingDown;

    GameObject[] Parts;
    float[] PartDestOpacity;

    public Color[] ColorSequence;
    public Func<Level> NextLevel { get; set; }

    public readonly List<Color> AbsorbedEnergy = new List<Color>();

    void Start()
    {
        Parts = new GameObject[ColorSequence.Length];
        PartDestOpacity = new float[ColorSequence.Length];
        var center = ColorSequence.Length / 2f;
        for (int i = 0; i < ColorSequence.Length; i++)
        {
            Parts[i] = gameObject.FindChild("Part" + (i + 1));
            var offset = (center - (i + 0.5f)) / center;
            Parts[i].transform.localPosition = new Vector3(0, -offset * 0.9f, 0);
            Parts[i].transform.localScale = new Vector3(1.5f, 2f / ColorSequence.Length * 0.675f, 1.5f);
            PartDestOpacity[i] = 14 / 255f;
        }

        for (int i = ColorSequence.Length; i < 3; i++)
            Destroy(gameObject.FindChild("Part" + (i + 1)));

        for (int i = 0; i < Parts.Length; i++)
        {
            var c = ColorSequence[i % ColorSequence.Length];
            Parts[i].renderer.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, 0));
        }

        SetAlpha();
    }

    void SetAlpha()
    {
        var eased = Easing.EaseInOut(Mathf.Clamp01(Pico.Level.SinceAlive), EasingType.Sine);

        if (Pico.Level.SinceAlive > 1) return;

        //var r = renderer;
        //var c = r.material.color;
        //r.material.color = new Color(c.r, c.g, c.b, eased);
        //var r = gameObject.FindChild("Outer").renderer;
        //var c = r.material.GetColor("_TintColor");
        //r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (13 / 255f)));

        foreach (var part in Parts)
        {
            var c = part.renderer.material.GetColor("_TintColor");
            part.renderer.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (14 / 255f)));
        }
    }

    void Update()
    {
        for (int i = 0; i < Parts.Length; i++)
        {
            var part = Parts[i];
            var c = part.renderer.material.GetColor("_TintColor");
            part.renderer.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, PartDestOpacity[i], 0.1f)));
        }

        if (FallingDown)
        {
            transform.position += Vector3.down * Pico.FallDistance * FallDownSpeed;
            return;
        }

        SetAlpha();
    }

    public void Empty()
    {
        AbsorbedEnergy.Clear();

        for (int i = 0; i < Parts.Length; i++)
            PartDestOpacity[i] = 13 / 255f;
    }

    public bool Absorb(EnergyInfo energy)
    {
        AbsorbedEnergy.Add(energy.Color);

        for (int i = 0; i < ColorSequence.Length; i++)
        {
            if (i == AbsorbedEnergy.Count)
            {
                Debug.Log("Not enough energy yet");
                return true;
            }

            if (ColorSequence[i] != AbsorbedEnergy[i])
            {
                Debug.Log("Energy signature did not match (expected " + ColorSequence[i] + ", got " + AbsorbedEnergy[i] + ")");
                Empty();
                return false;
            }
            // Else, accept
            PartDestOpacity[i] = 64 / 255f;
        }

        Debug.Log("Energy sequence is complete!");

        AbsorbedEnergy.Clear();
        Pico.CycleLevels(NextLevel);
        return true;
    }
}
