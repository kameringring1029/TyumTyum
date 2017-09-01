using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScript.Lang;

public class GameMgr : MonoBehaviour {

    public GameObject tsum;
    public Sprite[] tsumsprite;
    public GameObject selectedmark;
    public GameObject[] scoreTextObj;
    public Sprite[] scoresprite;
    public GameObject feverGauge;
    public GameObject skillGauge;

    public GameObject timerGauge;

    public GameObject msg;
    public GameObject msgtext;

    private GameObject firstTsum;
    private Array removableTsumList;
    private GameObject lastTsum;
    private string currentName;

    
    private int currentScore = 0;

    private int randx=0;

    private Array selectedMarkList;

    private bool isPlaying = false;
    private bool isFever = false;
    public GameObject[] timer;
    private int timeLimit = 30;
    private int countTime = 0;


 
    private void Start()
    {
       // msg.GetComponent<Canvas>().enabled = true;
        StartCoroutine(createTsum(35));
        StartCoroutine(CountDown());
    }

    private void Update()
    {
        if (isPlaying)
        {
            if (Input.GetMouseButton(0) && firstTsum == null)
            {
                OnDragStart();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnDragEnd();
            }
            else if (firstTsum != null)
            {
                OnDragging();
            }
        }
    }

    private IEnumerator createTsum(int num)
    {
        // ループ
        while (num>0)
        {
            // 1秒毎にループします
            yield return new WaitForSeconds(0.02f);
            createTsumChild(num);
            num--;
        }
    }

    private void createTsumChild(int num)
    {
        int randnum = new System.Random().Next(tsumsprite.Length);

        randx++;

        tsum.GetComponent<SpriteRenderer>().sprite = tsumsprite[randnum];
        tsum.name = "Tsum" + randnum;

        Instantiate(tsum, new Vector2(transform.position.x + randx * 0.1f, transform.position.y), transform.rotation);

        randx = new System.Random().Next(40);
    }



    private IEnumerator CountDown()
    {
        int count = countTime;
        while (count > 0)
        {
            int tmpcount = count;
            timer[0].GetComponent<Image>().sprite = scoresprite[(tmpcount % 10)];
            tmpcount /= 10;
            timer[1].GetComponent<Image>().sprite = scoresprite[(tmpcount % 10)];

            yield return new WaitForSeconds(1f);
            count -= 1; //カウントを1つ減らす
        }
        //timerText.text = "Start!";
        msg.GetComponent<Canvas>().enabled = false;
        isPlaying = true;

        StartCoroutine( StartTimer()); //制限時間のカウントを開始
    }

