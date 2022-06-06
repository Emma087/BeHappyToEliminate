using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum OperateType
{
    None,
    Help,
    Pause,
    End
}

public class MenuWindow : MonoBehaviour
{
    private GameRoot _gameRoot;

    public Transform helpRoot;
    public Transform pauseRoot;
    public Transform endRoot;

    private OperateType _operateType = OperateType.None;

    public void Init(OperateType operateType)
    {
        _gameRoot = GameRoot.Instance;
        _operateType = operateType;
        switch (_operateType)
        {
            case OperateType.Help:
                helpRoot.gameObject.SetActive(true);
                pauseRoot.gameObject.SetActive(false);
                endRoot.gameObject.SetActive(false);
                break;
            case OperateType.Pause:
                helpRoot.gameObject.SetActive(false);
                pauseRoot.gameObject.SetActive(true);
                endRoot.gameObject.SetActive(false);
                break;
            case OperateType.End:
                helpRoot.gameObject.SetActive(false);
                pauseRoot.gameObject.SetActive(false);
                endRoot.gameObject.SetActive(true);
                SetScoreData();
                break;
            case OperateType.None:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 继续游戏，将返回到上一次的界面中，如果是在大厅打开的，继续就返回大厅，如果是在战斗界面，继续按钮就返回战斗界面
    /// </summary>
    public void OnBtnContinue()
    {
        _gameRoot.PlayUIClick();
        if (_operateType == OperateType.Pause)
            _gameRoot.SetFightRun();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 返回大厅界面的逻辑
    /// </summary>
    public void OnBtnBackLobbyWindow()
    {
        _gameRoot.PlayUIClick();
        gameObject.SetActive(false);
        _gameRoot.ExitFightWindow();
    }

    /// <summary>
    /// 在战斗界面的暂停界面面板上，点击打开帮助界面
    /// </summary>
    public void OnBtnRuleClicked()
    {
        _gameRoot.PlayUIClick();
        helpRoot.gameObject.SetActive(true);
        pauseRoot.gameObject.SetActive(false);
    }

    #region 游戏结束界面相关逻辑

    public Text txtCurrent; //当前分数
    public Text txtHistory; //历史最高分
    public Image imgRecord; //新纪录的美术标识

    
    /// <summary>
    /// 设置结束界面的分数信息等等
    /// </summary>
    void SetScoreData()
    {
        txtCurrent.text = _gameRoot.GetCurrentScore().ToString();
        txtHistory.text = _gameRoot.GetHistoryScroe().ToString();
        if (_gameRoot.GetIsNewRecord())
            imgRecord.gameObject.SetActive(true);
        else
            imgRecord.gameObject.SetActive(false);
    }

    /// <summary>
    /// 再来一次，不离开战斗界面，直接重新开始
    /// </summary>
    public void OnBtnAgain()
    {
        _gameRoot.PlayUIClick();
        gameObject.SetActive(false);
        _gameRoot.OnBtnReStart();
    }

    #endregion
}