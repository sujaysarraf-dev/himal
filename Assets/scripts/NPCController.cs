using UnityEngine;

public class NPCController : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;
    public float waitTimeAtEnd = 3f;

    private int currentPointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("No path points assigned to NPC " + gameObject.name);
        }

        if (!CompareTag("NPC"))
        {
            gameObject.tag = "NPC";
        }
    }

    void Update()
    {
        if (isWaiting || pathPoints == null || pathPoints.Length == 0) return;

        Transform targetPoint = pathPoints[currentPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);
        }

        transform.position += transform.forward * speed * Time.deltaTime;

        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(targetPoint.position.x, 0, targetPoint.position.z));

        if (distance < 1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Length)
            {
                StartCoroutine(WaitAndReset());
            }
        }
    }

    System.Collections.IEnumerator WaitAndReset()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtEnd);
        currentPointIndex = 0;
        transform.position = pathPoints[0].position;
        isWaiting = false;
    }
}
