using UnityEngine;

public class Interactable : MonoBehaviour
{
    public delegate void OnInteractDelegate();

    public OnInteractDelegate onInteract;

    public GameObject interactTextCanvas;

    private void OnEnable()
    {
        interactTextCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void OnInteract()
    {
        onInteract.Invoke();
    }
}
