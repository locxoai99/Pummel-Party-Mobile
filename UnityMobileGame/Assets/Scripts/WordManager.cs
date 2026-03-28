using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; }
    public int   scoreToWin   = 5;
    public float newWordDelay = 2f;
    public string[] wordList = {
        "CAT","DOG","RUN","APEX","FIRE","ROCK","GAME","PLAY","FAST",
        "KICK","PUSH","WORD","STAR","BLUE","GOLD","KING","BEAT",
        "WILD","BOLD","HERO","ZEAL","FLUX","GUST","MAZE","JUMP"
    };

    public Text announcerText; // gán bởi WordWarsHUD

    private string currentWord = "";
    public  bool   roundActive = false;
    private bool   gameOver    = false;
    private Dictionary<PlayerWordSpeller,int> scores = new Dictionary<PlayerWordSpeller,int>();
    private List<PlayerWordSpeller> players = new List<PlayerWordSpeller>();

    void Awake() { if(Instance!=null){Destroy(gameObject);return;} Instance=this; }
    void Start()  { StartCoroutine(Init()); }

    IEnumerator Init()
    {
        yield return null;
        foreach(var s in FindObjectsOfType<PlayerWordSpeller>())
        { players.Add(s); scores[s]=0; }
        StartCoroutine(NewRound());
    }

    public IEnumerator NewRound()
    {
        if(gameOver) yield break;
        roundActive=false;
        KeyboardMap.Instance?.ResetAllTiles();
        currentWord = wordList[Random.Range(0,wordList.Length)].ToUpper();
        foreach(var s in players) s.StartNewWord(currentWord);
        Announce("GET READY!", 1.2f);
        yield return new WaitForSeconds(newWordDelay);
        Announce("SPELL:  " + currentWord, 2.5f);
        roundActive=true;
    }

    public void OnWordCompleted(PlayerWordSpeller speller)
    {
        if(!roundActive||gameOver) return;
        roundActive=false;
        scores[speller]++;
        WordWarsHUD.Instance?.UpdateScore(scores[speller]);
        Announce(speller.playerName + "  WINS!  +" + scores[speller], 2f);
        if(scores[speller]>=scoreToWin) StartCoroutine(Winner(speller.playerName));
        else StartCoroutine(NewRound());
    }

    IEnumerator Winner(string name)
    {
        gameOver=true;
        Announce("WINNER: "+name+"!", 5f);
        yield return new WaitForSeconds(5.5f);
        gameOver=false;
        foreach(var s in players) scores[s]=0;
        StartCoroutine(NewRound());
    }

    public void Announce(string msg, float dur)
    {
        if(WordWarsHUD.Instance!=null){WordWarsHUD.Instance.ShowAnnouncer(msg,dur);return;}
        if(!announcerText) return;
        StopCoroutine("HA"); announcerText.text=msg;
        announcerText.gameObject.SetActive(true);
        StartCoroutine(HA(dur));
    }
    IEnumerator HA(float t){yield return new WaitForSeconds(t);if(announcerText)announcerText.gameObject.SetActive(false);}
    public string CurrentWord=>currentWord;
}
