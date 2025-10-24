using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameStateManager : MonoBehaviour
{
    List<GameState> states = new();
    [SerializeField] GameState currentState;
    [SerializeField] GameState defaultState;
    [SerializeField] string menuSceneName = "Menu";
    [SerializeField] string gameSceneName = "GameScene";

    void Awake()
    {
        states.AddRange(GetComponentsInChildren<GameState>());

        GameEvents.OnStateEnter += StateEnter;
        GameEvents.OnGetCurrentState += GetCurrentState;
        SceneManager.sceneLoaded += AnnounceStateOnSceneLoad;
        
        if (currentState == null && defaultState != null)
            GameEvents.OnStateEnter?.Invoke(defaultState);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        // pick a target by scene name and enter it
        if (scene.name == gameSceneName)
            EnterStateByName("InGameState");
        else if (scene.name == menuSceneName)
            EnterStateByName("InMenuState");
    }
    void OnDestroy()
    {
        GameEvents.OnStateEnter -= OnStateEnter;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnStateEnter(GameState s)
    {
        currentState = s;
    }


    private void AnnounceStateOnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        if (currentState == null)
            GameEvents.OnStateEnter?.Invoke(defaultState);
        else
            GameEvents.OnStateEnter?.Invoke(currentState);
    }

    private GameState GetCurrentState()
    {
        return currentState;
    }

    private void StateEnter(GameState state)
    {
        currentState = state;
    }

    public void EnterStateByName(string stateObjectName)
    {
        if (string.IsNullOrEmpty(stateObjectName)) return;
        var target = System.Array.Find(GetComponentsInChildren<GameState>(true),
                                       s => s && s.name == stateObjectName);
        if (target) GameEvents.OnStateEnter?.Invoke(target);
    }
}
