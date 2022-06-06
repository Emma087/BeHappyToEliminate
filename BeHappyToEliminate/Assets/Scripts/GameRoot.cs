using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance;

    void Start()
    {
        Instance = this;
        _handleTipsWindow.gameObject.SetActive(true);
        Debug.Log("Game Start ...");
        OpenTipsWindow("Game Start ...");
        InitGameData();
        OpenLobbyWindow();
    }


    #region 关于战斗界面相关，大厅界面变量，打开战斗界面

    public HandleFightWindow FightWindow;

    public void OpenFightWindow()
    {
        FightWindow.gameObject.SetActive(true);
        FightWindow.Init();
        PlayFightBG();
    }

    #endregion


    #region 关于菜单窗口的打开，帮助窗口，暂停窗口，结束窗口

    public MenuWindow MenuWindow;

    public void OpenMenuWidow(OperateType operateType)
    {
        MenuWindow.gameObject.SetActive(true);
        MenuWindow.Init(operateType);
    }

    public void SetFightRun()
    {
        FightWindow.SetFightRun();
    }

    /// <summary>
    /// 退出战斗界面
    /// </summary>
    public void ExitFightWindow()
    {
        //清空所有的战斗数据
        FightWindow.ClearFightData();
        FightWindow.gameObject.SetActive(false);
        PlayLobbyBG();
    }

    public void OnBtnReStart()
    {
        FightWindow.ClearFightData();
        OpenFightWindow();
    }

    #endregion


    #region 关于大厅界面相关，玩家数据创建（金币数量，历史分数），打开界面，刷新玩家数据

    #region 关于玩家金币，历史最高分的声明，和获取数据的属性

    public HandleLobbyWindow LobbyWindow;
    private int mCoin = 0; //玩家总金币数量

    public int GetCoin() // mCoin的 get属性
    {
        return mCoin;
    }

    //更新金币的数量，从外面调用，参数赋给金币总数，然后再存进玩家数据
    public void UpdateCoinData(int coin)
    {
        mCoin = coin;
        PlayerPrefs.SetInt("coin", mCoin);
        LobbyWindow.RefreshCoinData(); //刷新玩家的金币显示
    }


    private string mTime = ""; //历史最高分的创建时间
    private int mScore = 0; //历史最高分

    private int mCurrentScore = 0; //本次的游戏结束的分数
    private bool isNewRecord = false; //是新纪录吗

    //更新分数的历史最高分记录，从外面调用，参数分数赋给分数，然后再存进玩家数据
    public void UpdateRecordData(int score)
    {
        mCurrentScore = score;
        if (mCurrentScore > mScore)
        {
            isNewRecord = true;
            mScore = score;
            var deltaTime = System.DateTime.Now;
            string date = deltaTime.Year + "-" + deltaTime.Month + "-" + deltaTime.ToLongTimeString();
            mTime = date;
            PlayerPrefs.SetInt("score", mScore);
            PlayerPrefs.SetString("time", mTime);
            LobbyWindow.RefreshHistory();
        }
        else
        {
            isNewRecord = false;
        }
    }

    public int GetCurrentScore() //获取本局的分数
    {
        return mCurrentScore;
    }

    public bool GetIsNewRecord()
    {
        return isNewRecord;
    }

    public int GetHistoryScroe() // mScore的 get属性
    {
        return mScore;
    }

    public string GetHistoryScoreTime() // mTime的 get属性
    {
        return mTime;
    }

    #endregion

    void OpenLobbyWindow()
    {
        LobbyWindow.gameObject.SetActive(true);
        LobbyWindow.Init();
        PlayLobbyBG();
    }

    /// <summary>
    /// 检测玩家数据，是否第一次登录，然后更新数据
    /// </summary>
    void InitGameData()
    {
        //如果 玩家不是第一次登录，就 Get到玩家的数据
        if (PlayerPrefs.HasKey("isFirstLogin"))
        {
            mCoin = PlayerPrefs.GetInt("coin");
            mScore = PlayerPrefs.GetInt("score");
            mTime = PlayerPrefs.GetString("time");
        }
        else //如果玩家是第一次登录，给玩家数据，默认值
        {
            PlayerPrefs.SetString("isFirstLogin", "Yes");
            mCoin = 8888;
            mScore = 0;
            mTime = "";
            PlayerPrefs.SetInt("coin", mCoin);
            PlayerPrefs.SetInt("score", mScore);
            PlayerPrefs.SetString("time", mTime);
            OpenTipsWindow("首次登录游戏");
        }
    }

    #endregion


    #region 音效相关

    public AudioSource bgAudio;
    public AudioSource uiAudio;
    public AudioSource effectAudio;

    public AudioClip lobbyBgClip;
    public AudioClip fightBgClip;
    public AudioClip bombClip;
    public AudioClip lightingClip;
    public AudioClip waveClip;
    public AudioClip[] cleanClips;

    public void PlayLobbyBG() //播放大厅界面音效
    {
        if (bgAudio.clip == null || bgAudio.clip.name != lobbyBgClip.name)
        {
            bgAudio.clip = lobbyBgClip;
            bgAudio.loop = true;
            bgAudio.Play();
        }
    }

    public void PlayFightBG() //播放游戏战斗界面音效
    {
        if (bgAudio.clip == null || bgAudio.clip.name != fightBgClip.name)
        {
            bgAudio.clip = fightBgClip;
            bgAudio.loop = true;
            bgAudio.Play();
        }
    }

    public void PlayUIClick() //播放 UI按钮音效
    {
        uiAudio.Play();
    }

    public void PlayClickCubeEffect(int number)
    {
        effectAudio.clip = cleanClips[number];
        effectAudio.Play();
    }

    public void PlayEffectAudio(string name) //播放所有的 点击特效，道具特效 相关的音效
    {
        // //找到对应的是第几下点击，传入名字，循环数组去找，如果能对应上，就播放音效
        // for (int i = 0; i < cleanClips.Length; i++)
        // {
        //     string clipName = cleanClips[i].name;
        //     if (clipName == name)
        //     {
        //         effectAudio.clip = cleanClips[i];
        //         effectAudio.Play();
        //         return;  //播放了点击音效以后，后面的不用执行了，所以 return
        //     }
        // }

        if (name == bombClip.name)
            effectAudio.clip = bombClip;
        else if (name == lightingClip.name)
            effectAudio.clip = lightingClip;
        else if (name == waveClip.name)
            effectAudio.clip = waveClip;
        effectAudio.Play();
    }

    #endregion

    #region 关于播放 Tips的功能

    public HandleTipsWindow _handleTipsWindow;

    /// <summary>
    /// 公共的，播放 Tips功能
    /// </summary>
    public void OpenTipsWindow(string tips)
    {
        _handleTipsWindow.AddTips(tips);
    }

    #endregion
}