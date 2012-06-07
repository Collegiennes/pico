using System;
using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit")]
class MouseOrbit : MonoBehaviour
{
    public Transform target;
    GameObject anchor;

    public float distance = 30;
    float interpolatedDistance = 30;
    Vector3 interpolatedAnchorPosition;

    public float xSpeed = 250;
    public float ySpeed = 120;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    float x, y;
    float curScore, destScore;

    IGamepads Gamepads;

    GameObject ProgressIndic;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;

        Gamepads = GamepadsManager.Instance;

        ProgressIndic = gameObject.FindChild("progressLocation");
    }

    void Update()
    {
        //Debug.Log("Pico.Level.PlacedCount " + Pico.Level.PlacedCount);

        var fairCount = Pico.Level.FairCount;
        var placed = Pico.Level.PlacedCount;

        var distToFair = placed - fairCount;

        destScore = (-distToFair) / 4f + 0.5f;
        if (destScore >= 1)
        {
            destScore = Mathf.Clamp01(destScore) - Pico.Level.PlacedCount / 16f;
        }
        else
        {
            destScore = Mathf.Clamp01(destScore);
        }

        curScore = Mathf.Lerp(curScore, destScore, 0.1f);

        ProgressIndic.transform.localPosition = new Vector3(-1.6f + 3.2f * curScore, 0.858587f, 2.669825f);
    }

    void LateUpdate()
    {
        if (anchor == null)
            anchor = GameObject.Find("Anchor");

        if (target)
        {
            if (Input.GetMouseButton(1) || (Input.GetKey("left ctrl") && Input.GetMouseButton(0)))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }

            x -= Gamepads.Any.RightStick.Position.x * xSpeed * 0.015f;
            y += Gamepads.Any.RightStick.Position.y * ySpeed * 0.015f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            var destinationRotation = Quaternion.Euler(y, x, 0);
            var rotation = Quaternion.Slerp(transform.rotation, destinationRotation, 0.1f);

            if (Gamepads.Any.Connected)
            {
                var rt = Gamepads.Any.RightTrigger.Value;
                if (rt > 0 && distance > 5)
                    distance -= rt;

                var lt = Gamepads.Any.LeftTrigger.Value;
                if (lt > 0 && distance < 30)
                    distance += lt;
            }
            interpolatedDistance = Mathf.Lerp(interpolatedDistance, distance, 0.1f);
            var zoom = (interpolatedDistance - 5) / 25;

            interpolatedAnchorPosition = Vector3.Lerp(interpolatedAnchorPosition, anchor.transform.position, 0.1f);
            var interpolatedTarget = Vector3.Lerp(interpolatedAnchorPosition, target.position, zoom);

            var position = rotation * new Vector3(0, 0, -interpolatedDistance) + interpolatedTarget;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    static float ClampAngle (float angle, float min, float max) 
    {
	    if (angle < -360)
		    angle += 360;
	    if (angle > 360)
		    angle -= 360;
	    return Mathf.Clamp (angle, min, max);
    }
}
