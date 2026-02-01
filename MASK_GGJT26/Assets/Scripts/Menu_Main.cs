using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Main : MonoBehaviour
{
    public GameObject carga1;
    public GameObject carga2;


    void Start()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        
    }
    public void next()
    {
        carga1.SetActive(false);
        carga2.SetActive(true);
    }
    public void start()
    {
        carga2.SetActive(false);
        SceneManager.LoadScene(1);   
    }
    public void Play()
    {
        carga1.SetActive(true);
    }
    public void ExitGameEXE()
    { print("Quitting Game..."); Application.Quit(); }
}
