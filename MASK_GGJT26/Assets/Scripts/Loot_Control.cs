using UnityEngine;

public class Loot_Control : MonoBehaviour
{// script en EMPTY COLECT_LIST
 // SINGLETON script
    public static Loot_Control instance;

    public GameObject[] collectables;

    void Awake()// singleton sin superponer
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
