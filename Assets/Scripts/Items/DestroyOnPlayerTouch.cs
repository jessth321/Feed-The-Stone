using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnPlayerTouch : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
