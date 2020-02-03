using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    private static GameObject[,] textArea;
    private GameObject[] wordArea;
    private GameObject playAreaPanel, fillerLineSample, levelCompletedPanel;
    private GameObject fillerLine, selectLine, guessText, scoreTimeAddTextArea;
    private GameObject recordNewAreaForLevelCompletedArea, recordOldAreaForLevelCompletedArea;
    private bool wordGuessStartCheck, isWordCross;
    private Vector2 firstTouchPos;    
    private List<string> foundWords;
    private int firstSelectedLetterLocX, firstSelectedLetterLocY;
    private string direction, selectedGameMode;
    private int matrixNumber, currentScore;
    private List<GameObject> fillerLineHolder;
    private float scoreFactorForTimeInterval;
    private Text scoreText, scoreAddText, timeAddText, levelCompletedScoreText;

    public static int timeIntervalBetweenFoundWords;
    public Color[] lineColors = new Color[8];
    public static bool playable;

    private void CreateThePlayArea()
    {
        try
        {
            firstSelectedLetterLocX = -1; firstSelectedLetterLocY = -1; timeIntervalBetweenFoundWords = 0; scoreFactorForTimeInterval = 1;
            foundWords = new List<string>(); playable = true; fillerLineHolder = new List<GameObject>();
            textArea = new GameObject[matrixNumber, matrixNumber];
            wordArea = new GameObject[matrixNumber * 2 - 8];
            for (int t = 0; t < matrixNumber * 2 - 8; t++)
            {
                wordArea[t] = GameObject.Find("Word" + (t + 1).ToString());
            } //Assign wordArea
            GameObject textAreaSample = GameObject.Find("TextAreaSample");
            textAreaSample.GetComponent<RectTransform>().localScale = new Vector2((float)280 / (matrixNumber - 1) / 35, (float)280 / (matrixNumber - 1) / 35);
            string[] levelInfo = PuzzleCreateController.finalLevelInfo.Split(',');
            int letterCounter = 0;
            for (int i = 0; i < matrixNumber; i++)
            {
                for (int j = 0; j < matrixNumber; j++)
                {
                    textArea[i, j] = Instantiate(textAreaSample, playAreaPanel.transform);
                    textArea[i, j].GetComponent<RectTransform>().localPosition = new Vector2((280 / (matrixNumber - 1) * j) - 140, (140 - (280 / (matrixNumber - 1) * i)) + 10);
                    textArea[i, j].transform.GetChild(0).GetComponent<Text>().text = levelInfo[letterCounter].TrimEnd(new char[] { '\r', '\n' });
                    textArea[i, j].transform.GetChild(0).GetComponent<Animation>().Play("PuzzleTextLoadEntry");
                    letterCounter += 1;
                }
            }
            for (int k = 0; k < matrixNumber * 2 - 8; k++)
            {
                wordArea[k].transform.GetChild(0).GetComponent<Animation>().Stop("WordFoundDrawLine");
                wordArea[k].transform.GetChild(0).GetComponent<Image>().fillAmount = 0; //Reset Fill Amount           
                wordArea[k].GetComponent<Text>().text = levelInfo[(int)Mathf.Pow(matrixNumber, 2) + k].TrimEnd(new char[] { '\r', '\n' });
                float foundLineScale = 0.6f + (levelInfo[(int)Mathf.Pow(matrixNumber, 2) + k].TrimEnd(new char[] { '\r', '\n' }).Length - 4) * 0.08f;
                wordArea[k].transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector2(foundLineScale, foundLineScale);
                wordArea[k].GetComponent<Animation>().Play("PuzzleTextLoadEntry");
            }
            if (selectedGameMode == "Zamana Karşı")
            {
                PuzzleCreateController puzzleCreateController = new PuzzleCreateController();
                StartCoroutine(puzzleCreateController.CreateThePuzzle(PlayerPrefs.GetString("selectedGameMode"), PlayerPrefs.GetInt("selectedMatrix"), PlayerPrefs.GetString("selectedCategory")));
            } //Load Next Puzzle Info For "Zamana Karşı" Mode
        }
        catch
        {
            GetComponent<GamePageButtonsController>().ErrorPanelOpen();
        }
    }

    private void GamePlayControl(bool playable)
    {
        if (Input.touchCount == 1 && playable == true)
        {
            Touch currentTouch = Input.GetTouch(0);
            Vector3 currentTouchPosRaw = Camera.main.ScreenToWorldPoint(currentTouch.position);
            Vector2 currentTouchPos = new Vector2(currentTouchPosRaw.x, currentTouchPosRaw.y);

            float cosFactor = 1;
            string guessTextString = "";

            if (currentTouch.phase == TouchPhase.Began)
            {
                for (int i = 0; i < matrixNumber; i++)
                {
                    for (int j = 0; j < matrixNumber; j++)
                    {
                        if (textArea[i, j].GetComponent<CircleCollider2D>() == Physics2D.OverlapPoint(currentTouchPos))
                        {
                            fillerLine = Instantiate(fillerLineSample, playAreaPanel.transform);
                            fillerLine.GetComponent<RectTransform>().localPosition = textArea[i, j].GetComponent<RectTransform>().localPosition;
                            fillerLine.GetComponent<RectTransform>().localScale = new Vector2((float)140 / (matrixNumber - 1) / 17.5f, (float)140 / (matrixNumber - 1) / 17.5f);
                            selectLine = fillerLine.transform.GetChild(0).gameObject;
                            int selectedLineColor = UnityEngine.Random.Range(0, lineColors.Length); // Line Color Pick
                            selectLine.GetComponent<Image>().color = lineColors[selectedLineColor]; fillerLine.GetComponent<Image>().color = lineColors[selectedLineColor]; // Line Color Pick
                            wordGuessStartCheck = true;
                            firstTouchPos = textArea[i, j].transform.position;
                            firstSelectedLetterLocX = i; firstSelectedLetterLocY = j;
                            guessTextString += textArea[i, j].transform.GetChild(0).GetComponent<Text>().text;
                            guessText.GetComponent<Text>().text = guessTextString;
                            break;
                        }
                    }
                    if (wordGuessStartCheck == true) { break; }
                }
            }

            else if (currentTouch.phase == TouchPhase.Ended && wordGuessStartCheck == true)
            {
                bool checkIsGuessCorrect = false;
                bool guessAlreadyFound = false;
                int whichWordFound = -1;
                for (int i = 0; i < matrixNumber * 2 - 8; i++)
                {
                    if (guessText.GetComponent<Text>().text == wordArea[i].GetComponent<Text>().text)
                    {
                        for (int k = 0; k < foundWords.Count; k++)
                        {
                            if (guessText.GetComponent<Text>().text == foundWords[k])
                            {
                                guessAlreadyFound = true;
                                break;
                            }
                        }
                        if (guessAlreadyFound == false)
                        {
                            checkIsGuessCorrect = true;
                            whichWordFound = i;
                            foundWords.Add(wordArea[whichWordFound].GetComponent<Text>().text);
                            wordArea[whichWordFound].transform.GetChild(0).GetComponent<Animation>().Play("WordFoundDrawLine");
                            fillerLineHolder.Add(fillerLine);
                            ScoreControl(wordArea[whichWordFound].GetComponent<Text>().text); //For Testing
                            if (foundWords.Count == matrixNumber * 2 - 8) { PuzzleCompleted(); } //Check Is Puzzle Completed
                        }
                        break;
                    }
                }
                if (checkIsGuessCorrect == true)
                {
                    if (isWordCross == false)
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3((wordArea[whichWordFound].GetComponent<Text>().text.Length * 2) - 1, 1, 1);
                    }
                    else
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3(1 + ((wordArea[whichWordFound].GetComponent<Text>().text.Length * 2 - 2) * Mathf.Sqrt(2)), 1, 1);
                    }
                } //Set selectLine
                else
                {
                    Destroy(fillerLine);
                } //Guess Is Not Correct
                wordGuessStartCheck = false;
                firstSelectedLetterLocX = -1; firstSelectedLetterLocY = -1;
                guessText.GetComponent<Text>().text = guessTextString;               
            }

            else if (wordGuessStartCheck == true)
            {
                Vector2 changedCurrentTouchPos = currentTouchPos;

                direction = "";
                double angle = Math.Atan2(changedCurrentTouchPos.y - firstTouchPos.y, changedCurrentTouchPos.x - firstTouchPos.x) * 180 / Math.PI;
                if (angle > 22.5f && angle < 67.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 45); direction = "northEast"; }
                else if (angle > 67.5f && angle < 112.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90); direction = "north"; }
                else if (angle > 112.5f && angle < 157.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 135); direction = "northWest"; }
                else if (angle > 157.5f || angle < -157.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180); direction = "west"; }
                else if (angle > -157.5f && angle < -112.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -135); direction = "southWest"; }
                else if (angle > -112.5f && angle < -67.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -90); direction = "south"; }
                else if (angle > -67.5f && angle < -22.5f) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -45); direction = "southEast"; }
                else if (angle > -22.5f || angle < 22.5) { fillerLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0); direction = "east"; }

                int flx = firstSelectedLetterLocX;
                int fly = firstSelectedLetterLocY;
                int letterNumberReachToEnd = 0;
                isWordCross = false;
                switch (direction)
                {
                    case "north":
                        cosFactor = 1; changedCurrentTouchPos = new Vector2(firstTouchPos.x, changedCurrentTouchPos.y);
                        for (int i = flx, t = i - matrixNumber; i > t; i--)
                        {
                            if (i >= 0) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        break;
                    case "northEast":
                        cosFactor = Mathf.Cos((float)(45 - angle) * Mathf.Deg2Rad);
                        for (int i = flx, k = fly, t = k + matrixNumber; k < t; k++, i--)
                        {
                            if (i >= 0 && k <= matrixNumber - 1) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        isWordCross = true;
                        break;
                    case "east":
                        cosFactor = 1; changedCurrentTouchPos = new Vector2(changedCurrentTouchPos.x, firstTouchPos.y);
                        for (int k = fly, t = k + matrixNumber; k < t; k++)
                        {
                            if (k <= matrixNumber - 1) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        break;
                    case "southEast":
                        cosFactor = Mathf.Cos((float)(45 + angle) * Mathf.Deg2Rad);
                        for (int i = flx, k = fly, t = k + matrixNumber; k < t; k++, i++)
                        {
                            if (i <= matrixNumber - 1 && k <= matrixNumber - 1) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        isWordCross = true;
                        break;
                    case "south":
                        cosFactor = 1; changedCurrentTouchPos = new Vector2(firstTouchPos.x, changedCurrentTouchPos.y);
                        for (int i = flx, t = i + matrixNumber; i < t; i++)
                        {
                            if (i <= matrixNumber - 1) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        break;
                    case "southWest":
                        cosFactor = Mathf.Cos((float)(135 + angle) * Mathf.Deg2Rad);
                        for (int i = flx, k = fly, t = i + matrixNumber; i < t; k--, i++)
                        {
                            if (i <= matrixNumber - 1 && k >= 0) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        isWordCross = true;
                        break;
                    case "west":
                        cosFactor = 1; changedCurrentTouchPos = new Vector2(changedCurrentTouchPos.x, firstTouchPos.y);
                        for (int k = fly, t = k - matrixNumber; k > t; k--)
                        {
                            if (k >= 0) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        break;
                    case "northWest":
                        cosFactor = Mathf.Cos((float)(135 - angle) * Mathf.Deg2Rad);
                        for (int i = flx, k = fly, t = k - matrixNumber; k > t; k--, i--)
                        {
                            if (i >= 0 && k >= 0) { letterNumberReachToEnd += 1; }
                            else { break; }
                        }
                        isWordCross = true;
                        break;
                }

                int selectedLettersCounter;
                if (isWordCross == false)
                {
                    float selectLineScaleAmount = (float)Math.Sqrt(Math.Pow(changedCurrentTouchPos.y - firstTouchPos.y, 2) + Math.Pow(changedCurrentTouchPos.x - firstTouchPos.x, 2))
                            * ((float)16 * (matrixNumber - 1) / 35) * cosFactor;
                    if (selectLineScaleAmount > (letterNumberReachToEnd * 2) - 1)
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3((letterNumberReachToEnd * 2) - 1, 1, 1);
                        selectedLettersCounter = letterNumberReachToEnd;
                    }
                    else
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3(selectLineScaleAmount, 1, 1);
                        selectedLettersCounter = 1 + (int)Math.Round((selectLineScaleAmount - 1) / 2);
                    }
                }
                else
                {
                    float selectLineScaleAmount = (float)Math.Sqrt(Math.Pow(changedCurrentTouchPos.y - firstTouchPos.y, 2) + Math.Pow(changedCurrentTouchPos.x - firstTouchPos.x, 2))
                            * ((float)16 * (matrixNumber - 1) / 35) * cosFactor;
                    if (selectLineScaleAmount > 1 + ((letterNumberReachToEnd * 2 - 2) * Mathf.Sqrt(2)))
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3(1 + ((letterNumberReachToEnd * 2 - 2) * Mathf.Sqrt(2)), 1, 1);
                        selectedLettersCounter = letterNumberReachToEnd;
                    }
                    else
                    {
                        selectLine.GetComponent<RectTransform>().localScale = new Vector3(selectLineScaleAmount, 1, 1);
                        selectedLettersCounter = (int)Math.Round((((selectLineScaleAmount - 1) / Mathf.Sqrt(2)) + 2) / 2);
                    }
                }

                switch (direction)
                {
                    case "north":
                        for (int i = flx, t = i, z = 0; i + selectedLettersCounter > t; i--, z++)
                        {
                            guessTextString += textArea[i, fly].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "northEast":
                        for (int i = flx, k = fly, t = k + selectedLettersCounter, z = 0; k < t; k++, i--, z++)
                        {
                            guessTextString += textArea[i, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "east":
                        for (int k = fly, t = k + selectedLettersCounter, z = 0; k < t; k++, z++)
                        {
                            guessTextString += textArea[flx, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "southEast":
                        for (int i = flx, k = fly, z = 0, t = k + selectedLettersCounter; k < t; k++, i++, z++)
                        {
                            guessTextString += textArea[i, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "south":
                        for (int i = flx, t = i + selectedLettersCounter, z = 0; i < t; i++, z++)
                        {
                            guessTextString += textArea[i, fly].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "southWest":
                        for (int i = flx, k = fly, t = i + selectedLettersCounter, z = 0; i < t; k--, i++, z++)
                        {
                            guessTextString += textArea[i, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "west":
                        for (int k = fly, t = k, z = 0; k + selectedLettersCounter > t; k--, z++)
                        {
                            guessTextString += textArea[flx, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                    case "northWest":
                        for (int i = flx, k = fly, t = k, z = 0; k + selectedLettersCounter > t; k--, i--, z++)
                        {
                            guessTextString += textArea[i, k].transform.GetChild(0).GetComponent<Text>().text;
                        }
                        break;
                }
                guessText.GetComponent<Text>().text = guessTextString;
            }
        }
    }

    private void PuzzleCompleted()
    {
        if(selectedGameMode == "Zamana Karşı")
        {         
            for (int i = 0; i < matrixNumber; i++)
            {
                for (int j = 0; j < matrixNumber; j++)
                {
                    Destroy(textArea[i, j]);
                }
            } //Reset Letters
            for(int t = 0; t < fillerLineHolder.Count; t++) { Destroy(fillerLineHolder[t]); } //Reset FillerLines
            CreateThePlayArea();
        }
        else
        {
            SaveBestScoreAndEditLevelCompletedPanel(selectedGameMode);
            playable = false;
            levelCompletedPanel.GetComponent<Animation>().Play("LevelCompletedPanelOpen");
        }
    }

    public void TimeIsUp()
    {
        SaveBestScoreAndEditLevelCompletedPanel(selectedGameMode);
        playable = false;
        levelCompletedPanel.GetComponent<Animation>().Play("LevelCompletedPanelOpen");
    }
 
    public void ScoreControl(string lastFoundWord)
    {
        //Set Score
        float scoreFactorForMatrix = 1 + (matrixNumber - 9) * 0.25f;
        int scoreFactorForWordLength = 10 + (lastFoundWord.Length - 4) * 5;
        if (timeIntervalBetweenFoundWords <= 5) { scoreFactorForTimeInterval += 0.5f; }
        else { scoreFactorForTimeInterval = 1; }
        int scoreToBeAdded = (int)scoreFactorForMatrix * scoreFactorForWordLength * (int)scoreFactorForTimeInterval;
        int timeToBeAdded = (int)scoreFactorForMatrix * scoreFactorForWordLength * (int)scoreFactorForTimeInterval / 5;
        currentScore += scoreToBeAdded;
        scoreText.text = currentScore.ToString();
        timeIntervalBetweenFoundWords = 0;
        //Set Time
        if(selectedGameMode == "Zamana Karşı")
        {
            TimerController.secondsCount += timeToBeAdded; //Add Time
            GetComponent<TimerController>().SetTimeWhenAddSeconds();
            //Add Score Text And Add Time Text Animation
            scoreAddText.text = "+" + scoreToBeAdded.ToString() + "p";
            timeAddText.text = "+" + timeToBeAdded.ToString() + "s";
            scoreTimeAddTextArea.GetComponent<Animation>().Play("ScoreAndTimeAddText");
        }
        else
        {
            //Add Score Text And Add Time Text Animation
            scoreAddText.text = "+" + scoreToBeAdded.ToString() + "p";
            scoreTimeAddTextArea.GetComponent<Animation>().Play("ScoreAndTimeAddText");
        }
    }

    private void SaveBestScoreAndEditLevelCompletedPanel(string selectedGameMode)
    {
        levelCompletedScoreText.text = currentScore.ToString();
        int bestScore;
        if (selectedGameMode == "Zamana Karşı") { bestScore = int.Parse(EncryptionController.Decrypt(PlayerPrefs.GetString("bestScoreOfAgainstTime"))); }
        else { bestScore = int.Parse(EncryptionController.Decrypt(PlayerPrefs.GetString("bestScoreOfNormal"))); }        
        if (currentScore > bestScore)
        {
            recordOldAreaForLevelCompletedArea.SetActive(false);
            recordNewAreaForLevelCompletedArea.SetActive(true);
            if (selectedGameMode == "Zamana Karşı")
            {
                PlayerPrefs.SetString("bestScoreOfAgainstTime", EncryptionController.Encrypt(currentScore.ToString()));
                if (EncryptionController.Decrypt(PlayerPrefs.GetString("removeAdsPurchased")) == "No")
                {
                    StartCoroutine(AdsAdMobController.InstanceAdsAdMobController.LoadInterstitialAd());
                } // Show InterstitialAd
                PlayServicesController.AddScoreToLeaderboard(GPGSIds.leaderboard_lider_tablosu__zamana_kar, currentScore);
            }
            else
            {
                PlayerPrefs.SetString("bestScoreOfNormal", EncryptionController.Encrypt(currentScore.ToString()));
                if (EncryptionController.Decrypt(PlayerPrefs.GetString("removeAdsPurchased")) == "No")
                {
                    StartCoroutine(AdsAdMobController.InstanceAdsAdMobController.LoadInterstitialAd());
                } // Show InterstitialAd
                PlayServicesController.AddScoreToLeaderboard(GPGSIds.leaderboard_lider_tablosu__normal_mod, currentScore);
            }
        }
        else
        {
            recordOldAreaForLevelCompletedArea.SetActive(true);
            recordNewAreaForLevelCompletedArea.SetActive(false);
            recordOldAreaForLevelCompletedArea.GetComponent<Text>().text = "Rekorunuz : " + bestScore.ToString();
        }
    }

    private void Awake()
    {
        fillerLineSample = GameObject.Find("FillerLineSample");
        playAreaPanel = GameObject.Find("LettersPanel");
        guessText = GameObject.Find("GuessText");
        levelCompletedPanel = GameObject.Find("LevelCompletedPanel");
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        scoreAddText = GameObject.Find("ScoreAddText").GetComponent<Text>();
        timeAddText = GameObject.Find("TimeAddText").GetComponent<Text>();
        scoreTimeAddTextArea = GameObject.Find("ScoreTimeAddTextArea");
        recordNewAreaForLevelCompletedArea = GameObject.Find("NewRecord");
        recordOldAreaForLevelCompletedArea = GameObject.Find("OldRecord");
        levelCompletedScoreText = GameObject.Find("LevelCompletedScoreText").GetComponent<Text>();
        selectedGameMode = PlayerPrefs.GetString("selectedGameMode");
        currentScore = 0;
        matrixNumber = PuzzleCreateController.matrixNumber;
        CreateThePlayArea();
    }

    private void Update()
    {
        GamePlayControl(playable);
    }
}