    private IEnumerator StartTimer()
    {
        var count = timeLimit;
        while (count > 0)
        {
            int tmpcount = count;
                timer[0].GetComponent<Image>().sprite = scoresprite[(tmpcount % 10)];
                tmpcount /= 10;
                timer[1].GetComponent<Image>().sprite = scoresprite[(tmpcount % 10)];
            

            // タイマーゲージを細かく減少
            for(int j=0; j<10; j++)
            {
                timerGauge.GetComponent<Image>().fillAmount = (count - j * 0.1f) / (float)timeLimit;
                if (isFever)
                {
                    feverGauge.GetComponent<Image>().fillAmount -= 0.01f;
                    if (feverGauge.GetComponent<Image>().fillAmount <= 0)
                    {
                        isFever = false;
                        feverGauge.GetComponent<Image>().color = new Color(60, 110, 255);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
                        
            count -= 1;


        }

        // タイムアップ
        //timerText.text = "Finish";
        timer[0].GetComponent<Image>().sprite = scoresprite[0];
        timer[1].GetComponent<Image>().sprite = scoresprite[0];
        timerGauge.GetComponent<Image>().fillAmount = 0;
        OnDragEnd();
        isPlaying = false;
        msg.GetComponent<Canvas>().enabled = true;
    }



    private void OnDragStart()
    {
        Collider2D col = GetCurrentHitCollider();
        if (col != null)
        {
            var colObj = col.gameObject;
            if (colObj.name.IndexOf("Tsum") != -1)
            {
                removableTsumList = new Array();
                firstTsum = colObj;
                currentName = colObj.name;
                PushToList(colObj);

                GameObject mark = Instantiate(selectedmark, colObj.transform.position, colObj.transform.rotation);
                selectedMarkList = new Array();
                selectedMarkList.push(mark);
            }
        }
    }


    private void PushToList(GameObject obj)
    {
        lastTsum = obj;
        removableTsumList.push(obj);
        obj.name = "_" + obj.name;
    }


    private Collider2D GetCurrentHitCollider()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        return hit.collider;
    }

    private void OnDragging()
    {
        Collider2D col = GetCurrentHitCollider();
        if (col != null)
        {
            //なにかをドラッグしているとき
            var colObj = col.gameObject;
            if (colObj.name == currentName)
            {
                //現在リストに追加している色と同じ色のボールのとき
                if (lastTsum != colObj)
                {
                    //直前にリストにいれたのと異なるボールのとき
                    var dist = Vector2.Distance(lastTsum.transform.position, colObj.transform.position); //直前のボールと現在のボールの距離を計算
                    if (dist <= 1.5)
                    {
                        //ボール間の距離が一定値以下のとき
                        PushToList(colObj); //消去するリストにボールを追加

                        // 選択マークの設定
                        GameObject mark = Instantiate(selectedmark, colObj.transform.position, colObj.transform.rotation);
                        selectedMarkList.push(mark);
                    }
                }
            }
        }
    }

    private void OnDragEnd()
    {
        if (firstTsum != null)
        {
            //1つ以上のボールをなぞっているとき
            var length = removableTsumList.length;
            if (length >= 3)
            {
                //スキルゲージ蓄積
                GameObject listedTsum = (GameObject)removableTsumList[0];
                if (listedTsum.name == "_Tsum4(Clone)") {
                    skillGauge.GetComponent<Image>().fillAmount += 0.5f;
                    if(skillGauge.GetComponent<Image>().fillAmount >= 1.0f)
                    {
                        skillGauge.GetComponent<Image>().color = new Color(255,255,0);
                    }
                }


                //消去するリストに３個以上ボールがあれば（ボールが三個以上つながっていたら）
                    for (var i = 0; i < length; i++)
                {
                    Destroy((GameObject)removableTsumList[i]); //リストにあるボールを消去
                }
                StartCoroutine(createTsum(length));//消した分ボールを生成

                //現スコア更新
                currentScore += (CalculateBaseScore(length) + 50 * length);

                //フィーバーゲージ蓄積
                if ((feverGauge.GetComponent<Image>().fillAmount < 1.0f) && !isFever)
                {
                    feverGauge.GetComponent<Image>().fillAmount += 0.3f;
                    if(feverGauge.GetComponent<Image>().fillAmount >= 1.0f)
                    {
                        feverGauge.GetComponent<Image>().color = new Color(255, 255, 0);
                        isFever = true;
                    }
                }


                //Score 表示更新
                int tmpscore = currentScore;
                int[] scoretextarray = new int[8];
                for(int i=0; i<7; i++)
                {
                    scoreTextObj[i].GetComponent<SpriteRenderer>().sprite = scoresprite[(tmpscore % 10)];
                    tmpscore /= 10;
                }

            }
            else
            {
                //消去するリストに3個以上ボールがないとき
                for (var j = 0; j < length; j++)
                {
                    GameObject listedTsum = (GameObject)removableTsumList[j];
                    listedTsum.name = listedTsum.name.Substring(1, 5) + "(Clone)"; //Tsumの名前を元に戻す
                }
            }
            firstTsum = null; //変数の初期化


            // 選択済みマークの削除
            for(int i=0; i<selectedMarkList.length; i++)
            {
                Destroy((GameObject)selectedMarkList[i]);
                //selectedMarkList.pop();
            }
        }
    }

    private int CalculateBaseScore(int n)
    {
        int tempScore = 82 * n * (n + 1) - 300;
        return tempScore;
    }
}
