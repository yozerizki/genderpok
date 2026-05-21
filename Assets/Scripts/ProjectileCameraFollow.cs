using UnityEngine;
using System.Collections;

public class ProjectileCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 followOffset = new Vector3(0, 2, -6);
    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 8f;

    private Transform target;
    private bool isFollowing;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    [SerializeField] private Transform overviewPoint;
    void Start()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (!isFollowing || target == null)
            return;

        Vector3 desiredPosition = target.position + target.TransformDirection(followOffset);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmoothSpeed * Time.deltaTime
        );

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    public void FollowTarget(Transform newTarget)
    {
        target = newTarget;
        isFollowing = true;
    }

    public void ReturnToDefaultImmediate()
    {
        isFollowing = false;
        target = null;

        StartCoroutine(MoveToPosition(defaultPosition, defaultRotation));
    }

    // private IEnumerator SmoothReturn()
    // {
    //     float t = 0f;
    //     float duration = 1f;

    //     Vector3 startPos = transform.position;
    //     Quaternion startRot = transform.rotation;

    //     while (t < duration)
    //     {
    //         t += Time.deltaTime;
    //         float lerp = t / duration;

    //         transform.position = Vector3.Lerp(startPos, defaultPosition, lerp);
    //         transform.rotation = Quaternion.Slerp(startRot, defaultRotation, lerp);

    //         yield return null;
    //     }

    //     transform.position = defaultPosition;
    //     transform.rotation = defaultRotation;
    // }
    public void MoveToOverview()
    {
        isFollowing = false;
        target = null;

        if (overviewPoint != null)
        {
            StopAllCoroutines();
            StartCoroutine(MoveToPosition(
                overviewPoint.position,
                overviewPoint.rotation
            ));
        }
    }
    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        float duration = 1f;
        float t = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, lerp);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}