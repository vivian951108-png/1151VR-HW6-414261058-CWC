
using UnityEngine;

public class FallingItem : MonoBehaviour
{
    [Header("Falling Settings")]
    public float fallSpeed = 2f;
    public float destroyY = -20f;

    [Header("Catch Settings")]
    public float catchHeight = 0.6f;
    public float catchDistance = 0.5f;

    private Rigidbody rb;
    private Transform basket;

    private bool processed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        BasketController controller =
            FindFirstObjectByType<BasketController>();

        if (controller != null)
        {
            basket = controller.transform;
        }
        else
        {
            Debug.LogError("找不到 BasketController！");
        }
    }

    public void Init(float speed)
    {
        fallSpeed = speed;
    }

    void FixedUpdate()
    {
        if (Director.Instance == null ||
            !Director.Instance.IsGameRunning)
            return;

        Vector3 currentPosition = rb != null
            ? rb.position
            : transform.position;

        Vector3 nextPosition = currentPosition +
            Vector3.down * fallSpeed * Time.fixedDeltaTime;

        if (rb != null)
            rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        // 物品下降並經過接物高度時，判斷是否接住
        if (!processed &&
            currentPosition.y >= catchHeight &&
            nextPosition.y <= catchHeight)
        {
            processed = true;
            CheckCatch();
        }

        // 掉出舞台後自動刪除
        if (nextPosition.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void CheckCatch()
    {
        if (basket == null)
            return;

        Vector3 itemPos = transform.position;
        Vector3 basketPos = basket.position;

        float dx = Mathf.Abs(itemPos.x - basketPos.x);
        float dz = Mathf.Abs(itemPos.z - basketPos.z);

        Debug.Log(
            "檢查接物：" + gameObject.name +
            " | X距離：" + dx +
            " | Z距離：" + dz
        );

        // 確認水果與籃子在同一格
        if (dx <= catchDistance &&
            dz <= catchDistance)
        {
            if (CompareTag("Apple"))
            {
                // Level 1 +2, Level 2 +1
                int points =
                    Director.Instance.CurrentLevel == 1 ? 2 : 1;

                Director.Instance.AddScore(points);

                Debug.Log(
                    "Level " + Director.Instance.CurrentLevel +
                    " 接到紅蘋果！+" + points + " 分"
                );

                Destroy(gameObject);
            }
            else if (CompareTag("Bomb"))
            {
                Director.Instance.AddScore(-3);

                Debug.Log("接到炸彈！-3 分");

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning(
                    "物件 Tag 不正確：" + gameObject.tag
                );
            }
        }
    }
}
