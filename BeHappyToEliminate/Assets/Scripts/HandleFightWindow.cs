using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class HandleFightWindow : MonoBehaviour
{
    private GameRoot gameRoot;
    private bool isGameRun = false; //游戏是否正在运行

    public void Init()
    {
        gameRoot = GameRoot.Instance;
        Debug.Log("Init FightWindow ...");
        InitCUbeData();

        //计时器的时间，初始化
        mCount = fightTime;
        SetTimerBarValue(mCount);
        isGameRun = true;

        //游戏总分的初始化
        mScore = 0;
        SetScoreNumber(mScore, false);

        //技能能量数的初始化
        mSkillPoint = 0;
        SetSkillBalValue(mSkillPoint);
    }


    #region 关于方块的操作，生成新的棋盘所有方格，方格的点击事件，方块的刷新波浪动画，方块是否能被消除，方块消除逻辑，方块下落逻辑，方块新生成补齐空缺

    //存放棋盘上所有的方块
    private CubeItem[,] itemArray = new CubeItem[6, 6];
    public float moveTime; //小方块的移动时间

    public GameObject cubeItemPrefab; //引擎中已经做好的那个 item
    public Transform cubeRootTransform; //引擎中 cube的根节点
    public float xSpace; //这里代表横着每一个格子的间距
    public float ySpace; //这里代表每一个竖着的格子的间距，我看这里最好做一个自适应

    /// <summary>
    /// 生成一个 6x6 的方块矩阵，生成以后，更改图片样式，改每个方格名字，设置位置，添加按钮事件
    /// </summary>
    void InitCUbeData()
    {
        int[,] iconIndexArray = null;
        while (true) //死循环
        {
            //图片的二维数组，根据随机来的数字二维数组，生成
            iconIndexArray = GetRandomArray(6, 6);
            if (IsVaildData(iconIndexArray)) //如果检测到这个二维数组符合规则，那么跳出循环
            {
                break;
            }
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                GameObject cube = Instantiate(cubeItemPrefab);
                CubeItem cubeItem = cube.GetComponent<CubeItem>();
                cubeItem.xIndex = i;
                cubeItem.yIndex = j;

                cubeItem.SetIconData(iconIndexArray[i, j]); //设置糖果 cube的颜色
                cubeItem.name = "item_" + i + "_" + j;
                cubeItem.GetComponent<RectTransform>().SetParent(cubeRootTransform);

                //设置每一个格子的位置（）
                cubeItem.GetComponent<RectTransform>().localPosition = new Vector2(i * xSpace, j * ySpace);

                //给每一个按钮，添加一个按钮事件
                Button btn = cubeItem.GetComponent<Button>();
                // btn.onClick.AddListener(cubeItem.OnItemClicked);
                btn.onClick.AddListener(() => { OnItemClicked(clickItem: cubeItem); });
                itemArray[i, j] = cubeItem;
            }
        }
    }

    private bool isProcess = false; //是否处于点击按钮以后的处理状态，默认 false

    /// <summary>
    /// 棋盘上每一个格子按钮的点击事件
    /// </summary>
    /// <param name="clickItem"></param>
    public void OnItemClicked(CubeItem clickItem)
    {
        if (isProcess || !isGameRun) //点击的时候，如果不是在游戏运行状态，或者正在操作，就返回，不执行下面操作
            return;
        isProcess = true;
        //  Debug.Log("item" + clickItem.name);
        //检测当前点击的小块，是否能消除
        bool canDestory = FindDestoryItem(clickItem);
        if (canDestory)
        {
            //这个数组代表，每一竖行销毁的个数，统计，并且存起来
            int[] createArray = new int[6];
            //可以，消除
            foreach (CubeItem item in canDestroyList)
            {
                itemArray[item.xIndex, item.yIndex] = null;
                createArray[item.xIndex] += 1; //将每一列需要新生成的数量累加
                Destroy(item.gameObject);
            }

            //游戏分数的变化
            mScore += cubeScore * canDestroyList.Count;
            SetScoreNumber(mScore);

            //小方块的数据迁移（下方为空格的 方块的下落）
            MoveRestCubeItem();

            //顶部创建新的 CubeItem
            CreateNewCubeItem(createArray);

            //技能点生效检测
            if (mSkillPoint < skillPointOp) //如果能量数小于 技能要求次数限制
            {
                mSkillPoint += 1;
                if (mSkillPoint >= skillPointOp) //如果能量数，达到了技能要求次数限制
                {
                    mSkillPoint = 0;
                    CreateRandomSkill(); //产生一个随机技能
                }

                SetSkillBalValue(mSkillPoint); //更新能量条显示
            }

            //检测最终数据的有效性
            int[,] iconIndexArray = new int[6, 6];
            foreach (CubeItem item in itemArray)
                iconIndexArray[item.xIndex, item.yIndex] = item.iconIndex;

            if (!IsVaildData(iconIndexArray)) //如果当前的数据不合规（没有一组能点的）
            {
                isGameRun = false; //这里关闭游戏运行状态，是为了给动画播放留出时间，不然也容易出 bug
                // 在协程函数 DelayIconSet中，将此 布尔值更改为 true
                while (true)
                {
                    //重新生成随机数据 
                    iconIndexArray = GetRandomArray(6, 6);
                    if (IsVaildData(iconIndexArray))
                    {
                        break;
                    }
                }

                //写入重新随机数据并且表现随机过程
                StartCoroutine(PlayWaveAnimation(iconIndexArray));
            }
        }
        else
        {
            //不可以，减少能量积攒值
            isProcess = false;
            mSkillPoint -= 2; //能量点减 2
            if (mSkillPoint <= 0)
            {
                mSkillPoint = 0; //如果能量点小于等于 0了，就等于 0
            }

            SetSkillBalValue(mSkillPoint); //更新能量条显示
        }
    }

    /// <summary>
    /// 产生随机技能
    /// </summary>
    void CreateRandomSkill()
    {
        int randomSkillIndex = Random.Range(5, 7); //随即一张技能的图片
        itemArray[Random.Range(0, 6), Random.Range(0, 6)].SetIconData(randomSkillIndex); //在棋盘上随即一张图片，更改为技能
    }

    #region 关于重置棋盘，播放波浪动画表现的逻辑代码

    public float waveSpace; //波浪动画，每一列的间隔时间

    /// <summary>
    /// 播放 重置棋盘所有方块的 波浪表现的动画
    /// </summary>
    IEnumerator PlayWaveAnimation(int[,] iconIndexArray)
    {
        yield return new WaitForSeconds(moveTime + 0.1f); // +0.1是为了视觉上的缓冲
        gameRoot.PlayEffectAudio("wave");
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(waveSpace);
            for (int j = 0; j < 6; j++)
            {
                CubeItem item = itemArray[i, j];
                Animator animator = item.GetComponent<Animator>();
                animator.Play("CubeItemAni");
                RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
                var clips = animatorController.animationClips;
                if (i == 5 && j == 5) //如果到第五个了，就播放一个提示重置得 tips
                    StartCoroutine(DelayIconSet(item, iconIndexArray, clips[0].length / 2, true));
                else
                    StartCoroutine(DelayIconSet(item, iconIndexArray, clips[0].length / 2));
            }
        }
    }

    /// <summary>
    /// 在动画转动一半的时候，更换美术的资源文件
    /// </summary>
    IEnumerator DelayIconSet(CubeItem item, int[,] iconIndexArray, float delay, bool isLast = false)
    {
        yield return new WaitForSeconds(delay);
        item.SetIconData(iconIndexArray[item.xIndex, item.yIndex]); //重新设置图片资源
        if (isLast)
        {
            gameRoot.OpenTipsWindow("重新随机方块");
            isGameRun = true;
        }
    }

    #endregion

    /// <summary>
    /// 生成新的小方块填补上棋盘上的空缺（当消除过后，小方块也下落完成了，就创建新的）
    /// </summary>
    void CreateNewCubeItem(int[] createArray)
    {
        for (int i = 0; i < createArray.Length; i++)
        {
            int count = createArray[i]; //每一列将要生成的格子的总数量
            //从第一个开始创建
            for (int j = 1; j <= count; j++)
            {
                GameObject cube = Instantiate(cubeItemPrefab);
                CubeItem cubeItem = cube.GetComponent<CubeItem>();
                RectTransform rectTransform = cubeItem.GetComponent<RectTransform>();
                cubeItem.xIndex = i; //是哪一列
                int yIndex = 5 - count + j; //这代表从哪一个位置开始生成方块（由下向上）
                cubeItem.yIndex = yIndex;

                cubeItem.SetIconData(Random.Range(0, 5));
                cubeItem.name = "item_" + i + "_" + yIndex;
                rectTransform.SetParent(cubeRootTransform);

                //确定小方块的出现位置，目标位置，然后播放下落动画
                // rectTransform.localPosition = new Vector3(i * xSpace, yIndex * ySpace, 0);
                Vector3 from = new Vector3(i * xSpace, (5 + j) * ySpace, 0);
                Vector3 to = new Vector3(i * xSpace, yIndex * ySpace, 0);
                cubeItem.MovePositionInTime(moveTime, from, to);

                //添加按钮点击事件
                Button btn = cubeItem.GetComponent<Button>();
                btn.onClick.AddListener(() => { OnItemClicked(cubeItem); });

                //更新 itemArray数据
                itemArray[i, yIndex] = cubeItem;
            }
        }

        StartCoroutine(CallAniDoneAction());
    }


    // 协助 CreateNewCubeItem函数，点击按钮以后，消除物体完成以后，将正在处于点击处理状态更改为 false
    IEnumerator CallAniDoneAction()
    {
        yield return new WaitForSeconds(moveTime);
        isProcess = false;
    }


    /// <summary>
    /// 小方块的数据迁移，就是下方为空格的小方块的掉落，一个小细节，需要下落几个单位，其实就是刚刚消除过几个单位，包括动画调用
    /// </summary>
    void MoveRestCubeItem()
    {
        int[,] offsetArray = new int[6, 6]; //这个代表当前棋盘，有几个物体将会被消除，将要消除的格子会记录为 1，不消除的格子记录为 0
        foreach (CubeItem item in canDestroyList)
        {
            for (int y = 0; y < 6; y++)
            {
                if (y > item.yIndex)
                {
                    offsetArray[item.xIndex, y] += 1; //将要消除的格子会记录为 1，不消除的格子记录为 0
                }
            }
        }

        //小块下落逻辑
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                CubeItem item = itemArray[x, y];
                if (item != null && offsetArray[x, y] != 0)
                {
                    //变化位置数据
                    RectTransform rectTransform = item.GetComponent<RectTransform>();
                    Vector3 pos = rectTransform.localPosition;
                    float posY = pos.y - offsetArray[x, y] * ySpace; //这是方块 y轴下落的目标位置，x轴不涉及倒下落，所以不用管
                    // rectTransform.localPosition = new Vector3(pos.x, posY, 0);  //这行是直接设置了位置信息，不行，要用一个动画
                    item.MovePositionInTime(moveTime, pos, new Vector3(pos.x, posY, 0)); //moveTime在引擎内赋值的哦

                    //更新索引，因为 x轴没有变化，所以光减去 y轴偏移量就可以了
                    item.yIndex -= offsetArray[x, y];

                    //更新 itemArray内的数量
                    itemArray[x, y] = null;
                    itemArray[item.xIndex, item.yIndex] = item;
                }
            }
        }
    }


    //存放所有可以消除的方块，的 List
    private List<CubeItem> canDestroyList = new List<CubeItem>();

    public Transform effectRootTransform; //存放所有特效的父节点
    public GameObject cleanEffect; //小方块消除的特效

    /// <summary>
    /// 当前点击的格子按钮，是否是一组能被消除的小块
    /// </summary>
    bool FindDestoryItem(CubeItem rootItem)
    {
        canDestroyList.Clear();
        canDestroyList.Add(rootItem);

        if (rootItem.iconIndex == 5 || rootItem.iconIndex == 6) //点击的是技能的 item
        {
            SelectBySkill(rootItem);
            return true; //如果点击的是技能，后面的就不用执行了
        }

        //存放每轮新查找到的根节点
        List<CubeItem> rootList = new List<CubeItem>();
        rootList.Add(rootItem);

        while (rootList.Count > 0) //如果
        {
            //本轮找到类型一样的 cubeItem物体，不包括本物体哦
            List<CubeItem> FindListThisTime = new List<CubeItem>();
            foreach (CubeItem rootListItem in rootList)
            {
                //CubeItem item = rootListItem;  //这条倒一手，可以不用倒的嘛
                //判断上下左右
                if (rootListItem.yIndex <= 4) //上
                {
                    CubeItem findItem = itemArray[rootListItem.xIndex, rootListItem.yIndex + 1]; //拿出本格子的上面的格子，做一下对比
                    //判断是否为同一类型的 cubeItem
                    if (findItem.IsSameType(rootListItem) && !IsSelectd(findItem))
                    {
                        canDestroyList.Add(findItem);
                        FindListThisTime.Add(findItem);
                    }
                }

                if (rootListItem.yIndex >= 1) //下
                {
                    CubeItem findItem = itemArray[rootListItem.xIndex, rootListItem.yIndex - 1];
                    if (findItem.IsSameType(rootListItem) && !IsSelectd(findItem))
                    {
                        canDestroyList.Add(findItem);
                        FindListThisTime.Add(findItem);
                    }
                }

                if (rootListItem.xIndex >= 1) //左
                {
                    CubeItem findItem = itemArray[rootListItem.xIndex - 1, rootListItem.yIndex];
                    if (findItem.IsSameType(rootListItem) && !IsSelectd(findItem))
                    {
                        canDestroyList.Add(findItem);
                        FindListThisTime.Add(findItem);
                    }
                }

                if (rootListItem.xIndex <= 4) //右
                {
                    CubeItem findItem = itemArray[rootListItem.xIndex + 1, rootListItem.yIndex];
                    if (findItem.IsSameType(rootListItem) && !IsSelectd(findItem))
                    {
                        canDestroyList.Add(findItem);
                        FindListThisTime.Add(findItem);
                    }
                }
            }

            //更新查找根节点
            rootList = FindListThisTime;
        }

        if (canDestroyList.Count >= 3) //如果 可消除List 数量大于等于 3，则说明本次点击可以消除
        {
            //播放特效
            foreach (CubeItem cubeItem in canDestroyList)
            {
                // Vector3 pos = cubeItem.GetComponent<RectTransform>().localPosition;
                GameObject clean = Instantiate(cleanEffect);
                clean.transform.SetParent(effectRootTransform);
                clean.GetComponent<RectTransform>().localPosition =
                    cubeItem.GetComponent<RectTransform>().localPosition;
            }

            //播放音效
            gameRoot.PlayClickCubeEffect(Random.Range(0, 8));
            return true;
        }

        return false;
    }

    /// <summary>
    /// 当前的格子，是否已经包含在 canDestroyList中了
    /// </summary>
    bool IsSelectd(CubeItem item)
    {
        // foreach (CubeItem canDestoryCube in canDestroyList)
        // {
        //     if (item.EqualsItem(canDestoryCube)) //如果传进来的物体，和 canDestroyList某一个重复了，就返回 true
        //     {
        //         return true;
        //     }
        // }
        if (canDestroyList.Contains(item)) //我认为用这种方法更方便，不用从 item方法中转一下了
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 校验棋盘没有不能点的时候，是否符合规则，是否至少有一组能够消除
    /// </summary>
    bool IsVaildData(int[,] indexArray)
    {
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                int val = indexArray[x, y];
                int count = 0;
                //上下左右查找，是否有跟自己一样的值
                if (y <= 4 && val == indexArray[x, y + 1]) // y<=4，说明不是顶层，如果是顶层5，那上面没有物体可查
                    count += 1;
                if (y >= 1 && val == indexArray[x, y - 1])
                    count += 1;
                if (x >= 1 && val == indexArray[x - 1, y])
                    count += 1;
                if (x <= 4 && val == indexArray[x + 1, y])
                    count += 1;

                // count >= 2 说明是有效数据，这里之所以是2，而不是3，是因为还得包括自己哦
                if (count >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 根据二位数组的行数和列数，返回一个充满随机的数字的二维数组
    /// </summary>
    int[,] GetRandomArray(int width, int height)
    {
        int[,] indexArray = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                indexArray[x, y] = Random.Range(0, 5);
            }
        }

        return indexArray;
    }

    #endregion

    private void Update()
    {
        if (isGameRun)
        {
            #region 刷新分数

            deltaTimeCount += Time.deltaTime; //累加 deltaTimeCount时间
            if (deltaTimeCount >= 1) //大于一秒就把总时间 -1，总时间是 int类型
            {
                deltaTimeCount -= 1;
                mCount -= 1;
                if (mCount <= 0) //如果游戏限定的总时间为 0 了，游戏结束
                {
                    isGameRun = false;
                    mCount = 0;
                    GameOver();
                }

                SetTimerBarValue(mCount); //刷新计时器的显示
            }

            #endregion
        }
    }

    #region 关于棋盘中技能的特效播放，还有道具技能消除方块的逻辑

    public GameObject bombEffect; //爆炸的特效
    public GameObject lightingHorizontalEffect; //闪电特效，横着的
    public GameObject lightingVerticalEffect; //闪电特效，竖着的

    /// <summary>
    /// 选中道具物体以后
    /// </summary>
    /// <param name="item"></param>
    void SelectBySkill(CubeItem item)
    {
        int x = item.xIndex;
        int y = item.yIndex;
        switch (item.iconIndex)
        {
            case 5: //炸弹，会炸光上下左右 9个单位包括自己的方块，上，下，左，右，左上，右上，左下，右下
                if (IsCubeInSkillRange(x, y + 1))
                    canDestroyList.Add(itemArray[x, y + 1]);
                if (IsCubeInSkillRange(x, y - 1))
                    canDestroyList.Add(itemArray[x, y - 1]);
                if (IsCubeInSkillRange(x - 1, y))
                    canDestroyList.Add(itemArray[x - 1, y]);
                if (IsCubeInSkillRange(x + 1, y))
                    canDestroyList.Add(itemArray[x + 1, y]);

                if (IsCubeInSkillRange(x - 1, y + 1))
                    canDestroyList.Add(itemArray[x - 1, y + 1]);
                if (IsCubeInSkillRange(x + 1, y + 1))
                    canDestroyList.Add(itemArray[x + 1, y + 1]);
                if (IsCubeInSkillRange(x - 1, y - 1))
                    canDestroyList.Add(itemArray[x - 1, y - 1]);
                if (IsCubeInSkillRange(x + 1, y - 1))
                    canDestroyList.Add(itemArray[x + 1, y - 1]);

                foreach (CubeItem cubeItem in canDestroyList)
                {
                    Vector3 pos = cubeItem.GetComponent<RectTransform>().localPosition;
                    GameObject bomb = Instantiate(bombEffect);
                    bomb.transform.SetParent(effectRootTransform);
                    bomb.GetComponent<RectTransform>().localPosition = pos;
                }

                gameRoot.PlayEffectAudio("bomb");

                break;
            case 6: //闪电，竖着一行的
                for (int i = 0; i < 6; i++)
                {
                    if (i != y) //这个条件是为了排除传进来的那个物体自己，因为不排除就重复了，这个物体在 find函数内就已经被添加了，这里不做重复添加
                        canDestroyList.Add(itemArray[x, i]); // x是传进的，被点击的那个方块，然后以这个为基础的一竖行
                    if (i != x)
                        canDestroyList.Add(itemArray[i, y]); // y是传进的，被点击的那个方块，然后以这个为基础的一横行
                }

                GameObject lightH = Instantiate(lightingHorizontalEffect); //水平闪电的 X轴不变，300就可以，变得只有 Y轴
                lightH.transform.SetParent(effectRootTransform);
                lightH.GetComponent<RectTransform>().localPosition = new Vector3(300, ySpace * y, 0);

                GameObject lightV = Instantiate(lightingVerticalEffect); //垂直闪电的  Y轴不变，305就可以，变得只有 X轴
                lightV.transform.SetParent(effectRootTransform);
                lightV.GetComponent<RectTransform>().localPosition = new Vector3(xSpace * x, 305, 0);

                gameRoot.PlayEffectAudio("lighting");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 判定方块是否在不合规的位置，是否超出了边框
    /// </summary>
    bool IsCubeInSkillRange(int x, int y)
    {
        if (x < 0 || x > 5 || y < 0 || y > 5)
            return false;
        return true;
    }

    #endregion

    #region 技能积攒的能量的 UI显示

    public int skillPointOp; //连续几次操作，才能产生能量的次数限制
    public Image topBarImg; //技能能量条的 UI
    public Image topBarPointStar; //技能条上面表示进度的星星
    private int mSkillPoint; //能量的总和

    /// <summary>
    /// 设置进度条 UI显示相关的功能
    /// </summary>
    void SetSkillBalValue(int point)
    {
        float value = point * 1.0f / skillPointOp;
        topBarImg.fillAmount = value;
        float distance = value * 400 + 70; //400是能量条在引擎中 RectTransform 的宽度，70是能量条距离屏幕边缘的距离
        topBarPointStar.rectTransform.localPosition = new Vector3(distance, -66, 0);
    }

    #endregion

    #region 关于计时器显示相关，倒计时的条 UI变化

    public Text txtTimer; //计时器 UI
    public Image bottomBarImg; //计时器的指条

    private float deltaTimeCount = 0; // Update里面需要用到，将每帧时间累加起来的和
    private int mCount; //这是游戏限定的总时间
    public int fightTime; //战斗花费时间

    /// <summary>
    /// 设置游戏内，倒计时的变更，还有倒计时条的刷新
    /// </summary>
    void SetTimerBarValue(int time)
    {
        float val = time * 1.0f / fightTime;
        bottomBarImg.fillAmount = val;
        txtTimer.text = time + "s";
    }

    #endregion

    #region 游戏分数的显示，分数的动画控制，分数的刷新

    public int cubeScore; //每一个方块，能够获得的分数
    private int mScore; //游戏的总分数

    public Transform numberRoot; //分数的父物体
    public Animator numberAni; //分数父物体的动画控制器

    /// <summary>
    /// 设置游戏内，分数的显示
    /// </summary>
    void SetScoreNumber(int mScore, bool isJumpNumber = true)
    {
        if (isJumpNumber)
        {
            numberAni.Play("NumberRootAni", 0, 0);
            RuntimeAnimatorController controller = numberAni.runtimeAnimatorController;
            var clips = controller.animationClips;
            StartCoroutine(AniPlayDone(clips[0].length / 3, mScore));
        }
        else
        {
            SetPictureNumber(mScore);
        }
    }

    /// <summary>
    /// 协程辅助 SetScoreNumber函数，在动画的 1/3处切换动画的图片
    /// </summary>
    IEnumerator AniPlayDone(float second, int number)
    {
        yield return new WaitForSeconds(second);
        SetPictureNumber(number);
    }

    /// <summary>
    /// 设置对应数字的数字图片
    /// </summary>
    void SetPictureNumber(int num)
    {
        Image[] images = new Image[5]; //因为分数最多也就 5位数
        for (int i = 0; i < numberRoot.childCount; i++)
        {
            Transform transform = numberRoot.GetChild(i);
            images[i] = transform.GetComponent<Image>();
        }

        string numberString = num.ToString(); //分数转成 string
        int len = numberString.Length; //获得分数 string的长度
        string[] numberArray = new string[len]; //创建一个新的 字符串类型数组，用来存放每一个单个的数字
        for (int i = 0; i < len; i++)
        {
            // 存值，每一次，都截取一下字符串的第 i个，截取长度为1，for循环以后会获得所有位数字
            numberArray[i] = numberString.Substring(i, 1);
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (i < numberArray.Length)
            {
                images[i].gameObject.SetActive(true);
                images[i].sprite = Resources.Load<Sprite>("ResImages/Fight/num_" + numberArray[i]);
            }
            else //超出分数长度位数的部分，取消显示
            {
                images[i].gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region 关于点击了菜单按钮以后，游戏帮助界面，游戏暂停界面，游戏暂停逻辑

    /// <summary>
    /// 开启游戏暂停界面，逻辑和显示
    /// </summary>
    public void OnBtnMenuClicked()
    {
        gameRoot.PlayUIClick();
        isGameRun = false;
        gameRoot.OpenMenuWidow(OperateType.Pause);
    }

    public void SetFightRun() //设置游戏运行状态为 true，在 gameRoot中中转调用
    {
        isGameRun = true;
    }

    #endregion


    /// <summary>
    /// 清除本次的所有战斗数据
    /// </summary>
    public void ClearFightData()
    {
        gameRoot = null;
        isProcess = false;
        isGameRun = false;
        deltaTimeCount = 0; //倒计时归零
        mCount = 0;
        mScore = 0;
        mSkillPoint = 0;
        canDestroyList.Clear();
        foreach (Transform gameObject in cubeRootTransform)
        {
            Destroy(gameObject.gameObject);
        }
    }


    //游戏结束
    void GameOver()
    {
        //保存分数数据
        gameRoot.UpdateRecordData(mScore);

        //弹窗
        gameRoot.OpenMenuWidow(OperateType.End);
    }
}