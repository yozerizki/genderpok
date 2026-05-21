using TMPro;
using UnityEngine;

public class SequentialObjectInteraction : MonoBehaviour
{
    [Header("POK Sound")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip pok;

    [Header("Objects")]
    [SerializeField] private Transform obj1;
    [SerializeField] private Transform obj2;
    [SerializeField] private Transform obj3;
    [SerializeField] private Transform obj4;

    [Header("Object Colliders (optional, auto-filled from objects)")]
    [SerializeField] private Collider obj1Collider;
    [SerializeField] private Collider obj2Collider;
    [SerializeField] private Collider obj3Collider;
    [SerializeField] private Collider obj4Collider;

    [Header("UI")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject endPanel;

    [Header("Drag Settings")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private bool allowDragObj1 = true;
    [SerializeField] private bool allowDragObj2 = true;
    [SerializeField] private bool allowDragObj3 = true;
    [SerializeField] private bool allowDragObj4 = true;

    [Header("Clamp (Local Space of commonParent, axes: X and Z)")]
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float minZ = -3f;
    [SerializeField] private float maxZ = 3f;

    [Header("Clamp In Parent Local Space")]
    [SerializeField] private bool clampInParentLocalSpace = true;
    [SerializeField] private Transform commonParent;

    [Header("Drag Layer")]
    [Tooltip("Layer untuk drag zone collider. Buat layer 'Draggable', pasang trigger collider besar di child tiap object pada layer itu.")]
    [SerializeField] private LayerMask dragLayerMask = ~0;

    [Header("Step 1 Behavior")]
    [SerializeField] private Vector3 obj2LocalPositionAfterAttach = new Vector3(0f, 0f, 0.017f);
    [SerializeField] private bool lockObj2AfterAttach = true;

    [Header("Step 3 Behavior")]
    [SerializeField] private Vector3 obj1LocalPositionAfterAttachToObj3 = new Vector3(0f, 0f, 0.05f);
    [SerializeField] private float obj1FinalMinLocalZ = 0.037f;
    [SerializeField] private float obj1FinalMaxLocalZ = 0.05f;
    [SerializeField] private Vector3 obj3finallocalposition = new Vector3(-1.75f, 0f, 0.35f);

    private Transform currentDragged;
    private Plane dragPlane;
    private Vector3 dragOffset;
    private bool obj2Locked;
    private bool obj2AttachedToObj1;
    private bool obj1AttachedToObj3;
    private bool finalCollisionSoundPlayed;

    // 0 = wait step1, 1 = wait step2, 2 = wait step3, 3 = finished
    private int currentStep;

    private void Awake()
    {
        if (dragCamera == null)
            dragCamera = Camera.main;

        if (commonParent == null && obj1 != null)
            commonParent = obj1.parent;

        CacheColliders();

        if (progressText != null)
            progressText.text = "0%\nbuatlah pelatuk dengan\nbambu (1) dan bambu (2)";

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    private void Update()
    {
        KeepAttachedObjectsLocked();
        HandlePointerInput();
        ResolveStepSequence();
    }

    private void KeepAttachedObjectsLocked()
    {
        if (obj2AttachedToObj1 && obj1 != null && obj2 != null && obj2.parent == obj1)
            obj2.localPosition = obj2LocalPositionAfterAttach;

        if (obj1AttachedToObj3 && obj3 != null && obj1 != null && obj1.parent == obj3)
        {
            Vector3 local = obj1.localPosition;
            local.x = obj1LocalPositionAfterAttachToObj3.x;
            local.y = obj1LocalPositionAfterAttachToObj3.y;
            local.z = Mathf.Clamp(local.z, obj1FinalMinLocalZ, obj1FinalMaxLocalZ);
            obj3.localPosition = obj3finallocalposition;
            allowDragObj3 = false;
        }
    }

    private void CacheColliders()
    {
        if (obj1 != null && obj1Collider == null)
            obj1Collider = obj1.GetComponent<Collider>();

        if (obj2 != null && obj2Collider == null)
            obj2Collider = obj2.GetComponent<Collider>();

        if (obj3 != null && obj3Collider == null)
            obj3Collider = obj3.GetComponent<Collider>();

        if (obj4 != null && obj4Collider == null)
            obj4Collider = obj4.GetComponent<Collider>();
    }

    private void HandlePointerInput()
    {
        if (dragCamera == null)
            return;

        if (TryGetPointerDown(out Vector2 downPos))
            TryBeginDrag(downPos);

        if (TryGetPointerHeld(out Vector2 heldPos))
            DragCurrent(heldPos);

        if (TryGetPointerUp())
            currentDragged = null;
    }

    private void TryBeginDrag(Vector2 screenPos)
    {
        Ray ray = dragCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dragLayerMask))
            return;

        Transform draggableRoot = ResolveDraggableRoot(hit.transform);
        if (!CanDrag(draggableRoot))
            return;

        currentDragged = draggableRoot;
        dragPlane = new Plane(Vector3.up, new Vector3(0f, currentDragged.position.y, 0f));

        if (dragPlane.Raycast(ray, out float enter))
            dragOffset = currentDragged.position - ray.GetPoint(enter);
        else
            dragOffset = Vector3.zero;
    }

    private Transform ResolveDraggableRoot(Transform hit)
    {
        if (hit == null) return null;
        if (obj1 != null && (hit == obj1 || hit.IsChildOf(obj1))) return obj1;
        if (obj2 != null && (hit == obj2 || hit.IsChildOf(obj2))) return obj2;
        if (obj3 != null && (hit == obj3 || hit.IsChildOf(obj3))) return obj3;
        if (obj4 != null && (hit == obj4 || hit.IsChildOf(obj4))) return obj4;
        return null;
    }

    private void DragCurrent(Vector2 screenPos)
    {
        if (currentDragged == null)
            return;

        Ray ray = dragCamera.ScreenPointToRay(screenPos);
        if (!dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 target = ray.GetPoint(enter) + dragOffset;

        if (clampInParentLocalSpace && commonParent != null)
        {
            Vector3 local = commonParent.InverseTransformPoint(target);
            Vector3 currentLocal = commonParent.InverseTransformPoint(currentDragged.position);
            local.x = Mathf.Clamp(local.x, minX, maxX);
            local.z = Mathf.Clamp(local.z, minZ, maxZ);
            local.y = currentLocal.y;
            target = commonParent.TransformPoint(local);
        }
        else
        {
            target.x = Mathf.Clamp(target.x, minX, maxX);
            target.z = Mathf.Clamp(target.z, minZ, maxZ);
            target.y = currentDragged.position.y;
        }

        if (obj1AttachedToObj3 && currentDragged == obj1 && obj3 != null && obj1.parent == obj3)
        {
            Vector3 localOnObj3 = obj3.InverseTransformPoint(target);
            localOnObj3.x = obj1LocalPositionAfterAttachToObj3.x;
            localOnObj3.y = obj1LocalPositionAfterAttachToObj3.y;
            localOnObj3.z = Mathf.Clamp(localOnObj3.z, obj1FinalMinLocalZ, obj1FinalMaxLocalZ);
            target = obj3.TransformPoint(localOnObj3);
        }

        currentDragged.position = target;
    }

    private bool CanDrag(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        if (obj1 != null && target == obj1)
            return allowDragObj1;

        if (obj2 != null && target == obj2)
        {
            if (obj2Locked)
                return false;
            return allowDragObj2;
        }

        if (obj3 != null && target == obj3)
            return allowDragObj3;

        if (obj4 != null && target == obj4)
            return allowDragObj4;

        return false;
    }

    private void ResolveStepSequence()
    {
        CacheColliders();

        if (currentStep == 0)
        {
            if (IsIntersecting(obj2Collider, obj1Collider))
            {
                DoStep1();
                currentStep = 1;
            }
            return;
        }

        if (currentStep == 1)
        {
            if (IsIntersecting(obj4Collider, obj3Collider))
            {
                DoStep2();
                currentStep = 2;
            }
            return;
        }

        if (currentStep == 2)
        {
            if (IsIntersecting(obj2Collider, obj3Collider))
            {
                DoStep3();
                currentStep = 3;
            }
        }

        if (currentStep == 3)
        {
            bool isFinalIntersecting = IsIntersecting(obj1Collider, obj3Collider);

            if (isFinalIntersecting)
            {
                if (finalCollisionSoundPlayed)
                    return;

                if (sfx != null && pok != null)
                    sfx.PlayOneShot(pok);
                if (progressText != null)
                    progressText.text = "100%\nberhasil!!";


                if (endPanel != null)
                    endPanel.SetActive(true);

                finalCollisionSoundPlayed = true;
            }
            else
            {
                // Reset gate after objects separate so next intersection can play again.
                finalCollisionSoundPlayed = false;
            }
        }
    }

    private bool IsIntersecting(Collider a, Collider b)
    {
        if (a == null || b == null)
            return false;

        if (!a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy)
            return false;

        return a.bounds.Intersects(b.bounds);
    }

    private void DoStep1()
    {
        if (obj1 != null && obj2 != null)
        {
            obj2.SetParent(obj1);
            obj2.localPosition = obj2LocalPositionAfterAttach;
            obj2Locked = lockObj2AfterAttach;
            obj2AttachedToObj1 = true;
        }

        if (progressText != null)
            progressText.text = "30%\nmasukkan proyektil (4)\nkedalam selongsong (3)";
    }

    private void DoStep2()
    {
        if (obj4 != null)
            Destroy(obj4.gameObject);

        obj4 = null;
        obj4Collider = null;

        if (progressText != null)
            progressText.text = "65%\nmasukkan pelatuk\nkedalam selongsong";
    }

    private void DoStep3()
    {
        if (obj1 != null && obj3 != null)
        {
            obj1.SetParent(obj3);
            obj1.localPosition = obj1LocalPositionAfterAttachToObj3;
            obj1AttachedToObj3 = true;
            allowDragObj1 = true;
            if (progressText != null)
                progressText.text = "100%\ncoba tembakkan!!";
        }

    }


    private bool TryGetPointerDown(out Vector2 screenPos)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                screenPos = t.position;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        screenPos = default;
        return false;
    }

    private bool TryGetPointerHeld(out Vector2 screenPos)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                screenPos = t.position;
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        screenPos = default;
        return false;
    }

    private bool TryGetPointerUp()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                return true;
        }

        return Input.GetMouseButtonUp(0);
    }
}
