using UnityEngine;

public class EmitterBehaviour : MonoBehaviour
{
    readonly float FallDownSpeed = UnityEngine.Random.Range(0.25f, 2);

    public Direction EmitInto;
    public bool FallingDown;

    GameObject BuildingEnergy;

    void Start()
    {
        BuildingEnergy = gameObject.FindChild("BuildingEnergy");
        var r = BuildingEnergy.renderer;
        r.material.color = renderer.material.color;

        r = gameObject.FindChild("CubeInsideout").renderer;
        var c = r.material.color;
        r.material.color = new Color(c.r, c.g, c.b, 0);

        BuildingEnergy.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        var eased = Easing.EaseInOut(Mathf.Clamp01(Pico.Level.SinceAlive), EasingType.Sine);
        var r = gameObject.FindChild("CubeInsideout").renderer;
        var c = r.material.color;
        r.material.color = new Color(c.r, c.g, c.b, eased / 2);

        if (Pico.BeatsLeftToGeneration == 1 && !FallingDown)
        {
            var ev = EmitInto.ToVector();
            var step = (1 - (Pico.TimeLeftToHeartbeat / Pico.HeartbeatDuration));
            BuildingEnergy.transform.localScale = ev.Abs() * step * 0.5f + (Vector3.one - ev.Abs()) * 0.5f;
            BuildingEnergy.transform.localPosition = ev * step * 0.25f - ev * 0.25f;
        }

        if (Pico.Generation)
        {
            BuildingEnergy.transform.localScale = Vector3.zero;

            var go = Instantiate(Pico.EnergyTemplate) as GameObject;
            go.transform.position = transform.position;
            go.renderer.material.color = renderer.material.color;
            var behaviour = go.GetComponent<EnergyBehaviour>();
            behaviour.Destination = transform.position;
            behaviour.Direction = EmitInto;
            behaviour.Update(); // Force move & test
            if (FallingDown)
                behaviour.FallingDown = true;
        }

        if (FallingDown)
        {
            transform.position += Vector3.down * Pico.FallDistance * FallDownSpeed;
        }
    }
}
