using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCard : MonoBehaviour
{
    public CardData cardData;

    public Vector3 originalLocalPos;

    public int indexInHand;
    public int indexInSelected;

    bool isSelected = false;

    DeckManager dm;

    private void Awake()
    {
        dm = transform.parent.GetComponent<DeckManager>();
    }

    public void SelectCard()
    {

        if (!isSelected)
        {
            isSelected = true;
            dm.SelectCard(indexInHand);
        }
        else
        {
            isSelected = false;
            dm.UnselectCard(indexInHand, indexInSelected);
        }
    }
}
