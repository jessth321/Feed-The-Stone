using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfAfterTime : MonoBehaviour
{
    public float time;
    public AnimationCurve brightnessFalloff;

    float timeElapsed;
    float initialIntensity;
    new Light light;

    void Start()
    {
        light = GetComponent<Light>();
        Destroy(gameObject, time);

        timeElapsed = 0;
        initialIntensity = light.intensity;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        light.intensity = brightnessFalloff.Evaluate(Mathf.Lerp(0, 1, timeElapsed / time)) * initialIntensity;
    }
}
