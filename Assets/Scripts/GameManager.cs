using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    void OnEnable()
    {
        Instance = this;
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayerDead()
    {
        StartCoroutine(PlayerDeadCoroutine());
    }
    private IEnumerator PlayerDeadCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
