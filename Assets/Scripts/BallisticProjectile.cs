using UnityEngine;
using TMPro;
using System.Collections;

public class BallisticProjectile : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public Transform cameraFollowTarget;
    public GameObject retryButton; // Button untuk kembalikan camera
    public TMP_Text textHeight;
    public TMP_Text textLength;
    public GenderpokShooter shooterController;

    [Header("Settings")]
    public float meterScale = 0.5f; // 1 unity unit = 1 meter (sementara)
    public float groundY = 0f;
    public float shooterHeight = 1.2f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float maxHeight;
    private float lengthAtShooterHeight;
    private bool hasRecordedLength;
    private bool hasHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        cameraFollowTarget = GameObject.FindObjectOfType<Camera>().transform;
        textHeight = GameObject.Find("Texttinggi (TMP)").GetComponent<TMP_Text>();
        textLength = GameObject.Find("Textjauh (TMP)").GetComponent<TMP_Text>();
        startPosition = transform.position;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    void Update()
    {
        if (hasHit) return;

        float currentHeight = (transform.position.y - groundY) * meterScale;
        float horizontalDistance = Vector3.Distance(
            new Vector3(startPosition.x, 0, startPosition.z),
            new Vector3(transform.position.x, 0, transform.position.z)
        ) * meterScale;

        // Update max height
        if (currentHeight > maxHeight)
            maxHeight = currentHeight;

        // Record distance when same height as shooter (≈1 meter)
        if (!hasRecordedLength && transform.position.y <= shooterHeight)
        {
            lengthAtShooterHeight = horizontalDistance;
            hasRecordedLength = true;
        }

        // Update UI realtime
        if (textHeight != null)
            textHeight.text = currentHeight.ToString("F1") + " m";

        if (textLength != null)
            textLength.text = horizontalDistance.ToString("F1") + " m";

        // Draw trajectory line
        if (lineRenderer != null)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        hasHit = true;

        ProjectileCameraFollow cam = FindObjectOfType<ProjectileCameraFollow>();
        if (cam != null)
        {
            retryButton.SetActive(true);
            if (shooterController != null)
                shooterController.deactivateSliders();
            cam.MoveToOverview();
        }

        // Final UI values
        if (textHeight != null)
            textHeight.text = "Tinggi Max: " + maxHeight.ToString("F1") + " m";

        if (textLength != null)
            textLength.text = "Jarak: " + lengthAtShooterHeight.ToString("F1") + " m";

        StartCoroutine(AfterHitRoutine());
    }
    public static void SetAllProjectileLinesVisible(bool isVisible)
    {
        BallisticProjectile[] allProjectiles = FindObjectsOfType<BallisticProjectile>();
        foreach (BallisticProjectile projectile in allProjectiles)
        {
            if (projectile.lineRenderer != null)
                projectile.lineRenderer.enabled = isVisible;
        }
    }

    public static void setAllProjectileLinesColor(Color color)
    {
        BallisticProjectile[] allProjectiles = FindObjectsOfType<BallisticProjectile>();
        foreach (BallisticProjectile projectile in allProjectiles)
        {
            if (projectile.lineRenderer != null)
                projectile.lineRenderer.material.color = color;
        }
    }

    private IEnumerator AfterHitRoutine()
    {
        yield return new WaitForSeconds(5f);

        // Visibility is controlled globally by GenderpokShooter while retry UI is active.
        if (lineRenderer != null && (retryButton == null || !retryButton.activeSelf))
            lineRenderer.enabled = false;
    }
}