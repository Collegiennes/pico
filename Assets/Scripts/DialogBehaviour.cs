using System;
using UnityEngine;

class DialogBehaviour : MonoBehaviour
{
    float SinceLevelStarted;

    void Start()
    {
        Pico.LevelChanged += OnLevelChanged;
        OnLevelChanged();
    }
    void OnLevelChanged()
    {
        guiText.text = Pico.Level.DialogText;
        SinceLevelStarted = 0;
    }

    void Update()
    {
        SinceLevelStarted += Time.deltaTime;

        if (SinceLevelStarted < 1)
        {
            var step = Easing.EaseIn(SinceLevelStarted / 1, EasingType.Cubic);

            var c = guiText.material.color;
            guiText.material.color = new Color(c.r, c.g, c.b, step);
        }
        else if (SinceLevelStarted > 15)
        {
            var step = Easing.EaseOut((SinceLevelStarted - 15) / 3, EasingType.Cubic);

            var c = guiText.material.color;
            guiText.material.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1 - step));
        }
        else
        {
            var c = guiText.material.color;
            guiText.material.color = new Color(c.r, c.g, c.b, 1);
        }
    }
}
