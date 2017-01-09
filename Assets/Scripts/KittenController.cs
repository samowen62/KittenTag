using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

    public float walkSpeed = 0.05f;

    private Animator animator;
    private CharacterController charController;

    private const string DIRECTION = "Direction";
    private DirectionEnum _currentDirection = DirectionEnum.SOUTH;
    private bool _current_is_left = false;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();

        charController = GetComponent<CharacterController>();
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
            charController.Move(walkSpeed * Vector2.up);
        }
        else if (vertical < 0)
        {
            changeDirection(DirectionEnum.NORTH);
            charController.Move(walkSpeed * Vector2.down);
        }
        else if (horizontal > 0)
        {
            changeDirection(DirectionEnum.SIDE);
            charController.Move(walkSpeed * Vector2.left);
            flipDirection(true);
        }
        else if (horizontal < 0)
        {
            changeDirection(DirectionEnum.SIDE);
            charController.Move(walkSpeed * Vector2.right);
            flipDirection(false);
        }
        else
        {
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

    
}