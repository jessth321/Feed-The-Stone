using UnityEngine;
using System.Threading.Tasks;

public class HeavyBlast : MonoBehaviour
{
    float lerp = 0;

    public AnimationCurve sizeCurve;
    public AnimationCurve distCurve;

    Vector3 originalPosition;
    Vector3 originalScale;
    Vector3 originalForward;

    private void OnEnable()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalForward = transform.forward;
        DoThing();
    }

    async void DoThing()
    {
        while (lerp <= 1)
        {
            transform.position = originalPosition + originalForward * distCurve.Evaluate(lerp) * 10f;

            transform.localScale = new Vector3(originalScale.x * sizeCurve.Evaluate(lerp), originalScale.y, originalScale.z * sizeCurve.Evaluate(lerp));


            lerp += Time.deltaTime * 3f;

            await Task.Yield();
        }

        Destroy(gameObject);
    }
}
