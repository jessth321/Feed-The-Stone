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

        timer = time;
    }

    // Update is called once per frame
    void Update()
    {


        if (timer > 0)
        {
            _light.intensity = fadeCurve.Evaluate(1 - timer / time) * initialIntensity;
        }


        timer -= Time.deltaTime;
    }
}
