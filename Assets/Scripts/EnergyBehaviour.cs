using UnityEngine;

public class EnergyBehaviour : MonoBehaviour 
{
    readonly float FallDownSpeed = UnityEngine.Random.Range(0.25f, 2);

    public Direction Direction;
    public Vector3 Destination;

    public bool FallingDown;

    GameObject Shadow;
    readonly SplitState[] Splits = new SplitState[8];

    public bool ScheduleDestroy { get; set; }
    public bool Rejected { get; private set; }
    float SinceRejected;
    bool Initialized;

    void Start()
    {
        Shadow = gameObject.FindChild("Shadow");
        for (int i = 1; i <= 8; i++)
        {
            var splitGO = gameObject.FindChild("Split" + i);
            splitGO.renderer.enabled = false;
            splitGO.renderer.material.color = renderer.material.color;

            Splits[i - 1] = new SplitState(splitGO, Quaternion.Slerp(UnityEngine.Random.rotation, Quaternion.identity, 0.5f));
            Splits[i - 1].RandomDirection = UnityEngine.Random.onUnitSphere / 2 + splitGO.transform.localPosition;
        }
        Initialized = true;
    }

    void LateUpdate()
    {
        if (Shadow != null)
        {
            Shadow.transform.position = new Vector3(transform.position.x, Pico.GridHeight - 0.5f, transform.position.z);
            Shadow.renderer.enabled = Pico.GridHeight <= transform.position.y;
        }
    }

    public void Update()
    {
        if (!Initialized)
            Start();

        if (FallingDown)
        {
            transform.position += Vector3.down * Pico.FallDistance * FallDownSpeed;
            Destination += Vector3.down * Pico.FallDistance * FallDownSpeed;
        }

        var hitWall = false;
        var oldPos = transform.position;
        transform.position = Vector3.Lerp(transform.position, Destination, 0.1f);
        if (Rejected)
        {
            if (Vector3.Distance(transform.position, Destination) < 0.75f)
            {
                hitWall = true;
                transform.position = oldPos;
            }
        }

        if (ScheduleDestroy)
        {
            var distance = Vector3.Distance(transform.position, Destination);
            renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g,
                                                renderer.material.color.b, distance);
            Shadow.renderer.material.SetColor("_TintColor", new Color(0, 0, 0, distance * (9 / 255f)));
        }

        if (Rejected && hitWall)
        {
            SinceRejected += Time.deltaTime;

            foreach (var split in Splits)
            {
                var step = SinceRejected / 0.6f;

                var eased = Easing.EaseIn(step, EasingType.Sine);
                split.GameObject.renderer.material.color = new Color(renderer.material.color.r,
                                                                     renderer.material.color.g,
                                                                     renderer.material.color.b, 1 - eased);

                eased = Easing.EaseOut(step, EasingType.Cubic);
                split.GameObject.transform.localRotation = Quaternion.Slerp(Quaternion.identity,
                                                                            split.DestinationRotation, eased);
                Shadow.renderer.material.SetColor("_TintColor", new Color(0, 0, 0, (1 - eased) * (9 / 255f)));

                split.Velocity -= new Vector3(0, Time.deltaTime * 0.25f, 0);
                split.Velocity -= split.RandomDirection * Time.deltaTime * 0.02f;
                split.Velocity += Direction.ToVector() * Time.deltaTime * 0.025f;
                split.GameObject.transform.position += split.Velocity;
            }

            if (SinceRejected >= 0.6f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (Pico.Heartbeat && !Rejected)
        {
            if (ScheduleDestroy)
            {
                Destroy(gameObject);
                return;
            }

            Destination += Direction.ToVector();

            // Test for projector/accumulator/receiver
            var projectorAt = Pico.Level.GetProjectorAt(Destination);
            if (projectorAt != null)
            {
                var behaviour = projectorAt.GetComponent<ProjectorBehaviour>();
                behaviour.AddEnergy(renderer.material.color);
                ScheduleDestroy = true;
                return;
            }

            var accumulatorAt = Pico.Level.GetAccumulatorAt(Destination);
            if (accumulatorAt != null)
            {
                var behaviour = accumulatorAt.GetComponent<AccumulatorBehaviour>();
                if (behaviour.AddEnergy(GetInfo()))
                    ScheduleDestroy = true;
                return;
            }

            var receiverAt = Pico.Level.GetReceiverAt(Destination);
            if (receiverAt != null)
            {
                var behaviour = receiverAt.GetComponent<ReceiverBehaviour>();
                if (behaviour.Absorb(GetInfo()))
                    ScheduleDestroy = true;
                else
                    Reject();
                return;
            }

            var halfSize = Pico.Level.Size / 2;
            if (Destination.x >= halfSize || Destination.x < -halfSize ||
                Destination.y >= halfSize || Destination.y < -halfSize ||
                Destination.z >= halfSize || Destination.z < -halfSize)
            {
                ScheduleDestroy = true;
            }
        }
    }

    public EnergyInfo GetInfo()
    {
        return new EnergyInfo(renderer.material.color, Direction);
    }

    public void Reject()
    {
        Rejected = true;
        foreach (var split in Splits)
        {
            split.GameObject.renderer.enabled = true;
            split.Velocity = split.RandomDirection * 0.02f + Direction.ToVector() * -0.025f + new Vector3(0, 0.04f, 0);
        }
        renderer.enabled = false;
    }
}

public struct EnergyInfo
{
    public EnergyInfo(Color color, Direction direction)
    {
        Color = color;
        Direction = direction;
    }

    public readonly Color Color;
    public readonly Direction Direction;
}

class SplitState
{
    public SplitState(GameObject gameObject, Quaternion destinationRotation)
    {
        GameObject = gameObject;
        DestinationRotation = destinationRotation;
        Velocity = Vector3.zero;
    }

    public readonly GameObject GameObject;
    public readonly Quaternion DestinationRotation;
    public Vector3 Velocity;
    public Vector3 RandomDirection;
}