using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeItem : MonoBehaviour
{
    public int xIndex; //x轴索引信息
    public int yIndex; //y轴索引信息

    public int iconIndex; //是哪一张糖果图片的索引
    public RectTransform rectTransform; //代表自己这个方块的 那个 rect组件

    // /// <summary>
    // /// 之所以不是直接被生成格子时候调用，是因为需要知道点击的是哪一个格子
    // /// </summary>
    // public void OnItemClicked()
    // {
    //     GameRoot.Instance.FightWindow.OnItemClicked(this);
    // }

    #region 动画控制相关，Update函数在这个折叠内

    private float moveTime = 0; //MovePositionInTime函数的，移动的时间保留
    Vector3 moveDirection = Vector3.zero; //移动速度的向量
    private bool isMoveAnimation = false; //是否正在向下落
    private float countTime = 0; //移动状态下的时间累加计数
    private Vector3 tatgetPosition; // MovePositionInTime函数的，目标位置保留

    /// <summary>
    /// 小块移动的一个动画
    /// </summary>
    /// <param name="time">移动的时间</param>
    /// <param name="from">起始的位置</param>
    /// <param name="to">目标的位置</param>
    public void MovePositionInTime(float time, Vector3 from, Vector3 to)
    {
        moveTime = time;
        tatgetPosition = to;
        rectTransform.localPosition = from; //这句是为了防止有误差，所以这样写哦
        float speedX = (to.x - from.x) / moveTime;
        float speedY = (to.y - from.y) / moveTime;
        float speedZ = (to.z - from.z) / moveTime;

        moveDirection = new Vector3(speedX, speedY, speedZ);
        isMoveAnimation = true;
    }

    private void Update()
    {
        if (isMoveAnimation)
        {
            float deltaTime = Time.deltaTime;
            rectTransform.localPosition += deltaTime * moveDirection;
            countTime += deltaTime;
            if (countTime >= moveTime) //如果移动状态下的时间累积总和，等于了移动时间
            {
                //那就强制让当前位置，等于目标位置，因为 update速率和 delTime的问题，防止有偏差
                rectTransform.localPosition = tatgetPosition;

                //这里是重置小块，所有的动画数据
                moveTime = 0;
                countTime = 0;
                tatgetPosition = Vector3.zero;
                moveDirection = Vector3.zero;
                isMoveAnimation = false;
            }
        }
    }

    #endregion

    /// <summary>
    /// 判断，传进来的物体 cubeItem是不是自己，如果是自己，返回 true
    /// </summary>
    public bool EqualsItem(CubeItem item)
    {
        if (item.xIndex == xIndex && item.yIndex == yIndex)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 传进来的参数 cubeItem和自己是否是同一类型，就是图片的颜色是否相同
    /// </summary>
    public bool IsSameType(CubeItem item)
    {
        if (item.iconIndex == iconIndex)
        {
            return true;
        }

        return false;
    }

    #region 关于设置图片资源

    public Image imgIcon; //当前 CubeItem显示的图片

    /// <summary>
    /// 设置 cubeItem显示哪一张图片，0-5是糖果的索引，5和6是道具
    /// </summary>
    public void SetIconData(int index)
    {
        iconIndex = index;

        Sprite sprite = Resources.Load<Sprite>("ResImages/Cubes/cube_" + index);
        imgIcon.sprite = sprite;
    }

    #endregion
}