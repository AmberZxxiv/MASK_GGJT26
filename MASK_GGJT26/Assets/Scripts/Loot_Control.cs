using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loot_Control : MonoBehaviour
{// script en EMPTY COLECT_LIST
 // SINGLETON script
    public static Loot_Control instance;
    public Player_Control _PC; //pillo SINGLE del PC

    public GameObject[] collectPrefab;
    public Sprite[] collectSprites;
    public Image[] uiSlots;
    public Image[] shipNotis;
    public bool listed = false;
    public Sprite collectedItem;
    public int collectedCount;
    public bool allCollected = false;
    public Transform[] lootSpawns;
    public int itemsCounter;

    private List<int> selectedIndexes = new List<int>(); // índices de los objetos seleccionados
    private List<GameObject> spawnedItems = new List<GameObject>();


    void Awake()// singleton sin superponer
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }
    void Start()
    {
        _PC = Player_Control.instance;
    }
    void Update()
    {
        if (listed && !allCollected)
        {
            CheckVictory();
        }
    }

    public void SelectRandomItems()
    {
        selectedIndexes.Clear();
        List<int> used = new List<int>();

        while (selectedIndexes.Count < itemsCounter)
        {
            int rand = Random.Range(0, collectPrefab.Length);
            if (!used.Contains(rand))
            {
                selectedIndexes.Add(rand);
                used.Add(rand);
            }
        }
        listed = true;
        UpdateShip();
        UpdateUI();
        SpawnItems();
    }
    void UpdateShip()
    {
        for (int i = 0; i < shipNotis.Length; i++)
        {
            if (i < selectedIndexes.Count)
            {
                shipNotis[i].sprite = collectSprites[selectedIndexes[i]];
                shipNotis[i].color = Color.white; // visible
                shipNotis[i].rectTransform.sizeDelta = new Vector2(150, 150);
            }
            else shipNotis[i].gameObject.SetActive(false);
        }
    }
    void UpdateUI()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < selectedIndexes.Count)
            {
                uiSlots[i].sprite = collectSprites[selectedIndexes[i]];
                uiSlots[i].color = Color.white; // visible
                uiSlots[i].rectTransform.sizeDelta = new Vector2(150, 150);
            }
            else uiSlots[i].gameObject.SetActive(false);
        }
    }

    void SpawnItems()
    {
        List<int> usedSpawns = new List<int>();

        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            int spawnIndex;
            do spawnIndex = Random.Range(0, lootSpawns.Length);
            while (usedSpawns.Contains(spawnIndex));

            usedSpawns.Add(spawnIndex);

            GameObject spawned = Instantiate(collectPrefab[selectedIndexes[i]], lootSpawns[spawnIndex].position, Quaternion.identity);
            Loot_Item lootItem = spawned.AddComponent<Loot_Item>();
            lootItem.index = i; // índice para la UI
            spawnedItems.Add(spawned);
        }
    }
    public void CollectItem(int index, GameObject item)
    {
        if (index >= 0 && index < uiSlots.Length)
        {
            uiSlots[index].sprite = collectedItem;
            collectedCount++;
            spawnedItems.Remove(item);
            Destroy(item);
        }
    }
    void CheckVictory()
    {
        if (collectedCount >= itemsCounter)
        {
            allCollected = true;
        }
    }
}
