using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int flowerCount = 0;
    public int bugCount = 0;
    public int shadowCount = 0;

    private List<int> usedPositions = new List<int>();
    private List<GameObject> UnusedFlowerPool = new List<GameObject>();

    [Header("Scene Settings")]
    public int maxFlowerCount = 7;
    public int maxBugCount = 20;
    public int minShadow = 2;
    public int maxShadow = 6;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private GameObject bugPrefab;
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private List<Transform> flowerPosition = new List<Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        List<int> selected = new List<int>();

        while (selected.Count < maxFlowerCount)
        {
            int r = Random.Range(0, flowerPosition.Count);

            if (!selected.Contains(r))
            {
                selected.Add(r);
            }
        }

        foreach (int index in selected)
        {
            Transform spawnPos = flowerPosition[index];
            GameObject flowerObj = Instantiate(flowerPrefab, spawnPos.position, Quaternion.identity);
            Flower flower = flowerObj.GetComponent<Flower>();
            flower.flowerGenerate(spawnPos.position);
            flower.positionIndex = index;
            usedPositions.Add(index);
            flowerCount++;
        }

        while (bugCount < maxBugCount)
        {
            GameObject bugObj = Instantiate(bugPrefab, Vector3.zero, Quaternion.identity);
            LightBug bug = bugObj.GetComponent<LightBug>();
            bug.generateLightBug();
            bugCount++;
        }
    }

    private void Update()
    {
        if (flowerCount < maxFlowerCount) { 
            List<int> notSelected = new List<int>();
            for (int i = 0; i < flowerPosition.Count; i++)
            {
                if (!usedPositions.Contains(i))
                {
                    notSelected.Add(i);
                }
            }

            int p = Random.Range(0, notSelected.Count);
            int posIndex = notSelected[p];
            Transform spawnPos = flowerPosition[posIndex];

            GameObject flowerObj;
            if (UnusedFlowerPool.Count > 0)
            {
                flowerObj = UnusedFlowerPool[0];
                UnusedFlowerPool.RemoveAt(0);
                flowerObj.SetActive(true);
            }
            else
            {
                flowerObj = Instantiate(flowerPrefab, spawnPos.position, Quaternion.identity);
            }

            Flower flower = flowerObj.GetComponent<Flower>();
            flower.flowerGenerate(spawnPos.position);
            flower.positionIndex = posIndex;

            usedPositions.Add(posIndex);
            flowerCount++;
        }

        if (bugCount < maxBugCount)
        {
            GameObject bugObj;
            bugObj = Instantiate(bugPrefab, Vector3.zero, Quaternion.identity);
            LightBug bug = bugObj.GetComponent<LightBug>();
            bug.generateLightBug();
            bugCount++;
        }

        if (shadowCount < minShadow)
        {
            GameObject shadowObj = Instantiate(shadowPrefab, Vector3.zero, Quaternion.identity);
            Shadow shadow = shadowObj.GetComponent<Shadow>();
            shadow.StartSpawnScale();
            shadowCount++;
        }
    }

    public void RecycleFlower(Flower flower)
    {
        flower.gameObject.SetActive(false);
        UnusedFlowerPool.Add(flower.gameObject);
        usedPositions.Remove(flower.positionIndex);
        flowerCount--;
    }

    public void RecycleBug() {
        bugCount--;
    }
}
