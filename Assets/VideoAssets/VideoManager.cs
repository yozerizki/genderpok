using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class VideoManager : MonoBehaviour
{
    const float PrepareTimeoutSeconds = 8f;

    [Header("UI & Video")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public RawImage videoScreen;
    public Slider videoSlider;
    public Button playPauseButton;
    public Sprite playIcon;
    public Sprite pauseIcon;
    private Image playPauseIconImage;
    public CanvasGroup playPauseCanvasGroup;
    public GameObject bufferingIndicator;
    private Coroutine bgmFadeCoroutine;
    private Coroutine fadeCoroutine;

    private AudioSource audiobgm;
    private AudioSource audioSource;
    private bool isDragging = false;
    private bool isReadyToPlay = false;
    private bool isPrepareCompleted = false;

    void Awake()
    {
        if (!CacheReferences())
        {
            Debug.LogError("VideoManager: Failed to cache references in Awake");
            return;
        }

        ConfigureVideoPlayerAudioOutput();
        videoPlayer.errorReceived += (vp, msg) => Debug.LogError("Video Error: " + msg);
    }

    void Update()
    {
        if (!isReadyToPlay || isDragging || !videoPlayer.isPlaying || videoPlayer.length <= 0)
            return;

        videoSlider.value = (float)(videoPlayer.time / videoPlayer.length);
    }

    public void PlayVideo(VideoClip clip)
    {
        StopAllCoroutines();
        isReadyToPlay = false;

        videoPlayer.Stop();
        videoPlayer.clip = clip;

        videoPanel.SetActive(true);
        StartCoroutine(PrepareAndPlay());
    }



    private IEnumerator PrepareAndPlay()
    {
        videoSlider.interactable = false;

        // Re-apply audio config per clip — KRITIS untuk Windows Media Foundation stability
        ConfigureVideoPlayerAudioOutput();

        videoPlayer.Prepare();

        float elapsed = 0f;
        while (!videoPlayer.isPrepared && elapsed < PrepareTimeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("VideoManager: Prepare() timeout untuk clip: " + videoPlayer.clip.name + ". Cek transcode Standalone, codec, dan audio config.");
            videoPanel.SetActive(false);
            yield break;
        }

        // Reset time sebelum Play() untuk mencegah offset audio/video di Media Foundation
        videoPlayer.time = 0d;
        videoPlayer.frame = 0;
        videoSlider.value = 0f;

        videoPlayer.Play();
        yield return null; // frame delay
        videoSlider.interactable = true;

        // ✅ Fade BGM saat play pertama kali
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeAudioVolume(audiobgm, 0.18f));

        isReadyToPlay = true;
        UpdatePlayPauseIcon();

        // Auto-hide play/pause button
        if (playPauseCanvasGroup != null)
        {
            playPauseCanvasGroup.alpha = 1f;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutPlayPauseIcon(2f));
        }

        // Register end-of-video callback (guard against duplicate subscriptions)
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    //public void TogglePlayPause()
    //{
    //    if (!isReadyToPlay) return;

    //    if (videoPlayer.isPlaying)
    //        videoPlayer.Pause();
    //    else
    //        videoPlayer.Play();

    //    UpdatePlayPauseIcon();

    //    // Reset alpha dan mulai fade-out
    //    if (playPauseCanvasGroup != null)
    //    {
    //        playPauseCanvasGroup.alpha = 1f;

    //        if (fadeCoroutine != null)
    //            StopCoroutine(fadeCoroutine);

    //        fadeCoroutine = StartCoroutine(FadeOutPlayPauseIcon(2f)); // delay 2 detik
    //    }
    //}
    public void TogglePlayPause()
    {
        if (!isReadyToPlay) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();

            // Fade in BGM saat pause
            if (bgmFadeCoroutine != null)
                StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeAudioVolume(audiobgm, 1f));
        }
        else
        {
            videoPlayer.Play();

            // Fade out BGM saat play
            if (bgmFadeCoroutine != null)
                StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeAudioVolume(audiobgm, 0.18f));
        }

        UpdatePlayPauseIcon();

        if (playPauseCanvasGroup != null)
        {
            playPauseCanvasGroup.alpha = 1f;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutPlayPauseIcon(2f));
        }
    }
    private void UpdatePlayPauseIcon()
    {
        if (playPauseButton == null) return;

        if (playPauseIconImage != null)
        {
            playPauseIconImage.sprite = videoPlayer.isPlaying ? pauseIcon : playIcon;
        }
    }



    public void OnSliderBeginDrag()
    {
        isDragging = true;
    }

    public void OnSliderEndDrag()
    {
        isDragging = false;

        // Pastikan time di-set ke waktu yang dipilih setelah selesai drag
        StartCoroutine(SeekWithBuffering());
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // Kembalikan volume BGM
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeAudioVolume(audiobgm, 1f));
    }

    private IEnumerator SeekWithBuffering()
    {
        if (bufferingIndicator != null)
            bufferingIndicator.SetActive(true);

        double newTime = videoSlider.value * videoPlayer.length;
        videoPlayer.time = newTime;

        // Tunggu sampai posisi video benar-benar update
        yield return null;

        // Tunggu hingga videoPlayer mulai main lagi
        while (!videoPlayer.isPlaying)
            yield return null;

        if (bufferingIndicator != null)
            bufferingIndicator.SetActive(false);
    }

    private void SeekVideoToSliderValue()
    {
        if (videoPlayer.length > 0)
        {
            double newTime = videoSlider.value * videoPlayer.length;
            videoPlayer.time = newTime;
        }
    }

    public void OnSliderValueChanged(float value)
    {
        // Jangan ubah time di sini saat dragging — tunggu hingga drag selesai
        if (isDragging)
        {
            // Optional: update preview UI atau waktu
        }
    }

    public void BackToThumbnails()
    {
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.Stop();
        videoPlayer.time = 0d;
        videoPlayer.frame = 0;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }

        videoPanel.SetActive(false);
        isReadyToPlay = false;
        bgmFadeCoroutine = StartCoroutine(FadeAudioVolume(audiobgm, 1f));
        if (playPauseCanvasGroup != null)
        {
            playPauseCanvasGroup.alpha = 1f;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
        }
    }
    
    private bool CacheReferences()
    {
        if (audiobgm == null)
        {
            GameObject holder = GameObject.Find("holder");
            if (holder != null)
            {
                audiobgm = holder.GetComponent<AudioSource>();
            }
        }

        if (videoPlayer == null)
        {
            Debug.LogError("VideoManager: VideoPlayer belum di-assign.");
            return false;
        }

        if (audioSource == null)
        {
            audioSource = videoPlayer.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("VideoManager: AudioSource pada object VideoPlayer tidak ditemukan.");
            return false;
        }

        if (playPauseButton != null && playPauseIconImage == null)
        {
            playPauseIconImage = playPauseButton.gameObject.GetComponentInChildren<Image>();
        }

        return true;
    }

    private void ConfigureVideoPlayerAudioOutput()
    {
        if (videoPlayer == null || audioSource == null)
        {
            return;
        }

        videoPlayer.source = VideoSource.VideoClip;
        audioSource.playOnAwake = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }
    
    private IEnumerator FadeOutPlayPauseIcon(float delay, float fadeDuration = 0.3f)
    {
        yield return new WaitForSeconds(delay);

        float startAlpha = playPauseCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            playPauseCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }

        playPauseCanvasGroup.alpha = 0f;
    }
    private IEnumerator FadeAudioVolume(AudioSource audio, float targetVolume, float duration = 0.5f)
    {
        float startVolume = audio.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        audio.volume = targetVolume;
    }
}
