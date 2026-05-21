using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class ProfileSelectUI : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public Button confirmButton;
    public Button createButton;
    public Button cancelButton;
    public GameObject profileItemPrefab;
    public RectTransform contentPanel;
    public Transform createprofilePanel;
    public Button clearAllButton;
    public ScrollRect srobject;

    private void Start()
    {
        
        RefreshProfileList();
        StartCoroutine(paksanaik());
        createButton.onClick.AddListener(() =>
        {
            createprofilePanel.gameObject.SetActive(true);
        });
        cancelButton.onClick.AddListener(() =>
        {
            playerNameInput.text = "";
            createprofilePanel.gameObject.SetActive(false);
            RefreshProfileList();
        });

        confirmButton.onClick.AddListener(() =>
        {
            string newName = playerNameInput.text.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                
                ProfileManager.Instance.CreateNewProfile(newName);
                playerNameInput.text = "";
                createprofilePanel.gameObject.SetActive(false);
                RefreshProfileList();
            }
        });
    }

    void RefreshProfileList()
    {
        // Clear old items
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        List<PlayerProfile> profiles = ProfileManager.Instance.GetAllProfiles();
        foreach (PlayerProfile profile in profiles)
        {
            GameObject item = Instantiate(profileItemPrefab, contentPanel);

            TMP_Text teks = item.GetComponentInChildren<TMP_Text>();
            teks.text = profile.playerName;
            

            Button[] buttons = item.GetComponentsInChildren<Button>();

            // 🔒 Ini penting: salin profile ke variabel lokal
            PlayerProfile capturedProfile = profile;

            buttons[0].onClick.AddListener(() =>
            {
                ProfileManager.Instance.SelectProfile(capturedProfile.playerName);
                dontDestroy.Instance.LoadFromProfile();
                FindObjectOfType<scenemanager>().gotomenu();
            });

            if (buttons.Length > 1)
            {
                buttons[1].onClick.AddListener(() =>
                {
                    ProfileManager.Instance.DeleteProfile(capturedProfile.playerName);
                    RefreshProfileList();
                });
            }
        }
    }

    public void createprofile()
    {
        createprofilePanel.gameObject.SetActive(true);
    }

    public void cancelcreateplayer()
    {
        playerNameInput.text = "";
        createprofilePanel.gameObject.SetActive(false);
        RefreshProfileList();
    }
    public void confirmcreateplayer()
    {
        string newName = playerNameInput.text.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            ProfileManager.Instance.CreateNewProfile(newName);
            playerNameInput.text = "";
            createprofilePanel.gameObject.SetActive(false);
            RefreshProfileList();
        }
    }
    IEnumerator paksanaik()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
        yield return null;
        srobject.verticalNormalizedPosition = 1f;
    }
}