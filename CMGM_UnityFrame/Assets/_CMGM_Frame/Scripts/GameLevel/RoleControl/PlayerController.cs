using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public enum E_PlayerDir
    {
        Left,
        Right,
        Back,
        Front
    }
    public E_PlayerDir playerDir { get; private set; } = E_PlayerDir.Right;

    private bool isMousePointerOverGameObjcet;
    //public SkeletonAnimation spineAnim;

    private Vector2 inputDir;
    public float speed;

    private Rigidbody2D rb;
    private RoleDetector2D roleDetector;

    private void Awake()
    {
        //注册角色的引用
        GameSystem.curPlayer = this;


        rb = GetComponent<Rigidbody2D>();
        roleDetector = new RoleDetector2D(gameObject);


        InputManager.Instance.Gameplay.Fire.started += (ctx) => 
        {
            //避免点击UI时触发该事件 (不要在事件中调用EventSystem.current)
            if (isMousePointerOverGameObjcet) return;

            print("atk"); 
        };
        InputManager.Instance.Gameplay.Interact.performed += (ctx) =>
        {
            //触发可交互物的交互事件
            roleDetector.TopInteractableObject?.OnInteracted.Invoke();
        };
    }

    private void OnEnable()
    {
        InputManager.Instance.Gameplay.Enable();
    }
    private void OnDisable()
    {
        InputManager.Instance.Gameplay.Disable();
    }


    private void Update()
    {
        roleDetector.UpdateDetect();


        inputDir = InputManager.Instance.Gameplay.Move.ReadValue<Vector2>();
        isMousePointerOverGameObjcet = EventSystem.current.IsPointerOverGameObject(); //这种赋值方式仅PC端生效，没有考虑多点触控

    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //设置移动
        rb.velocity = inputDir * speed * Time.deltaTime;

        //设置反转，排除等于0的点
        int faceDir = (int)transform.localScale.x;
        if (inputDir.x > 0) faceDir = 1;
        if (inputDir.x < 0) faceDir = -1;
        transform.localScale = new Vector3(faceDir, 1, 1);

        //设置动画排除中间的窗口区
        if (inputDir.magnitude > 0.05f)
        {
            if (inputDir.y > 0.3f)
            {
                playerDir = E_PlayerDir.Back;
                AnimSetDir("Back");
            }
            else if (inputDir.y < -0.3f)
            {
                playerDir = E_PlayerDir.Front;
                AnimSetDir("Front");
            }
            else if (faceDir == 1)
            {
                playerDir = E_PlayerDir.Right;
                AnimSetDir("Side");
            }
            else if (faceDir == -1)
            {
                playerDir = E_PlayerDir.Left;
                AnimSetDir("Side");
            }


            AnimSetState("Walk");
        }
        else
        {
            AnimSetState("Idle");
        }

    }

    private void AnimSetDir(string skinName)
    {
        //if (spineAnim.skeleton.Skin.Name != skinName)
        //    spineAnim.skeleton.SetSkin(skinName);
    }
    private void AnimSetState(string stateName)
    {
        //string skinName = spineAnim.skeleton.Skin.Name;

        //if (spineAnim.state.GetCurrent(0).Animation.Name != skinName + "_" + stateName)
        //    spineAnim.AnimationState.SetAnimation(0, skinName + "_" + stateName, true);
    }


}
