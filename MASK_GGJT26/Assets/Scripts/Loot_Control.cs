using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loot_Control : MonoBehaviour
{// script en EMPTY COLECT_LIST
 // SINGLETON script
    public static Loot_Control instance;

    public GameObject[] collectPrefab;
    public Sprite[] collectSprites;
    public Image[] uiSlots;
    public Sprite collectedItem;

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
        SelectRandomItems();
        UpdateUI();
        SpawnItems();
    }

    void SelectRandomItems()
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
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }

    void SpawnItems()
    {
        List<int> usedSpawns = new List<int>();

        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            int spawnIndex;
            do
            {
                spawnIndex = Random.Range(0, lootSpawns.Length);
            } while (usedSpawns.Contains(spawnIndex));

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
            spawnedItems.Remove(item);
            Destroy(item);
        }
    }
}
