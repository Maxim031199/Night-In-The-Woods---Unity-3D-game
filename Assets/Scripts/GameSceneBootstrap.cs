using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        RuntimeGameState.SetState(RuntimeState.Playing);

        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = false;

        Time.timeScale = 1f;
    }
}
