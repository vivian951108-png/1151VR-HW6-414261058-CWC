
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ItemFactory : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject applePrefab;
    public GameObject bombPrefab;

    [Header("Nine Grid Spawn Points")]
    public Transform[] spawnPoints = new Transform[9];

    [Header("Spawn Settings")]
    public float spawnHeight = 3f;

    [Header("Current Difficulty")]
    public float interval = 1.5f;
    public float fallSpeed = 2f;
    public float bombSpeed = 2f;
    public float bombRate = 0.25f;
    public float warningTime = 1f;
    public int spawnCount = 1;

    private readonly List<GameObject> shadows =
        new List<GameObject>();

    public void StartLevel(int level)
    {
        StopLevel();

        if (applePrefab == null || bombPrefab == null)
        {
            Debug.LogError("Factory 缺少 Prefab！");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length != 9)
        {
            Debug.LogError("必須設定 9 個 Spawn Points！");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogError("有 Spawn Point 未指定！");
                return;
            }
        }

        if (level == 1)
        {
            interval = 1.5f;
            fallSpeed = 2f;
            bombSpeed = 2f;
            bombRate = 0.25f;
            warningTime = 1f;
            spawnCount = 1;
        }
        else if (level == 2)
        {
            interval = 1.2f;
            fallSpeed = 3f;
            bombSpeed = 5f;
            bombRate = 0.35f;
            warningTime = 0.8f;
            spawnCount = 2;
        }
        else
        {
            interval = 0.8f;
            fallSpeed = 4f;
            bombSpeed = 7f;
            bombRate = 0.45f;
            warningTime = 0.5f;
            spawnCount = 3;
        }

        Debug.Log(
            "Level " + level +
            " | Warning: " + warningTime +
            " | Bomb Speed: " + bombSpeed +
            " | Spawn Count: " + spawnCount
        );

        StartCoroutine(SpawnLoop(level));
    }

    public void StopLevel()
    {
        StopAllCoroutines();

        foreach (GameObject shadow in shadows)
        {
            if (shadow != null)
                Destroy(shadow);
        }

        shadows.Clear();
    }

    IEnumerator SpawnLoop(int level)
    {
        while (Director.Instance != null &&
               Director.Instance.IsGameRunning &&
               Director.Instance.CurrentLevel == level)
        {
            yield return StartCoroutine(SpawnWave(level));

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, interval)
            );
        }
    }

    IEnumerator SpawnWave(int level)
    {
        List<int> available = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
            available.Add(i);

        List<int> selected = new List<int>();
        List<GameObject> waveShadows =
            new List<GameObject>();

        int count = Mathf.Min(spawnCount, available.Count);

        // 每波選擇不同格子
        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(0, available.Count);
            int index = available[pick];

            available.RemoveAt(pick);
            selected.Add(index);

            Transform tile = spawnPoints[index];

            Vector3 shadowPosition = new Vector3(
                tile.position.x,
                GetSurfaceY(tile) + 0.03f,
                tile.position.z
            );

            GameObject shadow = CreateShadow(shadowPosition);

            waveShadows.Add(shadow);
            shadows.Add(shadow);
        }

        // 先顯示落點陰影
        yield return new WaitForSeconds(warningTime);

        if (Director.Instance == null ||
            !Director.Instance.IsGameRunning ||
            Director.Instance.CurrentLevel != level)
        {
            yield break;
        }

        // 產生 Apple 或 Bomb
        foreach (int index in selected)
        {
            Transform tile = spawnPoints[index];

            bool isBomb = Random.value < bombRate;

            GameObject prefab = isBomb
                ? bombPrefab
                : applePrefab;

            float speed = isBomb
                ? bombSpeed
                : fallSpeed;

            Vector3 spawnPosition = new Vector3(
                tile.position.x,
                GetSurfaceY(tile) + spawnHeight,
                tile.position.z
            );

            GameObject item = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            FallingItem falling =
                item.GetComponent<FallingItem>();

            if (falling != null)
            {
                falling.Init(speed);
            }
            else
            {
                Debug.LogError(
                    item.name + " 缺少 FallingItem！"
                );
            }
        }

        // 移除本波預告陰影
        foreach (GameObject shadow in waveShadows)
        {
            if (shadow != null)
                Destroy(shadow);

            shadows.Remove(shadow);
        }
    }

    float GetSurfaceY(Transform tile)
    {
        Renderer renderer = tile.GetComponent<Renderer>();

        return renderer != null
            ? renderer.bounds.max.y
            : tile.position.y;
    }

    GameObject CreateShadow(Vector3 position)
    {
        GameObject shadow =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        shadow.name = "LandingShadow";
        shadow.transform.position = position;

        shadow.transform.localScale =
            new Vector3(0.65f, 0.005f, 0.65f);

        Collider col = shadow.GetComponent<Collider>();

        if (col != null)
            Destroy(col);

        Renderer r = shadow.GetComponent<Renderer>();

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Lit"
        );

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader != null)
        {
            Material material = new Material(shader);
            Color color = new Color(0.08f, 0.08f, 0.08f);

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else
                material.color = color;

            r.material = material;
        }

        r.shadowCastingMode = ShadowCastingMode.Off;

        return shadow;
    }
}
