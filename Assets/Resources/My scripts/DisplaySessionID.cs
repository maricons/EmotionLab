using UnityEngine;
using TMPro;

public class DisplaySessionID : MonoBehaviour
{
    public TMP_Text idTextComponent;
    public EmotionDataManager dataManager;

    private void Start()
    {
        if (dataManager == null)
            dataManager = EmotionDataManager.Instance != null ? EmotionDataManager.Instance : FindObjectOfType<EmotionDataManager>();
    }

    private void Update()
    {
        if (idTextComponent == null) return;

        if (dataManager == null)
            dataManager = EmotionDataManager.Instance != null ? EmotionDataManager.Instance : FindObjectOfType<EmotionDataManager>();

        if (dataManager != null)
        {
            // Si no hay sesión o el ID está vacío, forzamos la creación de un ID
            if (dataManager.sesionActual == null || string.IsNullOrEmpty(dataManager.sesionActual.idParticipante))
            {
                dataManager.IniciarSesion(null);
            }

            // Mostramos el ID generado
            if (dataManager.sesionActual != null && !string.IsNullOrEmpty(dataManager.sesionActual.idParticipante))
            {
                idTextComponent.text = $"ID: {dataManager.sesionActual.idParticipante}";
            }
        }
    }
}