using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using XInputDotNetPure;

public class Placement : MonoBehaviour 
{
    const float EasingTime = 0.1f;

    enum PlacingPhase
    {
        ChoosingPosition,
        ChoosingDirection
    }

    public GameObject WireTemplate;

    GameObject[] Lines;
    GameObject Anchor;
    GameObject HighlightFace;

    readonly List<GameObject> AllPlacedProjectors = new List<GameObject>();

    float LastHeight;
    float ActualHeight;
    int DestinationHeight;
    float EasingTimeLeft;
    float SinceBeatMatch;
    float SinceLongPress;
    Vector3 FaceNormal = Vector3.forward;

    IKeyboard Keyboard;
    IMouse Mouse;
    IGamepads Gamepads;

    PlacingPhase Phase = PlacingPhase.ChoosingPosition;

    void Start()
    {
        Pico.LevelChanged += Initialize;
        Initialize();

        Anchor = Instantiate(Pico.ProjectorTemplate) as GameObject;
        Anchor.name = "Anchor";
        Anchor.GetComponent<ProjectorBehaviour>().IsGizmo = true;
        Anchor.FindChild("Inner").active = false;
        Anchor.FindChild("Pyramid").active = false;
        HighlightFace = Anchor.FindChild("Highlight Face");
        HighlightFace.GetComponentInChildren<Renderer>().enabled = false;

        Keyboard = KeyboardManager.Instance;
        Keyboard.RegisterKey(KeyCode.W);
        Keyboard.RegisterKey(KeyCode.S);
        Keyboard.RegisterKey(KeyCode.R);
        Keyboard.RegisterKey(KeyCode.Z);
        Keyboard.RegisterKey(KeyCode.Space);
        Keyboard.RegisterKey(KeyCode.LeftControl);

        Anchor.transform.position = new Vector3(0, 0, 0);

        Mouse = MouseManager.Instance;
        Gamepads = GamepadsManager.Instance;
    }

    void Initialize()
    {
        DestinationHeight = 0;

        if (Lines != null)
            foreach (var line in Lines)
                Destroy(line);

        Lines = new GameObject[(Pico.Level.Size + 1) * 2];

        for (int i = 0; i < (Pico.Level.Size + 1) * 2; i++)
        {
            Lines[i] = Instantiate(WireTemplate) as GameObject;
            Lines[i].transform.localScale = new Vector3(0.025f, 0.025f, Pico.Level.Size);
        }

        AllPlacedProjectors.Clear();
    }

	void Update() 
    {
        var anyGamepad = Gamepads.Any;

        // Beat match
	    SinceBeatMatch += Time.deltaTime;
        if (anyGamepad.X.State == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.Space) == ComplexButtonState.Pressed)
        {
            if (SinceBeatMatch < 2)
            {
                Pico.HeartbeatDuration = SinceBeatMatch;
                Pico.TimeLeftToHeartbeat = SinceBeatMatch;
            }
            SinceBeatMatch = 0;
        }

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        var basePlane = new Plane(Vector3.up, new Vector3(0, ActualHeight - 0.5f, 0));
        float distance;

