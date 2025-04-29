using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 40f;
     float endPositionY = 2376f; // <-- Set this in Inspector or adjust based on your credits height
    [SerializeField] string mainMenuSceneName = "MainMenu"; // <-- Name of your Main Menu scene

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        // Check if the credits have scrolled past the desired Y position
        if (rectTransform.anchoredPosition.y >= endPositionY && gameObject.tag == "CreditNames")
        {
            SceneManager.LoadScene(mainMenuSceneName);
            RoundManager.ROUND_NUMBER = 0;
        }
    }
}
