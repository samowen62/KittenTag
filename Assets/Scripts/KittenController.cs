using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

    public float walkSpeed = 0.05f;  

    private const string DIRECTION              = "Direction";
    private const string TALL_BOX               = "TallBox";
    private const string FLAT_BOX               = "FlatBox";
    private const string SPRITE_CHILD           = "Sprite";
    private float COS_45                        = 1 / Mathf.Sqrt(2);

    private DirectionEnum   _currentDirection   = DirectionEnum.SOUTH;
    private bool            _current_is_left    = false;
    private bool            _current_is_tall    = false;

    private Animator animator;
    private SpriteRenderer renderer2d;
    //private Rigidbody rigidBody;
    private NavMeshAgent navAgent;
    private BoxCollider tallCollider;
    private BoxCollider flatCollider;

    // Use this for initialization
    void Start()
    {
        //TODO replace GeneralUtil with asserts
        GeneralUtil.Require(navAgent = GetComponent<NavMeshAgent>());
        GeneralUtil.Require(animator = transform.Find(SPRITE_CHILD).GetComponent<Animator>());
        GeneralUtil.Require(renderer2d = transform.Find(SPRITE_CHILD).GetComponent<SpriteRenderer>());
        
        //GeneralUtil.Require(rigidBody = GetComponent<Rigidbody>());
        //GeneralUtil.Require(rigidBody.freezeRotation = true);

        GeneralUtil.Require(tallCollider = transform.Find(TALL_BOX).gameObject.GetComponent<BoxCollider>());
        GeneralUtil.Require(flatCollider = transform.Find(FLAT_BOX).gameObject.GetComponent<BoxCollider>());

        navAgent.updateRotation = false;
        navAgent.speed = 1f;

        tallCollider.enabled = false;
        flatCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        navAgent.SetDestination(new Vector3(-4, 0, 0));

        moveKitten();
    }

    private void moveKitten()
    {
        //var vertical = Input.GetAxis("Vertical");
        //var horizontal = Input.GetAxis("Horizontal");

        float dir = Vector3.Dot(navAgent.velocity.normalized, Vector3.forward);

        if (dir > COS_45)
        {
            changeDirection(DirectionEnum.SOUTH);
            //rigidBody.velocity = walkSpeed * Vector3.forward;
            flipCollider(true);
        }
        else if (dir <= -COS_45)
        {
            changeDirection(DirectionEnum.NORTH);
            //rigidBody.velocity = walkSpeed * Vector3.back;
            flipCollider(true);
        }
        else
        {
            dir = Vector3.Dot(navAgent.velocity.normalized, Vector3.left);
            if (dir > 0)
            {
                changeDirection(DirectionEnum.SIDE);
                //rigidBody.velocity = walkSpeed * Vector3.left;
                flipDirection(true);
                flipCollider(false);
            }
            else if (dir < 0)
            {
                changeDirection(DirectionEnum.SIDE);
                //rigidBody.velocity = walkSpeed * Vector3.right;
                flipDirection(false);
                flipCollider(false);
            } else
            {
                //rigidBody.velocity = Vector3.zero;
                animator.SetInteger(DIRECTION, (int)DirectionEnum.IDLE);
            }      
        }
    }

    //TODO: have buffer time between switching directions
    //TODO: use this line when Unity's c sharp verison is upgraded
    //[MethodImpl(MethodImplOptions.AggresiveInlining)]
    private void changeDirection(DirectionEnum _direction)
    {
        _currentDirection = _direction;
        animator.SetInteger(DIRECTION, (int)_direction);
    }

    private void flipDirection(bool isLeft)
    {
        if (_current_is_left ^ isLeft)
        {
            renderer2d.flipX = !renderer2d.flipX;
            _current_is_left = isLeft;
        }
    }

    private void flipCollider(bool isTall)
    {
        if (_current_is_tall ^ isTall)
        {
            tallCollider.enabled = isTall;
            flatCollider.enabled = !isTall;
            _current_is_tall = isTall;
        }
    }


}