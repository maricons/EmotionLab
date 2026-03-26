using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void IrACierre(string Cierre)
    {
        SceneManager.LoadScene("Cierre");
    }

    public void IrAWaitingRoom(string WaitingRoom)
    {
        SceneManager.LoadScene("WaitingRoom");
    }

}