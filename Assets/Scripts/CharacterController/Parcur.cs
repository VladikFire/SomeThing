using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class Parcur : MonoBehaviour
{
    [SerializeField]
    private Transform _playerCenterPosition;
    private Animator _animator;
    private Animation _animation;
    private CharacterController _controller;
    private Collider _collider;
    private List<Vector3> _nearestPointOfCollider;
    public float m_MaxDistance = 300.0f;
    bool m_HitDetect;

    Collider m_Collider;
    RaycastHit m_Hit;
    public Vector3 BoxSize;

    protected internal bool _canJump;


    void Start()
    {
 
        _animator = GetComponent<Animator>();
        _animation = GetComponent<Animation>();
        _controller = GetComponent<CharacterController>();
    }
    void Update()
    {

        if (_animator.GetCurrentAnimatorStateInfo(1).IsName("Ellen_Idle"))
        {
            _controller.enabled = true;
            _animator.applyRootMotion = false;
        }
        m_HitDetect = Physics.BoxCast(_playerCenterPosition.position, BoxSize, -transform.up, out m_Hit, transform.rotation, m_MaxDistance);

        if (m_HitDetect)
        {
            float y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, y, 0f);
            if (_controller.height- m_Hit.distance >= 0.1999999 && 1.75f - m_Hit.distance <= 0.40001)
            {
                _canJump = false;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    float blendK = _controller.height - m_Hit.distance;
                    _controller.enabled = false;
                    _animator.applyRootMotion = true;
                    _animator.SetFloat("Vec", blendK);
                    _animator.Play("fgfgds");
                }
            }
            else
            {
                _canJump = true;
            }
        }
    }
    //   private void on  (Collision collision)
    //   {
    //	if (collision.gameObject.tag != "Player")
    //	{
    //		_nearestPointOfCollider.Add(collision.collider.ClosestPoint(_playerCenterPosition.position));
    //		for (Int16 i = 0; i < _nearestPointOfCollider.Count; i++)
    //		{
    //			Debug.DrawRay(_playerCenterPosition.position, _nearestPointOfCollider[i], Color.cyan, 0.2f);
    //		}
    //	}
    //}

    //  private void OnTriggerStay(Collider other)
    //   {
    //if (other.gameObject.tag!="Player")
    ///		if (!_nearestPointOfCollider.Contains(other.ClosestPoint(_playerCenterPosition.position)))
    //			_nearestPointOfCollider.Add(other.ClosestPoint(_playerCenterPosition.position));
    //}
    //	private void OnTriggerExit(Collider other)
    //	{
    //		if (other.gameObject.tag != "Player")
    //		if (_nearestPointOfCollider.Contains(other.ClosestPoint(_playerCenterPosition.position)))
    //	_nearestPointOfCollider.Remove(other.ClosestPoint(_playerCenterPosition.position));
    //}
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (m_HitDetect)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(_playerCenterPosition.position, -_playerCenterPosition.up * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(_playerCenterPosition.position  -_playerCenterPosition.up * m_Hit.distance, BoxSize * 2);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(_playerCenterPosition.position, -_playerCenterPosition.up * m_MaxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(_playerCenterPosition.position  -_playerCenterPosition.up * m_MaxDistance, BoxSize * 2);
        }
    }

}

