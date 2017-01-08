using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

    private Animator animator;

    private const string DIRECTION = "Direction";
    private DirectionEnum _currentDirection = DirectionEnum.IDLE;
    private bool _current_is_left = false;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");

        if (vertical > 0)
        {
            animator.SetInteger(DIRECTION, (int)DirectionEnum.SOUTH);
        }
        else if (vertical < 0)
        {
            animator.SetInteger(DIRECTION, (int)DirectionEnum.NORTH);
        }
        else if (horizontal > 0)
        {
            animator.SetInteger(DIRECTION, (int)DirectionEnum.EAST);
            flipDirection(true);
        }
        else if (horizontal < 0)
        {
            animator.SetInteger(DIRECTION, (int)DirectionEnum.WEST);
            flipDirection(false);
        }
        else
        {
            //TODO: for testing make a universal idle state so we know. (use the box lel)
            animator.SetInteger(DIRECTION, -1);
        }
    }

    private void flipDirection(bool isLeft)
    {

        if (_current_is_left ^ isLeft)
        {
            if (isLeft)
            {
                transform.Rotate(0, 180, 0);
            }
            else
            {
                transform.Rotate(0, -180, 0);
            }
            _current_is_left = isLeft;
        }

    }
}