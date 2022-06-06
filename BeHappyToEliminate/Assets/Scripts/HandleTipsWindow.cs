using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleTipsWindow : MonoBehaviour
{
    public Image tipsBG;
    public Text txtTips;
    public Animator _animator;

    private Queue<string> tipsQue = new Queue<string>();
    private bool isTispShow = false;

    public void AddTips(string tips)
    {
        tipsQue.Enqueue(tips);
    }

    private void Update()
    {
        if (tipsQue.Count > 0 && isTispShow == false)
        {
            string tips = tipsQue.Dequeue();
            isTispShow = true;
            SetTips(tips);
        }
    }

    private void SetTips(string tips)
    {
        //根据内容长度去调整 bg的大小显示
        int len = tips.Length;
        txtTips.text = tips;
        tipsBG.gameObject.SetActive(true);
        tipsBG.GetComponent<RectTransform>().sizeDelta
            = new Vector2(30 * len + 50, 80);

        //动画控制
        _animator.Play("TipsWindow", 0, 0);
        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        AnimationClip[] clips = controller.animationClips;
        StartCoroutine(AniPlayDone(clips[0].length));
    }

    IEnumerator AniPlayDone(float second)
    {
        yield return new WaitForSeconds(second);
        tipsBG.gameObject.SetActive(false);
        isTispShow = false;
    }
}