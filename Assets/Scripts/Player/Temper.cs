using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Temper : MonoBehaviour
{
    public Transform temperContainer;

    PlayerController pc;
    PlayerInventory pi;

    List<Image> fullImages = new List<Image>();
    List<Image> fillingImages = new List<Image>();
    List<Image> fillingOuterImages = new List<Image>();
    List<Image> fillingGlowImages = new List<Image>();
    List<Image> shutImages = new List<Image>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = FindAnyObjectByType<PlayerController>();
        pi = FindAnyObjectByType<PlayerInventory>();
        SetUpLists();
    }

    public void SetUpLists()
    {
        for (int i = 0; i < temperContainer.childCount; i++)
        {
            fullImages.Add(temperContainer.GetChild(i).GetChild(4).GetComponent<Image>());
            fillingImages.Add(temperContainer.GetChild(i).GetChild(2).GetComponent<Image>());
            fillingOuterImages.Add(temperContainer.GetChild(i).GetChild(3).GetComponent<Image>());
            fillingGlowImages.Add(temperContainer.GetChild(i).GetChild(1).GetComponent<Image>());
            shutImages.Add(temperContainer.GetChild(i).GetChild(0).GetComponent<Image>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < fullImages.Count; i++)
        {
            fullImages[i].enabled = false;
            fillingImages[i].enabled = false;
            fillingOuterImages[i].enabled = false;
            fillingGlowImages[i].enabled = false;
            shutImages[i].enabled = false;

            float temper = pc.temper - i;

            if (Mathf.Approximately(temper, 1f) || temper > 1f)
            {
                fullImages[i].enabled = true;
            }
            else if (temper > 0f)
            {
                fillingImages[i].enabled = true;
                fillingOuterImages[i].enabled = true;
                fillingGlowImages[i].enabled = true;

                fillingImages[i].fillAmount = temper;
            }
            else
            {
                shutImages[i].enabled = true;
            }
        }
    }
}
