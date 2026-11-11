using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;

public struct CardData
{
    public int suit;
    public int value;


    public CardData(int _suit, int _value)
    {
        suit = _suit;
        value = _value;
    }

}

public class DeckManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardSpawn;
    public Transform cardHandTransform;

    

    public float cardHandOffset;

    public bool areCardsDrawn;

    List<CardData> deck = new List<CardData>();
    List<CardData> hand = new List<CardData>();
    List<CardData> discards = new List<CardData>();

    List<StoneCard> handStoneCards = new List<StoneCard>();
    List<StoneCard> selectedStoneCards = new List<StoneCard>();

    List<float> cardLerps = new List<float>();
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeDeck();
        //DrawCards(8);
    }

    // Update is called once per frame
    void Update()
    {
        if (areCardsDrawn)
        {
            for (int i = 0; i < selectedStoneCards.Count; i++)
            {

                if (cardLerps[selectedStoneCards[i].indexInHand] < 1f)
                {
                    cardLerps[selectedStoneCards[i].indexInHand] += Time.deltaTime * 15;
                }
                else
                {
                    cardLerps[selectedStoneCards[i].indexInHand] = 1f;
                }
                selectedStoneCards[i].transform.localPosition = Vector3.Lerp(selectedStoneCards[i].originalLocalPos, selectedStoneCards[i].originalLocalPos + Vector3.up * 0.02f, cardLerps[selectedStoneCards[i].indexInHand]);
            }

            for (int i = 0; i < handStoneCards.Count; i++)
            {
                if (handStoneCards[i] != null)
                {
                    if (cardLerps[i] > 0f)
                    {
                        cardLerps[i] -= Time.deltaTime * 15;
                    }
                    else
                    {
                        cardLerps[i] = 0f;
                    }
                    handStoneCards[i].transform.localPosition = Vector3.Lerp(handStoneCards[i].originalLocalPos, handStoneCards[i].originalLocalPos + Vector3.up * 0.02f, cardLerps[i]);
                }

            }
        }
        


    }



    void InitializeDeck()
    {
        areCardsDrawn = false;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                deck.Add(new CardData(i, j));
            }
        }
    }

    async public void DrawCards(int num)
    {
        areCardsDrawn = false;
        if (handStoneCards.Count > 0)
        {
            for (int i = 0; i < handStoneCards.Count; i++)
            {
                Destroy(handStoneCards[i].gameObject);
            }

            hand = new List<CardData>();
            handStoneCards = new List<StoneCard>();
        }

        cardLerps = new List<float>();

        for (int i = 0; i < num; i++)
        {
            if (deck.Count == 0)
            {
                deck = discards;
                discards = new List<CardData>();
            }

            int index = Random.Range(0, deck.Count);
            hand.Add(deck[index]);
            discards.Add(deck[index]);

            cardLerps.Add(0f);

            deck.RemoveAt(index);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            handStoneCards.Add(Instantiate(cardPrefab, cardSpawn.position, Quaternion.identity, transform).GetComponent<StoneCard>());
            handStoneCards[i].cardData = hand[i];
            handStoneCards[i].indexInHand = i;
            handStoneCards[i].gameObject.SetActive(false);

            Material cardMat = handStoneCards[i].GetComponent<MeshRenderer>().material;


            switch (hand[i].suit)
            {
                case 0:
                    cardMat.SetColor("_Color", Color.magenta);
                    break;
                case 1:
                    cardMat.SetColor("_Color", Color.green);
                    break;
                case 2:
                    cardMat.SetColor("_Color", Color.yellow * 0.5f + Color.red * 0.5f);
                    break;
                case 3:
                    cardMat.SetColor("_Color", Color.cyan);
                    break;
            }

            cardMat.SetFloat("_Index", hand[i].suit * 10 + hand[i].value);

            

        }

        await Task.Yield();

        for (int i = 0; i < hand.Count; i++)
        {


            handStoneCards[i].gameObject.SetActive(true);

            Vector3 targetPos = cardHandTransform.position + cardHandTransform.right * (hand.Count * 0.5f - (hand.Count - i)) * cardHandOffset;
            float lerp = 0f;

            while (!Mathf.Approximately(lerp, 1f))
            {
                targetPos = cardHandTransform.position + cardHandTransform.right * (hand.Count * 0.5f - (hand.Count - i)) * cardHandOffset;
                lerp += Time.deltaTime * 10f;
                if (lerp > 1f || Mathf.Approximately(lerp, 1f))
                {
                    lerp = 1f;
                }

                handStoneCards[i].transform.position = Vector3.Lerp(cardSpawn.position, targetPos, lerp);
                handStoneCards[i].transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(-90, 0, 0), lerp);


                await Task.Yield();
            }

            handStoneCards[i].transform.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        }

        for (int i = 0; i < handStoneCards.Count; i++)
        {
            handStoneCards[i].originalLocalPos = handStoneCards[i].transform.localPosition;
        }

        areCardsDrawn = true;
    }

    public void SelectCard(int index)
    {
        selectedStoneCards.Add(handStoneCards[index]);

        selectedStoneCards[selectedStoneCards.Count - 1].indexInSelected = selectedStoneCards.Count - 1;

        handStoneCards[index] = null;
    }

    public void UnselectCard(int index, int selectedIndex)
    {
        handStoneCards[index] = selectedStoneCards[selectedIndex];

        for (int i = selectedIndex; i < selectedStoneCards.Count; i++)
        {
            selectedStoneCards[i].indexInSelected -= 1;

        }

        selectedStoneCards.RemoveAt(selectedIndex);



    }

}
