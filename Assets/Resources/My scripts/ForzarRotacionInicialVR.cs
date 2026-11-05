using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ForzarRotacionInicialVR : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform direccionPorDefecto;

    IEnumerator Start()
    {
        if (xrOrigin == null || xrOrigin.Camera == null || direccionPorDefecto == null) yield break;

        // El pose del headset tarda 1-2 frames en llegar tras cargar la escena;
        // si se corrige antes, el ángulo se calcula con una pose vieja y queda mal.
        yield return null;
        yield return null;

        AlinearRotacion();
    }

    private void AlinearRotacion()
    {
        Vector3 up = xrOrigin.transform.up;

        Vector3 forwardActual = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, up).normalized;
        Vector3 forwardDeseado = Vector3.ProjectOnPlane(direccionPorDefecto.forward, up).normalized;

        float angulo = Vector3.SignedAngle(forwardActual, forwardDeseado, up);
        xrOrigin.RotateAroundCameraUsingOriginUp(angulo);
    }
}
