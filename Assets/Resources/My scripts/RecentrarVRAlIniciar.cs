using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class RecentrarVRAlIniciar : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;

    IEnumerator Start()
    {
        if (xrOrigin == null) yield break;

        // El pose de la cámara tarda 1-2 frames en llegar tras cargar la escena;
        // TryRecenter() no funciona con OpenXR en Quest, por eso se realinea manualmente.
        yield return null;
        yield return null;

        xrOrigin.MatchOriginUpCameraForward(xrOrigin.transform.up, xrOrigin.transform.forward);
    }
}
