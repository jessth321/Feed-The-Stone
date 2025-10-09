using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeodeParticles : MonoBehaviour
{
    public List<float> rotationSpeeds = new List<float>(); 
    public List<Vector3> rotationAxis = new List<Vector3>();

    List<Transform> particles = new List<Transform>(); 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for ( int i = 0; i < transform.childCount; i++)
        {
            particles.Add(transform.GetChild(i));
        }
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            particles[i].Rotate(Vector3.Project(rotationAxis[i], Camera.main.transform.forward).normalized, rotationSpeeds[i] * Time.deltaTime);
        }
    }
}
