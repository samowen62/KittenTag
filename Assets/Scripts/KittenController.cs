using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
    private const int PLAYER_MASK               = ~(1 << 8);

    private DirectionEnum   _currentDirection   = DirectionEnum.IDLE;
    private bool            _current_is_left    = false;
    private bool            _current_is_tall    = false;

    private HidingPlace     _hiding_place_destination = null;
    private float           _path_end_threshold = 0.25f;
    private bool            hidden = false;
    private bool            canLeave = false;
    public bool isHidden { get { return hidden; } }

    private Animator animator;
    private SpriteRenderer renderer2d;
    private MeshRenderer spherePointer;
    private NavMeshAgent navAgent;
    private BoxCollider tallCollider;
    private BoxCollider flatCollider;

    public bool             isAI = false;
    private float           _ai_latest_poll_time = 0f;
    private float           _ai_latest_click_time = 0f;
    private float           _ai_poll_time = 0.1f;
    private float           _ai_click_time = 1.2f;
    private float           _ai_latest_meander_angle = -1f;
    private KittenController            _ai_kitten_target = null;
    private List<KittenController>      _ai_possible_targets = new List<KittenController>();
    //private to avoid garbage collection
    private Vector3         _ai_search_dir;
    private Ray             _ai_search_ray;
    private RaycastHit      _ai_ray_cast_hit;

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
        GetComponent<Rigidbody>().freezeRotation = true;

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
        if (isAI)
        {
            if(Time.fixedTime - _ai_latest_poll_time > _ai_poll_time)
            {
                _ai_latest_poll_time = Time.fixedTime;

                //Find a target to chase if one exists
                if (_ai_possible_targets.Count > 0)
                {
                    _ai_kitten_target = _ai_possible_targets.Find( e => !e.isHidden);
                } else
                {
                    _ai_kitten_target = null;
                }

                if (_ai_kitten_target != null)
                {
                    _ai_search_ray = new Ray(_ai_kitten_target.transform.position, Vector3.down);
                    moveToPoint(_ai_search_ray);
                }
                else if(Time.fixedTime - _ai_latest_click_time > _ai_click_time)
                {
                    _ai_latest_click_time = Time.fixedTime;
                    //choose random spot. (hiding spot too)

                    int attempts = 10;
                    while (attempts > 0)
                    {
                        //keep range around last used ones
                        float angle;
                        if(_ai_latest_meander_angle == -1)
                        {
                            angle = UnityEngine.Random.Range(0f, 360f);
                        }
                        else
                        {
                            angle = UnityEngine.Random.Range((_ai_latest_meander_angle - 45f) % 360, 
                                (_ai_latest_meander_angle + 45) % 360);
                        }

                        _ai_search_dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
                        _ai_search_ray = new Ray(transform.position, _ai_search_dir);
                        if (!Physics.Raycast(_ai_search_ray, out _ai_ray_cast_hit, 3f, PLAYER_MASK))
                        {
                            _ai_latest_meander_angle = angle;
                            _ai_search_ray = new Ray(Vector3.up + transform.position + 2.2f* _ai_search_dir, Vector3.down);
                            moveToPoint(_ai_search_ray);
                            break;
                        }

                            
                        attempts--;
                    }
         
                    if(attempts == 0)
                    {
                        Debug.Log("hit wall turning around");
                        _ai_latest_meander_angle = -1;
                    }
                }
            }
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            moveToPoint(Camera.main.ScreenPointToRay(Input.mousePosition));
        }
        /*foreach (var touch in Input.touches)
        {

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log(touch.position);
            }
        }*/
    }

    private void moveToPoint(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, PLAYER_MASK) && (!hidden || canLeave))
        {
            if (hit.transform.tag == PLANE)
            {
                //leave hiding place if exists
                if (hidden)
                {
                    Debug.Log("leaving");
                    animator.enabled = true;
                    _hiding_place_destination.leave();
                    _hiding_place_destination = null;
                    hidden = false;
                }

                navAgent.SetDestination(hit.point);

                //TODO: use pause parent class
                pushBegin = Time.fixedTime;
                if (!isAI)
                {
                    StartCoroutine(DrawRipple(hit.point));
                }
            }
            else if (hit.transform.tag == HIDING_PLACE)
            {
                Debug.Log("hiding");
                _hiding_place_destination = hit.transform.GetComponent<HidingPlace>();

                navAgent.SetDestination(_hiding_place_destination.transform.position);
                pushBegin = Time.fixedTime;

                //TODO: instead of the ripple try highlighting the object to hide in
                if (!isAI)
                {
                    StartCoroutine(DrawRipple(hit.point));
                }
            }
        }
    }

    private void checkAction()
    {
        //TODO:investigate
        //if(_hiding_place_destination != null)
        //  Debug.Log((transform.position - _hiding_place_destination.transform.position).sqrMagnitude);


        if (_hiding_place_destination != null) {
            if ((transform.localPosition - _hiding_place_destination.transform.position).sqrMagnitude < 0.2f 
                && !hidden)
            {
                Debug.Log("PlayHideAnimation()");
                _hiding_place_destination.hideIn();
                hidden = true;
                canLeave = false;
                navAgent.ResetPath();
                //TODO: to hide we will point the player to the destination 
                //object and then play the animation to enter the box

                animator.SetInteger(DIRECTION, 3);

                callAfterSeconds(() => {
                    animator.enabled = false;
                    canLeave = true;

                }, 0.7f);
                
            }
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

    private void callAfterSeconds(Action func, float seconds)
    {
        StartCoroutine(callFunc(seconds, func));
    }

    IEnumerator callFunc(float seconds, Action func)
    {
        //TOOD Time.fixedTime -> pauseInvariantTime
        float timeToStop = Time.fixedTime + seconds;
        yield return new WaitUntil(() => timeToStop <= Time.fixedTime);
        func();
    }

    private void OnTriggerEnter(Collider collider) {
    
        switch (collider.tag)
        {
            case "Player":
                KittenController kittenToChase = collider.GetComponent<KittenController>();
                if (isAI && !kittenToChase.isHidden && !_ai_possible_targets.Contains(kittenToChase))
                {
                    Debug.Log(collider.name + " added");
                    _ai_possible_targets.Add(kittenToChase);
                }
                break;

            default:
                break;
        }
        
    }

    private void OnTriggerExit(Collider collider)
    {
        switch (collider.tag)
        {
            case "Player":

                if (isAI && _ai_possible_targets.Contains(collider.GetComponent<KittenController>()))
                {
                    Debug.Log(collider.name + " removed");
                    _ai_possible_targets.Remove(collider.GetComponent<KittenController>());
                }
                break;

            default:
                break;
        }

    }
}