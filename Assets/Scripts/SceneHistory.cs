using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHistory : MonoBehaviour
{
    public static SceneHistory Instance { get; private set; }
    
    // A stack remembers order (First In, Last Out)
    private Stack<int> sceneHistory = new Stack<int>();

    private void Awake()
    {
        // Singleton pattern (like your other managers)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep this alive across scenes!
        
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Whenever a new scene loads, remember the OLD one
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't save the history if we are just going back!
        // We handle logic manually in LoadPreviousScene
    }

    public void LoadScene(string sceneName)
    {
        // Before we leave, save the current scene index
        sceneHistory.Push(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadPreviousScene()
    {
        if (sceneHistory.Count > 0)
        {
            // Pop the last scene off the stack and load it
            int previousSceneIndex = sceneHistory.Pop();
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.LogWarning("No previous scene to load!");
            // Optional: Default to Main Menu if history is empty
            // SceneManager.LoadScene("MainMenu");
        }
    }
}