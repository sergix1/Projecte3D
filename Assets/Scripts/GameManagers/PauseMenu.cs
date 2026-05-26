using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool showCursorOnPause = true;

    private bool isPaused;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle panelStyle;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D buttonActiveTexture;

    private void Awake()
    {
        ForceResumeState();
    }

    private void Update()
    {
        if (PauseKeyPressed())
            TogglePause();
    }

    private void OnGUI()
    {
        if (!isPaused)
            return;

        EnsureStyles();

        float panelWidth = Mathf.Min(460f, Screen.width - 40f);
        float panelHeight = 390f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight
        );

        DrawOverlay();
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        Rect titleRect = new Rect(panelRect.x + 36f, panelRect.y + 28f, panelRect.width - 72f, 46f);
        Rect subtitleRect = new Rect(panelRect.x + 36f, titleRect.yMax + 6f, panelRect.width - 72f, 28f);
        Rect continueRect = new Rect(panelRect.x + 54f, subtitleRect.yMax + 28f, panelRect.width - 108f, 56f);
        Rect restartRect = new Rect(panelRect.x + 54f, continueRect.yMax + 16f, panelRect.width - 108f, 56f);
        Rect quitRect = new Rect(panelRect.x + 54f, restartRect.yMax + 16f, panelRect.width - 108f, 56f);

        GUI.Label(titleRect, "PAUSA", titleStyle);
        GUI.Label(subtitleRect, "El juego esta detenido", subtitleStyle);

        if (GUI.Button(continueRect, "Continuar", buttonStyle))
            Resume();

        if (GUI.Button(restartRect, "Reiniciar", buttonStyle))
            RestartScene();

        if (GUI.Button(quitRect, "Salir", buttonStyle))
            QuitGame();
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (isPaused)
            return;

        previousLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        isPaused = true;
        UnityEngine.Time.timeScale = 0f;
        AudioListener.pause = true;

        if (showCursorOnPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;
        UnityEngine.Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = previousLockMode;
        Cursor.visible = previousCursorVisible;
    }

    public void RestartScene()
    {
        ForceResumeState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        ForceResumeState();
        Application.Quit();
    }

    private void OnDestroy()
    {
        ForceResumeState();
    }

    private void ForceResumeState()
    {
        isPaused = false;
        UnityEngine.Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private bool PauseKeyPressed()
    {
        if (Input.GetKeyDown(pauseKey))
            return true;

#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private void DrawOverlay()
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 38,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = Color.white }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.black },
            hover = { textColor = Color.black },
            active = { textColor = Color.black },
            focused = { textColor = Color.black }
        };
        buttonTexture = CreateTexture(Color.white);
        buttonHoverTexture = CreateTexture(new Color(0.86f, 0.86f, 0.86f, 1f));
        buttonActiveTexture = CreateTexture(new Color(0.7f, 0.7f, 0.7f, 1f));
        buttonStyle.normal.background = buttonTexture;
        buttonStyle.hover.background = buttonHoverTexture;
        buttonStyle.active.background = buttonActiveTexture;
        buttonStyle.focused.background = buttonHoverTexture;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = CreateTexture(Color.black);
    }

    private Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
