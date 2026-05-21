using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dontDestroy : MonoBehaviour
{
    public static dontDestroy Instance { get; private set; }

    public bool soundon;

    // Jawaban input field per-index dari scene materi text
    public string[] jawabanMateri;
    [SerializeField] private bool debugJawabanLog = true;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // prevent duplicates!
        }
    }

    public void SaveJawabanToProfile()
    {
        var profile = ProfileManager.Instance?.currentProfile;
        if (profile == null)
        {
            if (debugJawabanLog)
                Debug.LogWarning("[dontDestroy] SaveJawabanToProfile skipped: currentProfile is null.");
            return;
        }

        if (jawabanMateri != null)
            profile.jawabanMateri = (string[])jawabanMateri.Clone();

        ProfileManager.Instance.SaveProfiles();

        if (debugJawabanLog)
        {
            int len = profile.jawabanMateri != null ? profile.jawabanMateri.Length : 0;
            Debug.Log("[dontDestroy] SaveJawabanToProfile success. Profile='" + profile.playerName + "', answersLen=" + len + ".");
        }
    }

    public void LoadJawabanFromProfile()
    {
        var profile = ProfileManager.Instance?.currentProfile;
        if (profile == null)
        {
            if (debugJawabanLog)
                Debug.LogWarning("[dontDestroy] LoadJawabanFromProfile skipped: currentProfile is null.");
            return;
        }

        jawabanMateri = profile.jawabanMateri != null
            ? (string[])profile.jawabanMateri.Clone()
            : null;

        if (debugJawabanLog)
        {
            int len = jawabanMateri != null ? jawabanMateri.Length : 0;
            Debug.Log("[dontDestroy] LoadJawabanFromProfile success. Profile='" + profile.playerName + "', answersLen=" + len + ".");
        }
    }

    public void LoadFromProfile()
    {
        var profile = ProfileManager.Instance.currentProfile;
        LoadJawabanFromProfile();
    }

}
