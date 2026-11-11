using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeLight : MonoBehaviour
{
    public float time;
    float timer;
    float initialIntensity;
    public AnimationCurve fadeCurve;

    Light _light;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        initialIntensity = _light.intensity;

        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {


        if (timer < time)
        {
            _light.intensity = fadeCurve.Evaluate((timer + Time.deltaTime) / time) * initialIntensity;
        }


        timer += Time.deltaTime;
    }
}
