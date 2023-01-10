using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(LevelLoader))]
public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject mainCanvas;
    [SerializeField] Button startButton;
    [SerializeField] TMP_Dropdown noOfPlayers;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI rolledNumber;
    [SerializeField] TextMeshProUGUI currentTurn;
    public Button rollButton;

    LevelLoader levelLoader; 
    private void Awake()
    {
        levelLoader = GetComponent<LevelLoader>();
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => StartGame());
        }
        else
        {
            Debug.LogError("Assign a Start Button!");
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(() => NextLevel());
        }
        else
        {
            Debug.LogError("Assign a Next Button!");
        }

        if (rollButton != null)
        {
            rollButton.onClick.AddListener(() => RollDie());
        }
        else
        {
            Debug.LogError("Assign a Roll Button!");
        }
    }

    private void Update()
    {
        if(levelLoader.GetCurrrentTurn() != null)
            currentTurn.text = "Player " + levelLoader.GetCurrrentTurn().playerId + " Turn";
    }

    void StartGame()
    {
        mainCanvas.SetActive(false);
        nextButton.transform.parent.gameObject.SetActive(true);
        levelLoader.StartGame(noOfPlayers.value);
    }

    void NextLevel()
    {
        levelLoader.NextLevel();
    }

    void RollDie()
    {
        int roll = Random.Range(1, 7);
        rolledNumber.text = roll.ToString();
        levelLoader.EvaluateRoll(roll);
        rollButton.interactable = false;
    }
}
