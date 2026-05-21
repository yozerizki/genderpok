using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scenemanager : MonoBehaviour
{
    AudioSource sfx;
    Animator awan;
    GameObject panelexit;
    bool isChangingScene;

    static readonly int AwanInState = Animator.StringToHash("awanin");
    private void Start()
    {
        awan = GameObject.FindGameObjectWithTag("awan").GetComponent<Animator>();
        awan.Play("awanout");
        sfx = this.gameObject.GetComponent<AudioSource>();
        panelexit = GameObject.FindGameObjectWithTag("panelexit");
        
        if (panelexit != null)
            panelexit.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            exitpressed();
        }
    }

    public void gotohome()
    {
        StartCoroutine(waitandchangescene("scenemulai"));
    }

    public void gotomenu()
    {
        StartCoroutine(waitandchangescene("scenemenu"));
    }

    public void gotoprofileLoad()
    {
        StartCoroutine(waitandchangescene("sceneLoadProfile"));
    }


    public void gotopengembang()
    {
        StartCoroutine(waitandchangescene("scenepengembang"));
    }

    public void gotoengineering()
    {
        StartCoroutine(waitandchangescene("sceneengineering"));
    }

    public void gotomaaterivideo()
    {
        StartCoroutine(waitandchangescene("scenemenuvid"));
    }

    public void gotomateriimage()
    {
        StartCoroutine(waitandchangescene("scenematerigambar"));
    }

    public void gototechnology() {
        StartCoroutine(waitandchangescene("sceneTechnology"));
    }

    public void gotoart() {
        StartCoroutine(waitandchangescene("scenemateriart"));
    }
    public void gotomath() {
        StartCoroutine(waitandchangescene("scenemath"));
    }




    public void restartscene() {
        StartCoroutine(waitandchangescene(SceneManager.GetActiveScene().name));
    }


    public void exitpressed() {
        panelexit.SetActive(true);
    }

    public void cancelexitgame()
    {
        panelexit.SetActive(false);
    }

    public void exitgame()
    {
        //sfx.PlayOneShot(kliksound);
        Application.Quit();
    }

    public string getThissceneName() {
        string a = SceneManager.GetActiveScene().name;
        return a;
    }

    IEnumerator waitandchangescene(string namascene)
    {
        if (isChangingScene)
        {
            yield break;
        }

        isChangingScene = true;
        awan.Play("awanin", 0, 0f);
        awan.Update(0f);
        //Wait Until Sound has finished playing
        while (sfx.isPlaying)
        {
            yield return null;
        }

        while (!AwanIsPlaying())
        {
            yield return null;
        }

        while (AwanIsPlaying())
        {
            yield return null;
        }

        //Audio has finished playing, disable GameObject
        SceneManager.LoadScene(namascene);
    }
    bool AwanIsPlaying()
    {
        AnimatorStateInfo stateInfo = awan.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == AwanInState && stateInfo.normalizedTime < 1f;
    }

}
