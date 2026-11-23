
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CatchArea : MonoBehaviour
{
    [Header("Basket")]
    public Transform basket;

    [Header("Catch Settings")]
    public Vector3 offset = Vector3.zero;
    public Vector3 catchSize = new Vector3(1.5f, 1f, 1.5f);

    private BoxCollider box;
    private Rigidbody rb;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        // 設定碰撞盒
        box.isTrigger = true;
        box.center = Vector3.zero;
        box.size = catchSize;

        // 不使用重力
        rb.useGravity = false;
        rb.isKinematic = true;

        // 避免繼承模型旋轉與縮放
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    void Start()
    {
        if (basket == null)
        {
            Debug.LogError("CatchArea 尚未指定 Basket！");
            enabled = false;
            return;
        }

        transform.position = basket.position + offset;
        rb.position = transform.position;
    }

    void FixedUpdate()
    {
        if (basket == null)
            return;

        if (Director.Instance != null &&
            !Director.Instance.IsGameRunning)
            return;

        Vector3 targetPosition = basket.position + offset;

        rb.MovePosition(targetPosition);
    }

    void OnTriggerEnter(Collider other)
    {
        // 避免偵測到自己或籃子
        if (other.transform == basket ||
            other.transform.IsChildOf(basket))
            return;

        GameObject item = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        Debug.Log(
            "CatchArea 碰到：" + item.name +
            " | Tag：" + item.tag
        );

        if (Director.Instance == null ||
            !Director.Instance.IsGameRunning)
            return;

        if (item.CompareTag("Apple"))
        {
            Debug.Log("接到蘋果！+1");

            Director.Instance.AddScore(1);
            Destroy(item);
        }
        else if (item.CompareTag("Bomb"))
        {
            Debug.Log("接到炸彈！-3");

            Director.Instance.AddScore(-3);
            Destroy(item);
        }
    }
}
