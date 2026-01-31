using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Main : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void BackToShip()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitGameEXE()
    { print("Quitting Game..."); Application.Quit(); }
}
