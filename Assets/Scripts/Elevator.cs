using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] Transform topPoint;
    [SerializeField] float speed = 2f;

    private bool moving;

    private void Update()
    {
        if (!moving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            topPoint.position,
            speed * Time.deltaTime
        );

        if (transform.position == topPoint.position)
        {
            moving = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            moving = true;
        }
    }
}
