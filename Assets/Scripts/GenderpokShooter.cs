using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GenderpokShooter : MonoBehaviour
{
    [Header("Pok")]
    [SerializeField] private AudioClip pok;
    [SerializeField] private AudioSource sfxSource;
    [Header("UI")]
    [SerializeField] private Slider sliderPletokan;
    [SerializeField] private Slider sliderSudut;
    [SerializeField] private TMP_Text textSudutValue;

    [Header("Range Tampilan Sudut")]
    [SerializeField] private float sudutDisplayMin = 0f;
    [SerializeField] private float sudutDisplayMax = 90f;

    [Header("Transforms")]
    [SerializeField] private Transform genderpok;
    [SerializeField] private Transform pelatuk;
    [SerializeField] private Transform titikTembak;

    [Header("Peluru")]
    [SerializeField] private Rigidbody peluruPrefab;
    [SerializeField] private float gayaMaksimum = 20f;

    [Header("Posisi Pelatuk (Local Y)")]
    [SerializeField] private float pelatukYAtas = -3f;
    [SerializeField] private float pelatukYBawah = -4f;

    [SerializeField] private ProjectileCameraFollow cameraFollow;
    [SerializeField] private GameObject retryButtonReference;
    [SerializeField] private TMP_Text textHeight;
    [SerializeField] private TMP_Text textLength;
    [SerializeField] private float sliderParentDisabledAlpha = 0.3f;

    private SliderReleaseNotifier sliderReleaseNotifier;
    private Vector3 pelatukLocalPosAwal;
    private Vector3 genderpokLocalEulerAwal;
    private Image sliderPletokanParentImage;
    private Image sliderSudutParentImage;
    private Color sliderPletokanParentBaseColor = Color.white;
    private Color sliderSudutParentBaseColor = Color.white;

    private void Start()
    {
        if (genderpok == null)
        {
            genderpok = transform;
        }

        if (pelatuk != null)
        {
            pelatukLocalPosAwal = pelatuk.localPosition;
            SetPelatukBySliderValue(0f);
        }

        if (genderpok != null)
        {
            genderpokLocalEulerAwal = genderpok.localEulerAngles;
        }

        if (sliderPletokan != null)
        {
            sliderPletokan.onValueChanged.AddListener(OnPletokanValueChanged);
            if (sliderPletokan.transform.parent != null)
            {
                sliderPletokanParentImage = sliderPletokan.transform.parent.GetComponent<Image>();
                if (sliderPletokanParentImage != null)
                    sliderPletokanParentBaseColor = sliderPletokanParentImage.color;
            }

            sliderReleaseNotifier = sliderPletokan.GetComponent<SliderReleaseNotifier>();
            if (sliderReleaseNotifier == null)
            {
                sliderReleaseNotifier = sliderPletokan.gameObject.AddComponent<SliderReleaseNotifier>();
            }
            sliderReleaseNotifier.OnReleased += OnPletokanReleased;
            sliderPletokan.value = 0f;
        }

        if (sliderSudut != null)
        {
            sliderSudut.onValueChanged.AddListener(OnSudutValueChanged);
            if (sliderSudut.transform.parent != null)
            {
                sliderSudutParentImage = sliderSudut.transform.parent.GetComponent<Image>();
                if (sliderSudutParentImage != null)
                    sliderSudutParentBaseColor = sliderSudutParentImage.color;
            }

            OnSudutValueChanged(sliderSudut.value);
        }
    }

    private void OnDestroy()
    {
        if (sliderPletokan != null)
        {
            sliderPletokan.onValueChanged.RemoveListener(OnPletokanValueChanged);
        }

        if (sliderSudut != null)
        {
            sliderSudut.onValueChanged.RemoveListener(OnSudutValueChanged);
        }

        if (sliderReleaseNotifier != null)
        {
            sliderReleaseNotifier.OnReleased -= OnPletokanReleased;
        }
    }

    private void OnPletokanValueChanged(float value)
    {
        SetPelatukBySliderValue(value);
    }

    private void OnSudutValueChanged(float value)
    {
        float sudut = GetSudutDisplayValue(value);

        if (genderpok == null)
        {
            UpdateSudutText(sudut);
            return;
        }

        var euler = genderpokLocalEulerAwal;
        euler.x = sudut;
        genderpok.localEulerAngles = euler;
        UpdateSudutText(sudut);
    }

    private float GetSudutDisplayValue(float sliderValue)
    {
        if (sliderSudut == null)
            return sliderValue;

        float t = Mathf.InverseLerp(sliderSudut.minValue, sliderSudut.maxValue, sliderValue);
        return Mathf.Lerp(sudutDisplayMin, sudutDisplayMax, t);
    }

    private void UpdateSudutText(float sudut)
    {
        if (textSudutValue != null)
            textSudutValue.text = "Sudut: " + sudut.ToString("F0") + "\u00b0";
    }

    private void SetPelatukBySliderValue(float value)
    {
        if (pelatuk == null)
        {
            return;
        }

        var localPos = pelatukLocalPosAwal;
        localPos.y = Mathf.Lerp(pelatukYAtas, pelatukYBawah, Mathf.Clamp01(value));
        pelatuk.localPosition = localPos;
    }

    private void OnPletokanReleased(float releasedValue)
    {
        float multiplier = Mathf.Clamp01(releasedValue);

        if (sliderPletokan != null)
        {
            sliderPletokan.value = 0f;
        }

        SetPelatukBySliderValue(0f);

        if (multiplier <= 0f)
        {
            return;
        }

        if (sfxSource != null && pok != null)
            sfxSource.PlayOneShot(pok);

        Shoot(multiplier);
    }

    public void deactivateSliders()
    {
        if (sliderPletokan != null)
        {
            sliderPletokan.interactable = false;
        }


        if (sliderSudut != null)
        {
            sliderSudut.interactable = false;
        }

        SetSliderParentAlpha(sliderPletokanParentImage, sliderPletokanParentBaseColor, sliderParentDisabledAlpha);
        SetSliderParentAlpha(sliderSudutParentImage, sliderSudutParentBaseColor, sliderParentDisabledAlpha);

        BallisticProjectile.SetAllProjectileLinesVisible(true);
    }

    public void activateSliders()
    {
        if (sliderPletokan != null)
        {
            sliderPletokan.interactable = true;
        }

        if (sliderSudut != null)
        {
            sliderSudut.interactable = true;
        }

        SetSliderParentAlpha(sliderPletokanParentImage, sliderPletokanParentBaseColor, sliderPletokanParentBaseColor.a);
        SetSliderParentAlpha(sliderSudutParentImage, sliderSudutParentBaseColor, sliderSudutParentBaseColor.a);

        BallisticProjectile.setAllProjectileLinesColor(Color.white);
        BallisticProjectile.SetAllProjectileLinesVisible(false);

        if (retryButtonReference != null)
            retryButtonReference.SetActive(false);
    }

    private void SetSliderParentAlpha(Image image, Color baseColor, float alpha)
    {
        if (image == null)
            return;

        Color color = baseColor;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void Shoot(float multiplier)
    {
        if (peluruPrefab == null || genderpok == null)
            return;

        Transform spawnPoint = titikTembak;
        Rigidbody peluru = Instantiate(
            peluruPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
        peluru.useGravity = true;
        peluru.isKinematic = false;
        peluru.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 arahTembak = spawnPoint.forward.normalized;
        float gaya = gayaMaksimum * Mathf.Clamp01(multiplier);
        peluru.AddForce(arahTembak * gaya, ForceMode.Impulse);

        BallisticProjectile bp = peluru.GetComponent<BallisticProjectile>();

        if (bp != null)
        {
            bp.textHeight = textHeight;
            bp.textLength = textLength;
            bp.retryButton = retryButtonReference;
            bp.shooterController = this;
        }

        if (cameraFollow != null)
        {
            cameraFollow.FollowTarget(peluru.transform);
        }
    }
}

[RequireComponent(typeof(Slider))]
public class SliderReleaseNotifier : MonoBehaviour, IPointerUpHandler
{
    public System.Action<float> OnReleased;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnReleased?.Invoke(slider.value);
    }
}
