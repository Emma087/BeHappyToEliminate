using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleLobbyWindow : MonoBehaviour
{
    private GameRoot GameRoot;

    public void Init()
    {
        GameRoot = GameRoot.Instance;
        GameRoot.OpenTipsWindow("LobbyWindow Init...");
        RefreshCoinData();
        RefreshHistory();
    }

    #region 关于玩家数据的声明，获取，更新

    public Text txtCoin; //金币数量
    public Text txtHistory; //历史最高分

    public void RefreshCoinData() // 从 GameRoot获取金币的数量信息
    {
        txtCoin.text = GameRoot.GetCoin().ToString();
    }

    public void RefreshHistory() // 从 GameRoot获取历史最高分的数量信息
    {
        string mTime = GameRoot.GetHistoryScoreTime();
        if (mTime != "")
        {
            int mScore = GameRoot.GetHistoryScroe();
            txtHistory.text = "最高分数：" + mScore + "分\n\n记录时间为：" + mTime;
            txtHistory.alignment = TextAnchor.MiddleLeft;
        }
        else
            txtHistory.text = "暂无数据";
    }

    #endregion


    #region 按钮点击事件

    /// <summary>
    /// 在这里打开菜单，一定是帮助窗口
    /// </summary>
    public void OnBtnMenuClicked()
    {
        GameRoot.PlayUIClick();
        GameRoot.OpenMenuWidow(OperateType.Help);
    }

    /// <summary>
    /// 开始玩游戏，点击了以后，进入游戏姐买你
    /// </summary>
    public void OnBtnPracticeClicked()
    {
        GameRoot.PlayUIClick();

        if (GameRoot.GetCoin() >= 500)
        {
            GameRoot.UpdateCoinData(GameRoot.GetCoin() - 500);
            GameRoot.OpenFightWindow();
        }
        else
            GameRoot.OpenTipsWindow("金币不够了");
    }


    //测试按钮，用完删除
    public void OnBtnResetClick()
    {
        PlayerPrefs.DeleteAll();
        GameRoot.OpenTipsWindow("数据清空了");
    }

    #endregion
}