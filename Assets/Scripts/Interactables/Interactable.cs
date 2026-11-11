using UnityEngine;

public class Interactable : MonoBehaviour
{
    public delegate void OnInteractDelegate();

    public OnInteractDelegate onInteract;


    public void OnInteract()
    {
        if (onInteract != null && onInteract.GetInvocationList().Length > 0)
        {

            onInteract.Invoke();
        }
    }
}