        switch (Phase)
        {
            case PlacingPhase.ChoosingPosition:
                if (!anyGamepad.Connected)
                {
                    if (Mouse.RightButton.State == MouseButtonState.Idle &&
                        Keyboard.GetKeyState(KeyCode.LeftControl) == ComplexButtonState.Up &&
                        basePlane.Raycast(mouseRay, out distance))
                    {
                        Vector3 p = (Camera.main.transform.position + mouseRay.direction * distance).Round();
                        var hs = Pico.Level.Size / 2;

                        var clampedPosition = p.Clamp(-Pico.Level.Size / 2, Pico.Level.Size / 2 - 1);
                        if (Camera.main.transform.position.y < ActualHeight - 0.5f)
                            clampedPosition.y++;

                        if (Pico.Level.GetAccumulatorAt(clampedPosition) == null &&
                            Pico.Level.GetEmitterAt(clampedPosition) == null &&
                            Pico.Level.GetProjectorAt(clampedPosition) == null &&
                            Pico.Level.GetReceiverAt(clampedPosition) == null)
                        {
                            Anchor.transform.position = clampedPosition;

                            if (p.x < -hs || p.x >= hs || p.y < -hs || p.y >= hs || p.z < -hs || p.z >= hs)
                                ; // Do nothing
                            else if (Mouse.LeftButton.State == MouseButtonState.Clicked && !Pico.IsChangingLevel)
                                Phase = PlacingPhase.ChoosingDirection;
                        }
                    }
                }
                else
                {
                    var mat = Camera.main.camera.worldToCameraMatrix;

                    var right = mat.GetRow(0).ToVector3().MaxClampXZ();
                    Vector3 up;
                    var camUp = mat.GetRow(1).ToVector3();
                    var camForward = mat.GetRow(2).ToVector3();

                    if (Math.Abs(camUp.x) > Math.Abs(camUp.z)) camUp = new Vector3(camUp.x, 0, 0); else camUp = new Vector3(0, 0, camUp.z);
                    if (Math.Abs(camForward.x) > Math.Abs(camForward.z)) camForward = new Vector3(camForward.x, 0, 0); else camForward = new Vector3(0, 0, camForward.z);
                    if (Math.Abs(camUp.x + camUp.z) > Math.Abs(camForward.x + camForward.z)) up = camUp.Sign(); else up = -camForward.Sign();

                    if (anyGamepad.LeftStick.Right.State == ComplexButtonState.Pressed || anyGamepad.DPad.Right.State == ComplexButtonState.Pressed) Anchor.transform.position += right;
                    if (anyGamepad.LeftStick.Left.State == ComplexButtonState.Pressed || anyGamepad.DPad.Left.State == ComplexButtonState.Pressed) Anchor.transform.position -= right;
                    if (anyGamepad.LeftStick.Up.State == ComplexButtonState.Pressed || anyGamepad.DPad.Up.State == ComplexButtonState.Pressed) Anchor.transform.position += up;
                    if (anyGamepad.LeftStick.Down.State == ComplexButtonState.Pressed || anyGamepad.DPad.Down.State == ComplexButtonState.Pressed) Anchor.transform.position -= up;

                    if (anyGamepad.LeftStick.Right.TimePressed > 0.3 || anyGamepad.DPad.Right.TimePressed > 0.3) { SinceLongPress += Time.deltaTime; if (SinceLongPress > 0.05) { Anchor.transform.position += right; SinceLongPress = 0; } }
                    if (anyGamepad.LeftStick.Left.TimePressed > 0.3 || anyGamepad.DPad.Left.TimePressed > 0.3) { SinceLongPress += Time.deltaTime; if (SinceLongPress > 0.05) { Anchor.transform.position -= right; SinceLongPress = 0; } }
                    if (anyGamepad.LeftStick.Up.TimePressed > 0.3 || anyGamepad.DPad.Up.TimePressed > 0.3) { SinceLongPress += Time.deltaTime; if (SinceLongPress > 0.05) { Anchor.transform.position += up; SinceLongPress = 0; } }
                    if (anyGamepad.LeftStick.Down.TimePressed > 0.3 || anyGamepad.DPad.Down.TimePressed > 0.3) { SinceLongPress += Time.deltaTime; if (SinceLongPress > 0.05) { Anchor.transform.position -= up; SinceLongPress = 0; } }

                    Anchor.transform.position = Anchor.transform.position.Clamp(-Pico.Level.Size / 2, Pico.Level.Size / 2 - 1);
                    
                    var worldPos = Anchor.transform.position;
                    if (Pico.Level.GetAccumulatorAt(worldPos) == null &&
                        Pico.Level.GetEmitterAt(worldPos) == null &&
                        Pico.Level.GetProjectorAt(worldPos) == null &&
                        Pico.Level.GetReceiverAt(worldPos) == null &&
                        anyGamepad.A.State == ComplexButtonState.Pressed && !Pico.IsChangingLevel)
                    {
                        FaceNormal = -mat.GetRow(2).ToVector3().MaxClamp();
                        Phase = PlacingPhase.ChoosingDirection;
                    }
                }

                // Move up/down one layer
                if ((anyGamepad.RightShoulder.State == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.W) == ComplexButtonState.Pressed) && DestinationHeight < Pico.Level.Size / 2 - 1)
                {
                    LastHeight = DestinationHeight;
                    DestinationHeight++;
                    EasingTimeLeft = 0;
                }
                if ((anyGamepad.LeftShoulder.State == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.S) == ComplexButtonState.Pressed) && DestinationHeight > -Pico.Level.Size / 2)
                {
                    LastHeight = DestinationHeight;
                    DestinationHeight--;
                    EasingTimeLeft = 0;
                }

                if (!Pico.IsChangingLevel)
                {
                    // Undo last placement
                    if (AllPlacedProjectors.Count > 0 && (anyGamepad.B.State == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.Z) == ComplexButtonState.Pressed))
                    {
                        var anchorAt = Pico.Level.GetProjectorAt(Anchor.transform.position);
                        if (anchorAt != null)
                        {
                            Destroy(anchorAt);
                            AllPlacedProjectors.Remove(anchorAt);
                        }
                        else
                        {
                            Destroy(AllPlacedProjectors[AllPlacedProjectors.Count - 1]);
                            AllPlacedProjectors.RemoveAt(AllPlacedProjectors.Count - 1);
                        }

                        Pico.Level.PlacedCount--;
                    }

                    // Undo ALL placements
                    if (anyGamepad.Back == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.R) == ComplexButtonState.Pressed)
                    {
                        Pico.Level.PlacedCount = 0;
                        Pico.RebootLevel();
                    }
                }
                break;

            case PlacingPhase.ChoosingDirection:
                if (!Pico.IsChangingLevel)
                {
                    // Undo last placement
                    if (anyGamepad.B.State == ComplexButtonState.Pressed || Keyboard.GetKeyState(KeyCode.Z) == ComplexButtonState.Pressed)
                    {
                        HighlightFace.GetComponentInChildren<Renderer>().enabled = false;
                        Phase = PlacingPhase.ChoosingPosition;
                        break;
                    }
                }

                if (!anyGamepad.Connected)
                {
                    RaycastHit hitInfo;
                    if (Anchor.collider.Raycast(mouseRay, out hitInfo, float.MaxValue))
                    {
                        HighlightFace.GetComponentInChildren<Transform>().LookAt(HighlightFace.transform.position +
                                                                                 hitInfo.normal);
                        HighlightFace.transform.position = Anchor.transform.position + hitInfo.normal * 0.5f;
                        HighlightFace.GetComponentInChildren<Renderer>().enabled = true;

                        if (Mouse.LeftButton.State == MouseButtonState.Clicked && !Pico.IsChangingLevel)
                        {
                            Phase = PlacingPhase.ChoosingPosition;
                            HighlightFace.GetComponentInChildren<Renderer>().enabled = false;

                            var cellPosition = (Anchor.transform.position + VectorEx.New(Pico.Level.Size / 2)).Round();
                            //Debug.Log("Adding projector @ " + cellPosition);

                            AllPlacedProjectors.Add(Pico.Level.AddProjectorAt(
                                (int) cellPosition.x, (int) cellPosition.y, (int) cellPosition.z,
                                DirectionEx.FromVector(hitInfo.normal)));
                            Pico.Level.PlacedCount++;
                        }
                    }
                    else
                    {
                        HighlightFace.GetComponentInChildren<Renderer>().enabled = false;

                        if (Mouse.LeftButton.State == MouseButtonState.Clicked && !Pico.IsChangingLevel)
                        {
                            if (basePlane.Raycast(mouseRay, out distance))
                            {
                                Vector3 p = (Camera.main.transform.position + mouseRay.direction * distance).Round();
                                var hs = Pico.Level.Size / 2;
                                if (p.x < -hs || p.x >= hs || p.y < -hs || p.y >= hs || p.z < -hs || p.z >= hs)
                                    Phase = PlacingPhase.ChoosingPosition;
                            }
                            else
                                Phase = PlacingPhase.ChoosingPosition;
                        }
                    }
                }
                else
                {
                    var mat = Camera.main.camera.worldToCameraMatrix;
                    var right = mat.GetRow(0).ToVector3().MaxClamp();
                    var up = -mat.GetRow(2).ToVector3().MaxClamp();

                    if (anyGamepad.LeftStick.Position.sqrMagnitude > 0.5)
                        FaceNormal = (right * anyGamepad.LeftStick.Position.x + up * anyGamepad.LeftStick.Position.y).MaxClamp();

                    if (anyGamepad.DPad.Direction.sqrMagnitude > 0.5)
                        FaceNormal = (right * anyGamepad.DPad.Direction.x + up * anyGamepad.DPad.Direction.y).MaxClamp();

                    HighlightFace.GetComponentInChildren<Transform>().LookAt(HighlightFace.transform.position + FaceNormal);
                    HighlightFace.transform.position = Anchor.transform.position + FaceNormal * 0.479035f;
                    HighlightFace.GetComponentInChildren<Renderer>().enabled = true;

                    if (anyGamepad.A.State == ComplexButtonState.Pressed && !Pico.IsChangingLevel)
                    {
                        Phase = PlacingPhase.ChoosingPosition;
                        HighlightFace.GetComponentInChildren<Renderer>().enabled = false;

                        var cellPosition = (Anchor.transform.position + VectorEx.New(Pico.Level.Size / 2)).Round();
                        Debug.Log("Adding projector for " + FaceNormal);

                        AllPlacedProjectors.Add(Pico.Level.AddProjectorAt(
                            (int)cellPosition.x, (int)cellPosition.y, (int)cellPosition.z,
                            DirectionEx.FromVector(FaceNormal)));
                        Pico.Level.PlacedCount++;
                    }
                }
                break;
        }

        EasingTimeLeft += Time.deltaTime;
        float step = Mathf.Clamp01(EasingTimeLeft / EasingTime);
        Pico.GridHeight = ActualHeight = Mathf.Lerp(LastHeight, DestinationHeight, Easing.EaseIn(step, EasingType.Quartic));
	    Anchor.transform.position = new Vector3(Anchor.transform.position.x, DestinationHeight, Anchor.transform.position.z);

        for (int i = 0; i <= Pico.Level.Size; i++)
        {
            Lines[i].transform.position = new Vector3(i - Pico.Level.Size / 2 - 0.5f, ActualHeight - 0.5f, -0.5f);
        }
        for (int i = 0; i <= Pico.Level.Size; i++)
        {
            Lines[i + (Pico.Level.Size + 1)].transform.position = new Vector3(-0.5f, ActualHeight - 0.5f, i - Pico.Level.Size / 2 - 0.5f);
            Lines[i + (Pico.Level.Size + 1)].transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
        }
    }
}
