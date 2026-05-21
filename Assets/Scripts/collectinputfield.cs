using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class collectinputfield : MonoBehaviour
{
    [Header("Panels (ordered)")]
    [SerializeField] private List<GameObject> panelRoots = new List<GameObject>();

    [Header("Optional Manual Input Order")]
    [SerializeField] private List<TMP_InputField> manualInputFields = new List<TMP_InputField>();

    [Header("Behavior")]
    [SerializeField] private bool saveOnDisable = true;
    [SerializeField] private bool debugLog = true;

    private readonly List<TMP_InputField> cachedInputFields = new List<TMP_InputField>();
    private string[] cachedAnswers = new string[0];

    // Listeners stored so we can cleanly remove them.
    private readonly List<UnityEngine.Events.UnityAction<string>> fieldListeners =
        new List<UnityEngine.Events.UnityAction<string>>();

    private void Start()
    {
        StartCoroutine(LoadAnswersAfterOneFrame());
    }

    private IEnumerator LoadAnswersAfterOneFrame()
    {
        // Wait one frame so inactive/active panel hierarchy and TMP labels are fully initialized.
        yield return null;
        LoadAllAnswersToFields();
        SubscribeFieldListeners();
    }

    private void OnDestroy()
    {
        UnsubscribeFieldListeners();
    }

    private void OnDisable()
    {
        if (!saveOnDisable) return;

        // During scene teardown OnDisable fires while fields may already be destroyed or cleared.
        // We must NOT read field.text here — use cachedAnswers which is kept current by onValueChanged.
        FlushCacheToProfile();
    }

    // Writes cachedAnswers to dontDestroy + disk without touching any input fields.
    private void FlushCacheToProfile()
    {
        if (dontDestroy.Instance == null)
        {
            if (debugLog)
                Debug.LogWarning("[collectinputfield] FlushCacheToProfile skipped: dontDestroy.Instance is null.");
            return;
        }

        if (cachedAnswers.Length == 0)
        {
            if (debugLog)
                Debug.LogWarning("[collectinputfield] FlushCacheToProfile skipped: cachedAnswers is empty (fields not yet loaded?).");
            return;
        }

        dontDestroy.Instance.jawabanMateri = (string[])cachedAnswers.Clone();
        dontDestroy.Instance.SaveJawabanToProfile();

        if (debugLog)
        {
            int nonEmpty = 0;
            for (int i = 0; i < cachedAnswers.Length; i++)
                if (!string.IsNullOrEmpty(cachedAnswers[i])) nonEmpty++;
            Debug.Log("[collectinputfield] FlushCacheToProfile: answers=" + cachedAnswers.Length + ", nonEmpty=" + nonEmpty + ".");
        }
    }

    // Keep cachedAnswers up-to-date as user types, so FlushCacheToProfile always has latest values.
    private void SubscribeFieldListeners()
    {
        UnsubscribeFieldListeners();
        fieldListeners.Clear();

        for (int i = 0; i < cachedInputFields.Count; i++)
        {
            TMP_InputField field = cachedInputFields[i];
            if (field == null)
            {
                fieldListeners.Add(null);
                continue;
            }

            int capturedIndex = i;
            UnityEngine.Events.UnityAction<string> listener = val =>
            {
                if (capturedIndex < cachedAnswers.Length)
                    cachedAnswers[capturedIndex] = val ?? "";
            };

            field.onValueChanged.AddListener(listener);
            fieldListeners.Add(listener);
        }

        if (debugLog)
            Debug.Log("[collectinputfield] Subscribed onValueChanged for " + fieldListeners.Count + " fields.");
    }

    private void UnsubscribeFieldListeners()
    {
        for (int i = 0; i < fieldListeners.Count && i < cachedInputFields.Count; i++)
        {
            if (cachedInputFields[i] != null && fieldListeners[i] != null)
                cachedInputFields[i].onValueChanged.RemoveListener(fieldListeners[i]);
        }
        fieldListeners.Clear();
    }

    // Call this after changing active panel, if needed.
    public void RefreshFieldsFromPanels()
    {
        UnsubscribeFieldListeners();
        RebuildFieldCache();
        EnsureAnswerCapacity();
        LoadAllAnswersToFields();
        SubscribeFieldListeners();
    }

    // Call this from your panel-switch button OnClick to persist current inputs.
    // This is an explicit save: reads from live fields, updates cache, then persists.
    public void SaveAllAnswers()
    {
        // Read current text from all live fields into cachedAnswers.
        for (int i = 0; i < cachedInputFields.Count; i++)
        {
            if (i < cachedAnswers.Length)
                cachedAnswers[i] = cachedInputFields[i] != null ? cachedInputFields[i].text : "";
        }

        if (debugLog)
        {
            int nonEmpty = 0;
            for (int i = 0; i < cachedAnswers.Length; i++)
                if (!string.IsNullOrEmpty(cachedAnswers[i])) nonEmpty++;
            Debug.Log("[collectinputfield] SaveAllAnswers: fields=" + cachedInputFields.Count + ", nonEmpty=" + nonEmpty + ".");
        }

        FlushCacheToProfile();

        // Re-apply so inactive panels also reflect saved values.
        ApplyAnswersToFields();
    }

    // Load from dontDestroy/profile into all input fields based on cached order.
    public void LoadAllAnswersToFields()
    {
        RebuildFieldCache();
        EnsureAnswerCapacity();

        if (dontDestroy.Instance == null)
        {
            if (debugLog)
                Debug.LogWarning("[collectinputfield] LoadAllAnswersToFields: dontDestroy.Instance is null. Applying local cache only.");
            ApplyAnswersToFields();
            return;
        }

        // Always pull fresh from profile in case scene was reopened.
        dontDestroy.Instance.LoadJawabanFromProfile();

        string[] saved = dontDestroy.Instance.jawabanMateri;
        if (saved != null && saved.Length > 0)
        {
            int count = Mathf.Min(saved.Length, cachedAnswers.Length);
            for (int i = 0; i < count; i++)
                cachedAnswers[i] = saved[i] ?? "";

            if (debugLog)
                Debug.Log("[collectinputfield] Loaded from profile: savedLen=" + saved.Length + ", fields=" + cachedInputFields.Count + ", applied=" + count + ".");
        }
        else if (debugLog)
        {
            Debug.LogWarning("[collectinputfield] LoadAllAnswersToFields: jawabanMateri is null or empty — no saved data yet.");
        }

        ApplyAnswersToFields();
    }

    private void RebuildFieldCache()
    {
        cachedInputFields.Clear();

        if (manualInputFields != null && manualInputFields.Count > 0)
        {
            for (int i = 0; i < manualInputFields.Count; i++)
            {
                TMP_InputField input = manualInputFields[i];
                if (input != null)
                    cachedInputFields.Add(input);
            }
            return;
        }

        if (panelRoots == null)
            return;

        for (int i = 0; i < panelRoots.Count; i++)
        {
            GameObject panel = panelRoots[i];
            if (panel == null)
                continue;

            TMP_InputField[] fields = panel.GetComponentsInChildren<TMP_InputField>(true);
            for (int j = 0; j < fields.Length; j++)
            {
                if (fields[j] != null)
                    cachedInputFields.Add(fields[j]);
            }
        }

        if (debugLog)
            Debug.Log("[collectinputfield] RebuildFieldCache complete. Input fields found=" + cachedInputFields.Count + ".");
    }

    private void EnsureAnswerCapacity()
    {
        if (cachedAnswers != null && cachedAnswers.Length == cachedInputFields.Count)
            return;

        string[] resized = new string[cachedInputFields.Count];
        if (cachedAnswers != null)
        {
            int copyCount = Mathf.Min(cachedAnswers.Length, resized.Length);
            for (int i = 0; i < copyCount; i++)
                resized[i] = cachedAnswers[i] ?? "";
        }

        for (int i = 0; i < resized.Length; i++)
        {
            if (resized[i] == null)
                resized[i] = "";
        }

        cachedAnswers = resized;
    }

    private void ApplyAnswersToFields()
    {
        int count = Mathf.Min(cachedAnswers.Length, cachedInputFields.Count);
        for (int i = 0; i < count; i++)
        {
            if (cachedInputFields[i] != null)
                cachedInputFields[i].text = cachedAnswers[i] ?? "";
        }

        if (debugLog)
            Debug.Log("[collectinputfield] ApplyAnswersToFields wrote TMP_InputField.text for " + count + " fields.");
    }
}
