using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;


//地面检测：返回角色跳跃、上楼梯、上坡状态
//头顶检测：决定了蹲下的角色是否可以站起来
//视野检测：是否在该角色可视范围内，每个角色有自己的视夹角
//交互检测：是否在可交互范围内
//其他任何需要进行检测的内容都在该类中补充（需要注意2D,3D检测方法不同）
public class RoleDetector2D
{
    private GameObject role;

    /// <summary>
    /// 实例化一个角色检测器必须传入一个角色对象
    /// </summary>
    /// <param name="role">角色对象</param>
    public RoleDetector2D(GameObject role)
    {
        CmgmLog.TODO("该类视野的格挡物检测应添加忽略trigger的逻辑");
        //赋值内部变量的角色对象
        this.role = role;
    }

    //公共属性
    /// <summary>
    /// 角色是否在地面
    /// </summary>
    public bool IsGround { get; private set; }
    /// <summary>
    /// 角色是否能站立
    /// </summary>
    public bool CanStand { get; private set; }

    /// <summary>
    /// 检测到的所有当前能交互的物体
    /// </summary>
    public List<MapInteractable> AllCanInteractObj { get; private set; } = new List<MapInteractable>();
    /// <summary>
    /// 交互范围内优先级最高的可交互物体中，离玩家最近的那个
    /// </summary>
    public MapInteractable TopInteractableObject =>
        AllCanInteractObj
        .Where(target => target.Priority == AllCanInteractObj.Max(target => target.Priority))
        .OrderBy(target => Vector3.Distance(target.transform.position, role.transform.position)).FirstOrDefault();//不在循环中Linq，节约了性能


    /// <summary>
    /// 需要被循环检测的函数，请在角色MonoBehaviour脚本的Update中调用该函数，来启动检测
    /// </summary>
    public void UpdateDetect()
    {
        //可交互物检测
        DetectInteractable();

    }


    #region 内部检测用的私有变量与函数
    //私有变量
    private float VIEW_DISTANCE = 10f;//视野最大距离
    private float VIEW_ANGLE = 120f;//最大视角
    private float TOUCH_DISTANCE = 1.5f;//交互最大距离

    //检测范围内可交互物时使用的层遮罩，暂时为-1即检测一切，以后根据需求修改
    private int _interactableLayerMask = -1;
    //NonAlloc检测需要预先声明数组，以节约性能
    private Collider2D[] targetsTemp = new Collider2D[8];


    /// <summary>
    /// 检测可交互范围内的所有地图交互物
    /// </summary>
    private void DetectInteractable()
    {
        //先清空可交互物列表
        if (AllCanInteractObj.Count > 0) AllCanInteractObj.Clear();


        //球状检测
        int count = Physics2D.OverlapCircleNonAlloc(role.transform.position, TOUCH_DISTANCE, targetsTemp, _interactableLayerMask);

        //对球状检测中得到的对象，逐个判断是否能看到、是否能得到Interactable，可以的存入AllCanInteractObj
        for (int i = 0; i < count; i++)
        {
            if (targetsTemp[i].gameObject == role.gameObject) continue; //排除自身

            var target = targetsTemp[i];

            if (CanSee(target) && target.TryGetComponent(out MapInteractable interactable))
            {
                //Debug.Log($"{role.name}检测到了可交互物{interactable.name}");
                AllCanInteractObj.Add(interactable);
            }
        }
    }



    #endregion

    #region 公共的检测方法

    /// <summary>
    /// 某对象是否能被该角色看到
    /// </summary>
    /// <param name="target">对象的碰撞体</param>
    public bool CanSee(Collider2D target)
    {
        //判断角色collider是否在目标trigger范围內，如果在，就不管视角和格挡物了，直接返回可看到
        //(避免都站到拾取物上了但是朝向不对就捡不到！)
        var isInTrigger = target.bounds.Contains(role.GetComponent<Collider2D>().bounds.ClosestPoint(target.transform.position));   //角色的包围盒离target最近的点，是否在target的包围盒内
        if (isInTrigger)
        {
#if UNITY_EDITOR
            Debug.DrawLine(role.transform.position, target.transform.position, Color.green);
#endif
            return true;
        }


        //物体到角色的距离
        var targetDistance = (target.transform.position - role.transform.position).magnitude;
        //物体是否在距离内
        if (targetDistance > VIEW_DISTANCE) return false;

        //角色指向物体的向量
        var targetDirection = (target.transform.position - role.transform.position).normalized;
        //角色自己的朝向
        Vector3 roleDir = role.transform.up;
        switch (role.gameObject.GetComponent<PlayerController>().playerDir)
        {
            case PlayerController.E_PlayerDir.Left:
                roleDir = -role.transform.right;
                break;
            case PlayerController.E_PlayerDir.Right:
                roleDir = role.transform.right;
                break;
            case PlayerController.E_PlayerDir.Back:
                roleDir = role.transform.up;
                break;
            case PlayerController.E_PlayerDir.Front:
                roleDir = -role.transform.up;
                break;
            default:
                break;
        }

        //物体是否在视角内
        bool isInViewAngle =
            Vector3.Angle(roleDir, targetDirection) <= VIEW_ANGLE / 2;
        if (!isInViewAngle) return false;


        //判断主角与目标之间有无其他collider遮挡。忽略player,TODO:忽略trigger
        //
        RaycastHit2D hitInfo = Physics2D.Raycast(role.transform.position, targetDirection, targetDistance, role.layer);
        //Physics.Raycast(role.transform.position, targetDirection, out hitInfo, targetDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)
        if (hitInfo.transform != null)
        {
            if (hitInfo.transform.GetInstanceID() != target.transform.GetInstanceID())
            {
#if UNITY_EDITOR
                Debug.DrawLine(role.transform.position, hitInfo.point, Color.red);
#endif
                return false;
            }
        }

#if UNITY_EDITOR
        Debug.DrawLine(role.transform.position, target.transform.position, Color.green);
#endif
        return true;


    }

    #endregion
}
