using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

    public float walkSpeed = 0.05f;  

    private const string DIRECTION              = "Direction";
    private const string TALL_BOX               = "TallBox";
    private const string FLAT_BOX               = "FlatBox";

    private DirectionEnum   _currentDirection   = DirectionEnum.SOUTH;
    private bool            _current_is_left    = false;
    private bool            _current_is_tall    = false;

    private Animator animator;
    private Rigidbody2D rigidBody;
    private BoxCollider2D tallCollider;
    private BoxCollider2D flatCollider;

    // Use this for initialization
    void Start()
    {
        GeneralUtil.Require(animator = GetComponent<Animator>());

        GeneralUtil.Require(rigidBody = GetComponent<Rigidbody2D>());
        GeneralUtil.Require(rigidBody.freezeRotation = true);

        GeneralUtil.Require(tallCollider = transform.Find(TALL_BOX).gameObject.GetComponent<BoxCollider2D>());
        GeneralUtil.Require(flatCollider = transform.Find(FLAT_BOX).gameObject.GetComponent<BoxCollider2D>());
    }

    // Update is called once per frame
    void Update()
    {
        moveKitten();
    }

    private void moveKitten()
    {
        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");

        if (vertical > 0)
        {
            changeDirection(DirectionEnum.SOUTH);
            rigidBody.velocity = walkSpeed * Vector2.up;
            flipCollider(true);
        }
        else if (vertical < 0)
        {
            changeDirection(DirectionEnum.NORTH);
            rigidBody.velocity = walkSpeed * Vector3.down;
            flipCollider(true);
        }
        else if (horizontal < 0)
        {
            changeDirection(DirectionEnum.SIDE);
            rigidBody.velocity = walkSpeed * Vector3.left;
            flipDirection(true);
            flipCollider(false);
        }
        else if (horizontal > 0)
        {
            changeDirection(DirectionEnum.SIDE);
            rigidBody.velocity = walkSpeed * Vector3.right;
            flipDirection(false);
            flipCollider(false);
        }
        else
        {
            rigidBody.velocity = Vector3.zero;
            animator.SetInteger(DIRECTION, (int)DirectionEnum.IDLE);
        }
    }

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
            transform.Rotate(0, isLeft ? 180 : -180, 0);
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