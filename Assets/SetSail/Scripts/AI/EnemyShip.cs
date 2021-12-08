using System;
using System.Net.Http.Headers;
using System.Xml;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyShip : MonoBehaviour
    {
        // This will eventually be replaced with a GameManager to get the player reference to decouple
        [SerializeField]
        private GameObject PlayerObject;

        [SerializeField]
        private bool isPlayerAI = false;
        
        [SerializeField]
        private bool isDebug = true;

        [SerializeField] private TextMeshPro debugText;
        
        private enum State
        {
            Wander,
            Approach,
            Attack
        }

        private State currentState;
        private Vector3 currentTargetPosition = Vector3.zero;
        private Vector3 facing;
        private Quaternion targetLookRotation;

        private float moveSpeed = 5f;
        private float stoppingRange = 50f;
        private float attackRange = 150f;
        private float sightRange = 200f;

        

        private void Awake()
        {
            if (!isPlayerAI)
            {
                GameObject player = GameObject.FindWithTag("Player");

                if (player != null) PlayerObject = player;
            }
        }

        
        private void Update()
        {
            var rayLineColour = NotPerpendicular() ? Color.red : Color.green;
            Debug.DrawRay(transform.position, transform.right * attackRange, rayLineColour);
            Debug.DrawRay(transform.position, (transform.right * -1) * attackRange, rayLineColour);
            
            // Finite state machine
            switch (currentState)
            {
                case State.Wander:
                    WanderState();
                    break;
                case State.Approach:
                    ApproachState();
                    break;
                case State.Attack:
                    AttackState();
                    break;
            }

            if (isDebug)
            {
                debugText.SetText(currentState.ToString());
            }
        }

        #region States

         private void AttackState()
        {
            if (NotPerpendicular())
                {
                    Debug.Log("Rotating");
                    transform.RotateAround(transform.position, Vector3.up, 5*Time.deltaTime);
                }
            if (!NotPerpendicular())
                {
                    debugText.SetText("Attacking!");
                    Debug.Log("Attacking the player!");
                }
                if (CheckPlayerInSight() && !CheckPlayerInAttackRange())
                {
                    currentState = State.Approach;
                }

                if (!CheckPlayerInSight())
                {
                    currentState = State.Wander;
                }
        }

        private void ApproachState()
        {
            Debug.Log("Approaching");

            if (NotArrived())
            {
                SelectPlayerPosition();
            }

            transform.rotation = targetLookRotation;
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);

            var rayLineColour = IsObstacleBlocking() ? Color.red : Color.blue;
            Debug.DrawRay(transform.position, facing * sightRange, rayLineColour);
            
            if (CheckPlayerInAttackRange())
            {
                Debug.Log("Change to Attack");
                currentState = State.Attack;
            }

            if (!CheckPlayerInSight())
            {
                Debug.Log("Change to Wander");
                currentState = State.Wander;
            }
            
        }
        
        private void WanderState()
        {
            Debug.Log("Wandering");
            if (NotArrived())
            {
                Debug.Log("Selecting Random");
                SelectRandomPosition();
            }

            // Update the rotation of the ship to face where it's heading { Backlog: add linear interpolation for smooth turning }
            transform.rotation = targetLookRotation;
                    
            // Start moving towards the target
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);

            var rayLineColour = IsObstacleBlocking() ? Color.red : Color.green;
            Debug.DrawRay(transform.position, facing * 100f, rayLineColour);
            
            if (IsObstacleBlocking())
            {
                Debug.Log("Obstacle is blocking the path, rerouting...");
                SelectRandomPosition();
            }
            if (PlayerObject != null && CheckPlayerInAttackRange())
            {
                Debug.Log("Change to attack");
                currentState = State.Attack;
            }
            if (CheckPlayerInSight() && !CheckPlayerInAttackRange())
            {
                Debug.Log("Change to Approach");
                currentState = State.Approach;
            }
        }

        #endregion
        
        #region Utilities
        private bool CheckPlayerInSight()
        {
            float distance = Vector3.Distance(transform.position, PlayerObject.transform.position);

            return distance <= sightRange;
        }

        private void SelectPlayerPosition()
        {
            currentTargetPosition = PlayerObject.transform.position;
            facing = Vector3.Normalize(currentTargetPosition - transform.position);
            facing = new Vector3(facing.x, 0f, facing.z);
            targetLookRotation = Quaternion.LookRotation(facing);
        }
        
        
        private void SelectRandomPosition()
        {
            // Select a position in front of the boat, with random variation to left or right
            Vector3 tryThisPosition = (transform.position + (transform.forward * 50f)) +
                                      new Vector3(UnityEngine.Random.Range(-100f, 100f), 0f,
                                          UnityEngine.Random.Range(-100f, 100f));

            currentTargetPosition = new Vector3(tryThisPosition.x, transform.position.y, tryThisPosition.z);
            facing = Vector3.Normalize(currentTargetPosition - transform.position);
            facing = new Vector3(facing.x, 0f, facing.z);
            targetLookRotation = Quaternion.LookRotation(facing);
        }

        private bool NotArrived()
        {
            if (currentTargetPosition == Vector3.zero)
                return true;

            float remainingDistance = Vector3.Distance(transform.position, currentTargetPosition);
            return remainingDistance <= stoppingRange;
        }

        private bool IsObstacleBlocking()
        {
            return false;
        }
        
        private bool CheckPlayerInAttackRange()
        {
            float distance = Vector3.Distance(transform.position, PlayerObject.transform.position);

            return distance <= attackRange;
        }

        private bool NotPerpendicular()
        {
            Vector3 rayPos = transform.position + (Vector3.up * 0.5f); 
            
            bool notPerpendicular = true;
            RaycastHit hit;
            RaycastHit hit2;
            
            if (Physics.Raycast(rayPos, transform.right, out hit, attackRange))
            {
                if (hit.transform.gameObject == null) return true;
                // If we hit player then ship is perpendicular
                if (hit.transform.gameObject.CompareTag("Player"))
                    notPerpendicular = false;
            } 
            
            if (Physics.Raycast(rayPos, transform.right * -1, out hit2, attackRange))
            {
                if (hit2.transform.gameObject == null) return true;
                // If we hit player then ship is perpendicular
                if (hit2.transform.gameObject.CompareTag("Player"))
                    notPerpendicular = false;
            }
            return notPerpendicular;
        }

        #endregion
        
        
        
        
    }
}