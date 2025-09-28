using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class StickyBomb : MonoBehaviour
{
    public GameObject explosion;

    public float timer;

    MarchingCubes mc;
    Rigidbody rb;
    Collider col;
    Animator anim;
    MeshRenderer mr;

    [Header("")]
    [Range(0f, 500f)]
    public float deformationRadius;
    [Range(0f, 1f)]
    public float deformationBlend;
    [Range(0f, 1f)]
    public float deformationFrequency;
    [Range(0f, 1f)]
    public float deformationAmp;
    [Range(0, 12)]
    public int deformationOct;
    [Range(0f, 20f)]
    public float deformationLac;
    [Range(0f, 1f)]
    public float deformationPers;

    private void Start()
    {
        mc = FindFirstObjectByType<MarchingCubes>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        anim = GetComponent<Animator>();
        mr = GetComponent<MeshRenderer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            anim.SetTrigger("startExplosion");
            DeformTerrain();

            col.enabled = false;
        }
    }

    public async void DeformTerrain()
    {
        if (!Application.isPlaying) return;
        mc.DeformChunks(new Deformation(transform.position, deformationRadius, deformationBlend, deformationFrequency, deformationAmp, (float)deformationOct, deformationLac, deformationPers), timer * ((deformationRadius * 2) / mc.chunkSize));

        await Task.Delay((int)(timer * 1000 * ((deformationRadius * 2) / mc.chunkSize)));
        if (!Application.isPlaying) return;

        GameObject explo = Instantiate(explosion, transform.position, Quaternion.identity);
        explo.transform.localScale = Vector3.one * deformationRadius * 0.32f;
        mr.enabled = false;

        await Task.Delay(2000);
        if (!Application.isPlaying) return;

        Destroy(explo);
        Destroy(gameObject);
    }
}
