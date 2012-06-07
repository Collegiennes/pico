using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectorBehaviour : MonoBehaviour
{
    readonly float FallDownSpeed = UnityEngine.Random.Range(0.25f, 2);

    Direction projectInto;

    readonly List<Color> TemporaryEnergy = new List<Color>();
    public Color? ContainedEnergy;

    public bool FallingDown;
    GameObject Inner, FaceSelector;
    public bool IsGizmo;

    public Direction ProjectInto
    {
        get { return projectInto; }
        set
        {
            projectInto = value;
            var pyramid = gameObject.FindChild("Pyramid");
            pyramid.transform.LookAt(transform.position + projectInto.ToVector());
            pyramid.transform.rotation = pyramid.transform.rotation * Quaternion.AngleAxis(90, Vector3.right);
        }
    }

    void Start() 
    {
        Inner = gameObject.FindChild("Inner");
        Inner.renderer.enabled = false;
        FaceSelector = gameObject.FindChild("Highlight Face");
        FaceSelector.GetComponentInChildren<Renderer>().enabled = false;

        SetAlpha();
    }

    public bool AddEnergy(Color color)
    {
        TemporaryEnergy.Add(color);
        //Debug.Log("Projector @ " + transform.position + " absorbed a " + color);
        return true;
    }

    public void Empty()
    {
        Inner.renderer.enabled = false;
        TemporaryEnergy.Clear();
        ContainedEnergy = null;
    }

    void SetAlpha()
    {
        if (IsGizmo)
        {
            if (FaceSelector.GetComponentInChildren<Renderer>().enabled == false)
            {
                var r = gameObject.FindChild("CubeInsideout").renderer;
                var c = r.material.GetColor("_TintColor");
                r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, (13 / 255f) * (Mathf.Sin(Time.realtimeSinceStartup * 10) * 0.25f + 1)));
            }
            else
            {
                var r = gameObject.FindChild("CubeInsideout").renderer;
                var c = r.material.GetColor("_TintColor");
                r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, (13 / 255f)));

                r = FaceSelector.GetComponentInChildren<Renderer>();
                c = r.material.color;
                r.material.color = new Color(c.r, c.g, c.b, 1 + Mathf.Sin(Time.realtimeSinceStartup * 10) * 0.25f);
            }
        }
        else
        {
            var eased = Easing.EaseInOut(Mathf.Clamp01(Pico.Level.SinceAlive), EasingType.Sine);
            var r = gameObject.FindChild("CubeInsideout").renderer;
            var c = r.material.GetColor("_TintColor");
            r.material.SetColor("_TintColor", new Color(c.r, c.g, c.b, eased * (13 / 255f)));
            //r = gameObject.FindChild("Pyramid").renderer;
            //r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, eased);
        }
    }

    void Update()
    {
        SetAlpha();

        if (FallingDown)
            transform.position += Vector3.down * Pico.FallDistance * FallDownSpeed;
    }

    public void LateUpdate()
    {
        if (Pico.Heartbeat)
        {
            // Projection
            if (ContainedEnergy.HasValue)
            {
                var go = Instantiate(Pico.EnergyTemplate) as GameObject;
                go.renderer.material.color = ContainedEnergy.Value;
                go.transform.position = transform.position;
                var behaviour = go.GetComponent<EnergyBehaviour>();
                behaviour.Direction = ProjectInto;
                behaviour.Destination = transform.position;
                behaviour.Update(); // Force move & test

                // Reset
                ContainedEnergy = null;
                Inner.renderer.enabled = false;
            }
        }

        if (Pico.PostHeartbeat)
        {
            // Blend & digest energy
            if (TemporaryEnergy.Count > 0)
            {
                //Debug.Log("Digesting @ " + transform.position + ", energy is " + StringHelper.DeepToString(TemporaryEnergy));

                var additive = TemporaryEnergy.Aggregate((a, b) => a + b);
                //Debug.Log("Additive color is " + additive);
                var saturate = additive.Saturate();
                //Debug.Log("Saturated color is " + saturate);
                ContainedEnergy = saturate;
                Inner.renderer.material.color = saturate;
                Inner.renderer.enabled = true;

                TemporaryEnergy.Clear();
            }
        }
    }
}
