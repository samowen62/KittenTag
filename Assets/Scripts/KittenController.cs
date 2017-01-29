using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

    public float walkSpeed = 0.05f;
    public float direction_switch_time = 0.3f;
    private float direction_switch_last = 0f;

    private const string DIRECTION              = "Direction";
    private const string TALL_BOX               = "TallBox";
    private const string FLAT_BOX               = "FlatBox";
    private const string SPRITE_CHILD           = "Sprite";
    private float COS_45                        = 1 / Mathf.Sqrt(2);

    private DirectionEnum   _currentDirection   = DirectionEnum.IDLE;
    private bool            _current_is_left    = false;
    private bool            _current_is_tall    = false;

    private Animator animator;
    private SpriteRenderer renderer2d;
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

        GeneralUtil.Require(tallCollider = transform.Find(TALL_BOX).gameObject.GetComponent<BoxCollider>());
        GeneralUtil.Require(flatCollider = transform.Find(FLAT_BOX).gameObject.GetComponent<BoxCollider>());

        navAgent.updateRotation = false;
        navAgent.speed = walkSpeed;

        tallCollider.enabled = false;
        flatCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {        
        updateInputs();

        pointKitten();
    }

    private void updateInputs()
    {

        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform.name == "Plane")
                {
                    Debug.Log(hit.point);
                    navAgent.SetDestination(hit.point);
                }
            }
        }
        /*foreach (var touch in Input.touches)
        {

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log(touch.position);
            }
        }*/
    }

    private void pointKitten()
    {

        float dir = Vector3.Dot(navAgent.velocity.normalized, Vector3.forward);

        if (dir > COS_45)
        {
            changeDirection(DirectionEnum.SOUTH);
            flipCollider(true);
        }
        else if (dir <= -COS_45)
        {
            changeDirection(DirectionEnum.NORTH);
            flipCollider(true);
        }
        else
        {
            dir = Vector3.Dot(navAgent.velocity.normalized, Vector3.left);
            if (dir > 0)
            {
                changeDirection(DirectionEnum.SIDE);
                flipDirection(true);
                flipCollider(false);
            }
            else if (dir < 0)
            {
                changeDirection(DirectionEnum.SIDE);
                flipDirection(false);
                flipCollider(false);
            } else
            {
                changeDirection(DirectionEnum.IDLE);
            }      
        }
    }

    //TODO: use this line when Unity's c sharp verison is upgraded
    //[MethodImpl(MethodImplOptions.AggresiveInlining)]
    private void changeDirection(DirectionEnum _direction)
    {
        if(_direction == DirectionEnum.IDLE)
        {
            _currentDirection = _direction;
            animator.SetInteger(DIRECTION, (int)_direction);
        }
        else if (_currentDirection != _direction)
        {
            if (Time.fixedTime - direction_switch_last > direction_switch_time)
            {
                direction_switch_last = Time.fixedTime;
                _currentDirection = _direction;
                animator.SetInteger(DIRECTION, (int)_direction);
            }
        }
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