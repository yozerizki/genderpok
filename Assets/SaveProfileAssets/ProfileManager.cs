using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    public ProfileDataWrapper profileDataWrapper = new ProfileDataWrapper();
    public PlayerProfile currentProfile;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "profiles.json");
            LoadProfiles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadProfiles()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            profileDataWrapper = JsonUtility.FromJson<ProfileDataWrapper>(json);
        }
    }

    public void SaveProfiles()
    {
        string json = JsonUtility.ToJson(profileDataWrapper, true);
        File.WriteAllText(savePath, json);
    }

    public void CreateNewProfile(string name)
    {
        PlayerProfile profile = new PlayerProfile(name);
        profileDataWrapper.profiles.Add(profile);
        currentProfile = profile;
        SaveProfiles();
    }

    public void SelectProfile(string name)
    {
        currentProfile = profileDataWrapper.profiles.Find(p => p.playerName == name);
    }

    public List<PlayerProfile> GetAllProfiles()
    {
        return profileDataWrapper.profiles;
    }

    public void DeleteProfile(string name)
    {
        var profile = profileDataWrapper.profiles.Find(p => p.playerName == name);
        if (profile != null)
        {
            profileDataWrapper.profiles.Remove(profile);

            // If current profile is deleted, clear reference
            if (currentProfile != null && currentProfile.playerName == name)
                currentProfile = null;

            SaveProfiles();
        }
    }


}
