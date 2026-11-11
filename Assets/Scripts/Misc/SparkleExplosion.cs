using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkleExplosion : MonoBehaviour
{
    public float lifetime;

    public float innerSpeed;
    public float outerSpeed;

    public AnimationCurve sizeOverLifetimeInner;
    public AnimationCurve sizeOverLifetimeOuter;
    public AnimationCurve speedOverLifetime;
    public AnimationCurve transparencyOverLifetime;

    [HideInInspector] public Color sparkleColor = Color.white;
    bool hasSetColor = false;
    //public float timeOffsetRange;
    //public AnimationCurve sizeTimingOffset;

    float timeAlive = 0;

    List<Transform> sparkles = new List<Transform>();
    List<Vector3> rotationAxis = new List<Vector3>();
    List<Vector3> initialScales = new List<Vector3>();

    private void OnEnable()
    {

        for (int i = 0; i < transform.childCount - 1; i++)
        {
            sparkles.Add(transform.GetChild(i));
            rotationAxis.Add(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            initialScales.Add(sparkles[i].localScale);


            sparkles[i].localScale = Vector3.zero;
        }
        Destroy(gameObject, lifetime + 0.1f);


    }

    private void Update()
    {
        if (hasSetColor)
        {
            for (int i = 0; i < sparkles.Count; i++)
            {

                float offsetTimeAlive = timeAlive;// - sizeTimingOffset.Evaluate(Mathf.Lerp(0, 1, (float)i / sparkles.Count)) * timeOffsetRange;

                if (i  < 2)
                {
                    sparkles[i].Rotate(Vector3.Project(rotationAxis[i], Camera.main.transform.forward).normalized,
                        Mathf.Lerp(innerSpeed * speedOverLifetime.Evaluate(offsetTimeAlive / lifetime), outerSpeed * speedOverLifetime.Evaluate(offsetTimeAlive / lifetime), (float)i / sparkles.Count) * Time.deltaTime);
                    sparkles[i].localScale = Vector3.Lerp(Vector3.zero, initialScales[i], sizeOverLifetimeInner.Evaluate(offsetTimeAlive / lifetime));
                }
                else
                {
                    sparkles[i].Rotate(Vector3.Project(rotationAxis[i], Camera.main.transform.forward).normalized,
                        Mathf.Lerp(innerSpeed * speedOverLifetime.Evaluate(offsetTimeAlive / lifetime), outerSpeed * speedOverLifetime.Evaluate(offsetTimeAlive / lifetime), (float)i / sparkles.Count) * Time.deltaTime);
                    sparkles[i].localScale = Vector3.Lerp(Vector3.zero, initialScales[i], sizeOverLifetimeOuter.Evaluate(offsetTimeAlive / lifetime));
                }


            }

            timeAlive += Time.deltaTime;
        }
        else
        {

            for (int i = 0; i < sparkles.Count; i++)
            {
                int renderQueueOffset = 0;
                float intensity = 45;

                if (i == 0)
                {
                    renderQueueOffset = 1;
                    intensity = 25;
                }
                else if (i == 1)
                {
                    renderQueueOffset = 2;
                    intensity = 10;
                }

                MeshRenderer mr = sparkles[i].GetComponent<MeshRenderer>();
                mr.material.renderQueue = 2000 + renderQueueOffset;
                mr.material.SetColor("_Color", sparkleColor);
                mr.material.SetFloat("_Intensity", intensity);

            }
            transform.GetComponentInChildren<Light>().color = sparkleColor;
            hasSetColor = true;
        }
        
    }
}
