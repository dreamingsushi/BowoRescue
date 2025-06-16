using System.Collections;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        StartCoroutine(WaitForCamera());
    }

    private IEnumerator WaitForCamera()
    {
        while (Camera.main == null)
            yield return null;

        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam == null) return;
        transform.forward = mainCam.transform.forward;
    }
}
