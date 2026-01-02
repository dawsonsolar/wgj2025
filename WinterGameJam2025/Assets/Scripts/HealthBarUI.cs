using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public float visibleTime = 2f;
    public Vector3 offset = new Vector3(0, 0.6f, 0);

    Transform target;
    Coroutine hideRoutine;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void Initialize(Transform targetTransform, float current, float max)
    {
        target = targetTransform;
        SetHealth(current, max);
        Show();
    }

    public void SetHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleTime);
        gameObject.SetActive(false);
    }
}
