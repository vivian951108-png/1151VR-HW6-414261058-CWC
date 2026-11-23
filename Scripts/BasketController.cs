
using UnityEngine;

public class BasketController : MonoBehaviour
{
    [Header("Stage Settings")]
    public Transform stageCenter;
    public float cellSize = 2f;
    public float basketY = 0.6f;

    [Header("Movement")]
    public float moveSpeed = 10f;

    private Rigidbody rb;
    private Vector3 targetPosition;

    private int gridX = 0;
    private int gridZ = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (stageCenter == null)
        {
            Debug.LogError("請設定 Stage Center！");
            enabled = false;
            return;
        }

        UpdateTargetPosition();

        // 遊戲開始時，桶子位於九宮格中央
        transform.position = targetPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("按下右方向鍵！");
        }
        if (Director.Instance != null &&
            !Director.Instance.IsGameRunning)
            return;

        // 方向鍵控制，每次移動一格
        if (Input.GetKeyDown(KeyCode.UpArrow))
            gridZ++;

        else if (Input.GetKeyDown(KeyCode.DownArrow))
            gridZ--;

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            gridX--;

        else if (Input.GetKeyDown(KeyCode.RightArrow))
            gridX++;

        // 限制在九宮格內
        gridX = Mathf.Clamp(gridX, -1, 1);
        gridZ = Mathf.Clamp(gridZ, -1, 1);

        UpdateTargetPosition();
    }

    void UpdateTargetPosition()
    {
        Vector3 center = stageCenter.position;

        targetPosition = new Vector3(
            center.x + gridX * cellSize,
            center.y + basketY,
            center.z + gridZ * cellSize
        );
    }

    void FixedUpdate()
    {
        if (Director.Instance != null &&
            !Director.Instance.IsGameRunning)
            return;

        Vector3 nextPosition = Vector3.MoveTowards(
            rb.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }



    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("碰撞成功：" + other.name);

        GameObject item = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        Debug.Log("物件 Tag：" + item.tag);

        if (Director.Instance == null)
        {
            Debug.LogError("找不到 Director！");
            return;
        }

        if (!Director.Instance.IsGameRunning)
        {
            Debug.LogWarning("遊戲目前不是 Running 狀態！");
            return;
        }

        if (item.CompareTag("Apple"))
        {
            Debug.Log("接到蘋果，加 1 分");
            Director.Instance.AddScore(1);
            Destroy(item);
        }
        else if (item.CompareTag("Bomb"))
        {
            Debug.Log("接到炸彈，扣 3 分");
            Director.Instance.AddScore(-3);
            Destroy(item);
        }
    }


}

