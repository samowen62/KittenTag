using UnityEngine;
using System.Collections;
using System;

public class KittenController : MonoBehaviour {

    public float walkSpeed = 0.05f;
    public float direction_switch_time = 0.3f;
    private float direction_switch_last = 0f;

    //for User input
    private float pushBegin = 0f;

    private const string DIRECTION              = "Direction";
    private const string SPHERE_POINTER         = "SpherePointer";
    private const string PLANE                  = "Plane";
    private const string HIDING_PLACE           = "HidingPlace";
    private const string TALL_BOX               = "TallBox";
    private const string FLAT_BOX               = "FlatBox";
    private const string SPRITE_CHILD           = "Sprite";
    private float COS_45                        = 1 / Mathf.Sqrt(2);

    private DirectionEnum   _currentDirection   = DirectionEnum.IDLE;
    private bool            _current_is_left    = false;
    private bool            _current_is_tall    = false;

    private GameObject      _hiding_place_destination = null;
    private float           _path_end_threshold = 0.25f;

    private Animator animator;
    private SpriteRenderer renderer2d;
    private MeshRenderer spherePointer;
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
        GeneralUtil.Require(spherePointer = GameObject.Find(SPHERE_POINTER).gameObject.GetComponent<MeshRenderer>());


        navAgent.updateRotation = false;
        navAgent.speed = walkSpeed;

        tallCollider.enabled = false;
        flatCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {        
        updateInputs();

        checkAction();

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
                if (hit.transform.tag == PLANE)
                {
                    _hiding_place_destination = null;
                    navAgent.SetDestination(hit.point);

                    //TODO: use pause parent class
                    pushBegin = Time.fixedTime;
                    StartCoroutine(DrawRipple(hit.point));
                } else if (hit.transform.tag == HIDING_PLACE)
                {
                    Debug.Log("hiding");
                    _hiding_place_destination = hit.transform.gameObject;

                    navAgent.SetDestination(_hiding_place_destination.transform.position);
                    pushBegin = Time.fixedTime;

                    //TODO: instead of the ripple try highlighting the object to hide in
                    StartCoroutine(DrawRipple(hit.point));
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

    private void checkAction()
    {
        //TODO:investigate
        //if(_hiding_place_destination != null)
          //  Debug.Log((transform.position - _hiding_place_destination.transform.position).sqrMagnitude);


        if (_hiding_place_destination != null 
            && (transform.localPosition - _hiding_place_destination.transform.position).sqrMagnitude < 1.4f)
        {
            Debug.Log("PlayHideAnimation()");
            navAgent.ResetPath(); 
            //TODO: to hide we will point the player to the destination 
            //object and then play the animation to enter the box
            _hiding_place_destination = null;
        }
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

    IEnumerator DrawRipple(Vector3 center)
    {
        float radius = 0.4f;
        float totalTime = 0.1f;
        float timePassed = Time.fixedTime - pushBegin;
        spherePointer.gameObject.SetActive(true);
        spherePointer.transform.position = center;
        spherePointer.transform.localScale = new Vector3(radius, radius, radius);

        while (timePassed < totalTime)
        {
            radius = Mathf.Lerp(0.1f, 0.3f, timePassed / totalTime);
            spherePointer.transform.localScale = new Vector3(radius, radius, radius);
            timePassed = Time.fixedTime - pushBegin;
            yield return null;
        }

        spherePointer.gameObject.SetActive(false);
        yield return true;
    }
}