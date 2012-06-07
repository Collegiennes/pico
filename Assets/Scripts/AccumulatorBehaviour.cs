using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccumulatorBehaviour : MonoBehaviour
{
    readonly float FallDownSpeed = UnityEngine.Random.Range(0.25f, 2);

    public bool FallingDown;

    GameObject[] Halves;
    GameObject Inner;
    bool Full, Incubated;

    Direction LastDirection;
    readonly List<EnergyInfo> TemporaryEnergy = new List<EnergyInfo>();
    Color? ContainedEnergy;

    void Start()
    {
        Halves = new []
        {
            gameObject.FindChild("First Half"), 
            gameObject.FindChild("Second Half")
        };

        Inner = gameObject.FindChild("Inner");
        Inner.renderer.enabled = false;

        foreach (var half in Halves)
            half.renderer.enabled = false;

        SetAlpha();
    }

    void SetAlpha()
    {
        var eased = Easing.EaseInOut(Mathf.Clamp01(Pico.Level.SinceAlive), EasingType.Sine);
        var r = gameObject.FindChild("Empty-Second Half").renderer;
        var c = r.material.GetColor("_TintColor");
        r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (14 / 255f)));
        r = gameObject.FindChild("Empty-First Half").renderer;
        c = r.material.GetColor("_TintColor");
        r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (14 / 255f)));
        r = gameObject.FindChild("CubeInsideout").renderer;
        c = r.material.GetColor("_TintColor");
        r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (13 / 255f)));
    }

    public bool AddEnergy(EnergyInfo energy)
    {
        TemporaryEnergy.Add(energy);
        return true;
    }

    public void Empty()
    {
        TemporaryEnergy.Clear();
        ContainedEnergy = null;

        Inner.renderer.enabled = false;

        foreach (var half in Halves)
            half.renderer.enabled = false;
    }

    void Update()
    {
        SetAlpha();

        if (FallingDown)
        {
            transform.position += Vector3.down * Pico.FallDistance * FallDownSpeed;
        }
    }

    public void LateUpdate()
    {
        if (Pico.Heartbeat)
        {
            // Projection
            if (Incubated)
            {
                var go = Instantiate(Pico.EnergyTemplate) as GameObject;
                go.transform.position = transform.position;
                go.renderer.material.color = ContainedEnergy.Value;
                var behaviour = go.GetComponent<EnergyBehaviour>();
                behaviour.Direction = LastDirection;
                behaviour.Destination = transform.position;
                behaviour.Update(); // Force move & test

                // Reset
                ContainedEnergy = null;
                Incubated = false;
                Inner.renderer.enabled = false;

                //if (behaviour.ScheduleDestroy) // Well, shit
                //{
                //    foreach (var projector in FindObjectsOfType(typeof(ProjectorBehaviour)).Cast<ProjectorBehaviour>())
                //        projector.LateUpdate();
                //    foreach (var otherAccumulator in FindObjectsOfType(typeof(AccumulatorBehaviour)).Cast<AccumulatorBehaviour>())
                //        if (otherAccumulator != this)
                //            otherAccumulator.LateUpdate();
                //}
            }
        }

        if (Pico.PostHeartbeat)
        {
            // Incubate
            if (Full && !Incubated)
            {
                Inner.renderer.enabled = true;
                Inner.renderer.material.color = ContainedEnergy.Value;

                foreach (var half in Halves)
                    half.renderer.enabled = false;

                Full = false;
                Incubated = true;
            }

            // Blend & digest energy
            if (TemporaryEnergy.Count > 1)
            {
                // Reject (re-create input)
                foreach (var energy in TemporaryEnergy)
                {
                    var go = Instantiate(Pico.EnergyTemplate) as GameObject;
                    go.renderer.material.color = energy.Color;
                    go.transform.position = transform.position - energy.Direction.ToVector();
                    var behaviour = go.GetComponent<EnergyBehaviour>();
                    behaviour.Direction = energy.Direction;
                    behaviour.Destination = transform.position;
                }

                TemporaryEnergy.Clear();
            }
            else if (TemporaryEnergy.Count == 1)
            {
                var blend = TemporaryEnergy[0].Color.Saturate();
                if (ContainedEnergy.HasValue && blend != ContainedEnergy.Value)
                {
                    // Reject (re-create input)
                    foreach (var energy in TemporaryEnergy)
                    {
                        var go = Instantiate(Pico.EnergyTemplate) as GameObject;
                        go.renderer.material.color = energy.Color;
                        go.transform.position = transform.position - energy.Direction.ToVector();
                        var behaviour = go.GetComponent<EnergyBehaviour>();
                        behaviour.Direction = energy.Direction;
                        behaviour.Destination = transform.position;
                    }
                }
                else
                {
                    // Accept
                    Full = ContainedEnergy.HasValue;
                    var newHalfIndex = Full ? 1 : 0;
                    Halves[newHalfIndex].renderer.enabled = true;
                    Halves[newHalfIndex].renderer.material.color = blend;
                    ContainedEnergy = blend;

                    LastDirection = TemporaryEnergy[0].Direction;
                }

                TemporaryEnergy.Clear();
            }
        }
    }
}
