using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private Button startGameButton;
    private Button exitGameButton;

    //images to cycle between
    public Image img_1;
    public Image img_2;

    private const string START_BUTTON = "Start";
    private const string EXIT_BUTTON = "Exit";

    private bool startBlocked = false;
    private AsyncOperation async = null;

    private float scaleSmall = 1f;
    private float scaleLarge = 1.5f;

    private float pushBegin = 0f;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Kitten Tag Game Start!!");
        Button[] buttons = FindObjectsOfType<Button>();

        foreach (var button in buttons)
        {
            switch (button.name)
            {
                case START_BUTTON:
                    startGameButton = button;
                    startGameButton.onClick.AddListener(delegate () { startButtonClicked(); });
                    break;
                case EXIT_BUTTON:
                    exitGameButton = button;
                    exitGameButton.onClick.AddListener(delegate () { endGameButtonClicked(); });
                    break;
                default:
                    Debug.LogWarning("unused button component: " + button.name);
                    break;
            }
        }


    }

    void Update()
    {
        if(((int)Time.fixedTime) % 2 == 0)
        {
            img_1.gameObject.SetActive(false);
            img_2.gameObject.SetActive(true);
        }
        else
        {
            img_1.gameObject.SetActive(true);
            img_2.gameObject.SetActive(false);
        }
    }

    private void startButtonClicked()
    {
        if (!startBlocked)
        {
            startBlocked = true;
            Debug.Log("Started Test");

            pushBegin = Time.fixedTime;
            StartCoroutine(buttonPush());
        }
    }

    /*private void optionsButtonClicked()
    {
        Debug.Log("Options clicked");
    }*/

    public void endGameButtonClicked()
    {
        Debug.Log("Exiting Game");
        Application.Quit();
    }
  

    IEnumerator buttonPush()
    {
        while (Time.fixedTime - pushBegin < 0.3f)
        {
            float scale = fontSizeForSeconds(Time.fixedTime);
            startGameButton.transform.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
             
        SyncLoadLevel(GeneralUtil.TEST_1);
        yield return false;
    }

    private float fontSizeForSeconds(float seconds)
    {
        float timeInSeq = seconds - pushBegin;
        //chop off int part and make into abs() function
        timeInSeq = -Mathf.Abs(timeInSeq - 0.15f) + 0.15f;
        return Mathf.Lerp(scaleSmall, scaleLarge, timeInSeq);
    }

    private void SyncLoadLevel(string levelName)
    {
        Debug.Log("loading");
        async = SceneManager.LoadSceneAsync(levelName);
        StartCoroutine(Load());
    }

    //TODO: fix this!!
    IEnumerator Load()
    {
        Debug.Log("progress: " + async.progress);
        yield return async;
    }

}