using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class CubeController : MonoBehaviour
{
    public static CubeController Singleton;
    public Obi.ObiPathSmoother obiPathSmoother;
    [SerializeField] private float moveSpeed;
    
    
    private Camera cam;
    private GameObject selectedCube;
    private Vector3 screenPoint;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 resultVector;
    private Vector3 cubeStartTransfrom;
    private bool isMoveDirectionVertical;
    private bool isACubeSelected;
    private bool isMovementStarted;
    private bool isCubesClickable;
    private float leftBorder, rightBorder, topBorder, bottomBorder;

    private LevelGenerator _levelGenerator;
    private enum MoveDirection
    {
        Back,
        Forward
    }
    private enum BorderAxis
    {
        X,
        Z
    }
    [SerializeField] private float transitionTime;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        _levelGenerator = FindObjectOfType<LevelGenerator>();

        leftBorder = (_levelGenerator.GetComponent<LevelGenerator>().width-1) * -1 * 1.5f;
        rightBorder = (_levelGenerator.GetComponent<LevelGenerator>().width-1) * 1.5f;
        topBorder = (_levelGenerator.GetComponent<LevelGenerator>().height-1) * 1.5f;
        bottomBorder = (_levelGenerator.GetComponent<LevelGenerator>().height-1) * -1 * 1.5f;
        
        isACubeSelected = false;
        isMovementStarted = false;
        isCubesClickable = true;
        cam = Camera.main;
    }
    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            if(!StateMachine.isGamePlaying()) return;

            if(isCubesClickable)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Casting a ray to select the cube
                RaycastHit hit;
                if (Physics.SphereCast(ray, 0.2f, out hit))
                {
                    if (hit.collider.transform.CompareTag("Cube"))
                    {
                        selectedCube = hit.collider.gameObject;
                        cubeStartTransfrom = selectedCube.transform.position;
                        isACubeSelected = true;
                        isCubesClickable = false;
                    }
                }

                screenPoint = cam.WorldToScreenPoint(transform.position);
                startPoint = transform.position -
                             cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                                 screenPoint.z));
            }
        }
        if(isACubeSelected)
            HandleMovement();
    }
    void HandleMovement()
    {
        if (Input.GetMouseButton(0))
        {
            //Calculate a vector with mouse position for moving the cube
            endPoint = transform.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,screenPoint.z));
            resultVector = startPoint - endPoint;
            resultVector.y = 0f;

            if(!isMovementStarted) //Selecting x or z axis to move the cube and LOCK the axis to PREVENT moving in the another axis
            {
                if (Mathf.Abs(resultVector.x) > Mathf.Abs(resultVector.z)) //Lock the move trajectory to X axis
                {
                    selectedCube.transform.position = Vector3.MoveTowards(selectedCube.transform.position,
                        cubeStartTransfrom + new Vector3(resultVector.x, 0, 0), moveSpeed * Time.deltaTime);

                    if (Mathf.Abs(resultVector.x) > 0.1f && !isMovementStarted)
                    {
                        if (selectedCube.transform.position.x > rightBorder + .1f ||
                            selectedCube.transform.position.x < leftBorder - .1f)
                        {
                            isACubeSelected = false;
                            isMovementStarted = false;
                            MoveCubeToClosestPoint(BorderAxis.X);
                        }
                        else
                        {
                            isMoveDirectionVertical = false;
                            isMovementStarted = true;
                        }
                        
                    }
                }
                else //Lock the move trajectory to Z axis
                {
                    selectedCube.transform.position = Vector3.MoveTowards(selectedCube.transform.position,
                        cubeStartTransfrom + new Vector3(0, 0, resultVector.z), moveSpeed * Time.deltaTime);

                    if (Mathf.Abs(resultVector.z) > 0.1f && !isMovementStarted)
                    {
                        if (selectedCube.transform.position.z> topBorder + .1f ||
                            selectedCube.transform.position.z < bottomBorder - .1f)
                        {
                            isACubeSelected = false;
                            isMovementStarted = false;
                            MoveCubeToClosestPoint(BorderAxis.Z);
                        }
                        else
                        {
                            isMoveDirectionVertical = true;
                            isMovementStarted = true;
                        }
                    }
                }
            }
            else //Move the cube on the selected axis
            {
                if (!isMoveDirectionVertical)
                {
                    if (selectedCube.transform.position.x > rightBorder + .1f ||
                        selectedCube.transform.position.x < leftBorder - .1f)
                    {
                        isACubeSelected = false;
                        isMovementStarted = false;
                        MoveCubeToClosestPoint(BorderAxis.X);
                    }
                    else
                    {
                        selectedCube.transform.position = Vector3.MoveTowards(selectedCube.transform.position,
                            cubeStartTransfrom + new Vector3(resultVector.x, 0, 0), moveSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    if (selectedCube.transform.position.z> topBorder + .1f ||
                        selectedCube.transform.position.z < bottomBorder - .1f)
                    {
                        isACubeSelected = false;
                        isMovementStarted = false;
                        MoveCubeToClosestPoint(BorderAxis.Z);
                    }
                    else
                    {
                        selectedCube.transform.position = Vector3.MoveTowards(selectedCube.transform.position,
                            cubeStartTransfrom + new Vector3(0, 0, resultVector.z), moveSpeed * Time.deltaTime);
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isMovementStarted = false;      //Free the selected axis
            PutInPoint();                  //Move the cube to closest valid point
            isACubeSelected = false;      //Free the selected cube
        }
    }
    void PutInPoint()
    {
        if (!isMoveDirectionVertical)
        {
            if(selectedCube.transform.position.x - cubeStartTransfrom.x > 0)
            {
                if ((selectedCube.transform.position.x - cubeStartTransfrom.x)%3 < 1.5f) //Right Movement
                {
                    MoveCubeToClosestPoint(Vector3.right, MoveDirection.Back);
                }
                else
                {
                    MoveCubeToClosestPoint(Vector3.right, MoveDirection.Forward);
                }
            }
            else
            {
                if ((selectedCube.transform.position.x - cubeStartTransfrom.x)%3 > -1.5f) //Left Movement
                {
                    MoveCubeToClosestPoint(Vector3.left, MoveDirection.Back);
                }
                else
                {
                    MoveCubeToClosestPoint(Vector3.left, MoveDirection.Forward);
                }
            }
        }
        else
        {
            if(selectedCube.transform.position.z - cubeStartTransfrom.z > 0)
            {
                if ((selectedCube.transform.position.z - cubeStartTransfrom.z)%3 < 1.5f) //Forward movement
                {
                    MoveCubeToClosestPoint(Vector3.forward, MoveDirection.Back);
                }
                else
                {
                    MoveCubeToClosestPoint(Vector3.forward, MoveDirection.Forward);
                }
            }
            else
            {
                if ((selectedCube.transform.position.z - cubeStartTransfrom.z)%3 > -1.5f) //Back movement
                {
                    MoveCubeToClosestPoint(Vector3.back, MoveDirection.Back);
                }
                else
                {
                    MoveCubeToClosestPoint(Vector3.back, MoveDirection.Forward);
                }
            }
        }
    }
    void MakeCubesClickable()
    {
        isCubesClickable = true;
    }
    void MoveCubeToClosestPoint(Vector3 directionVector,MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.Back && directionVector.x != 0)
        {
            Transition.Instance.Move(selectedCube.transform, cubeStartTransfrom + directionVector * (3 * Convert.ToInt32(Mathf.Floor(Mathf.Abs((selectedCube.transform.position.x - cubeStartTransfrom.x) / 3)))), 0f, transitionTime);
        }
        else if(moveDirection == MoveDirection.Forward && directionVector.x != 0)
        {
            Transition.Instance.Move(selectedCube.transform, cubeStartTransfrom + directionVector * (3 * (Convert.ToInt32(Mathf.Floor(Mathf.Abs((selectedCube.transform.position.x - cubeStartTransfrom.x) / 3)))+1)), 0f, transitionTime);
        }
        else if(moveDirection == MoveDirection.Back && directionVector.z != 0)
        {
            Transition.Instance.Move(selectedCube.transform, cubeStartTransfrom + directionVector * (3 * Convert.ToInt32(Mathf.Floor(Mathf.Abs((selectedCube.transform.position.z - cubeStartTransfrom.z) / 3)))), 0f, transitionTime);
        }
        else if(moveDirection == MoveDirection.Forward && directionVector.z != 0)
        {
            Transition.Instance.Move(selectedCube.transform, cubeStartTransfrom + directionVector * (3 * (Convert.ToInt32(Mathf.Floor(Mathf.Abs((selectedCube.transform.position.z - cubeStartTransfrom.z) / 3)))+1)), 0f, transitionTime);
        }
        Invoke(nameof(MakeCubesClickable),transitionTime);
    }
    void MoveCubeToClosestPoint(BorderAxis borderAxis)
    {
        if(borderAxis == BorderAxis.X)  //Check if the cube is exceeding the border on the X axis
        {
            if (selectedCube.transform.position.x > 0) //Right Border
            {
                GameObject targetTransfrom = new GameObject(); //Create a GameObject which contains the Target Transform
                targetTransfrom.transform.position = new Vector3(rightBorder,selectedCube.transform.position.y,selectedCube.transform.position.z);
                
                Transition.Instance.Move(selectedCube.transform, targetTransfrom.transform, 0f, transitionTime);
                Destroy(targetTransfrom,transitionTime + 1f);
            }
            else //Left Border
            {
                GameObject targetTransfrom = new GameObject(); //Create a GameObject which contains the Target Transform
                targetTransfrom.transform.position= new Vector3(leftBorder,selectedCube.transform.position.y,selectedCube.transform.position.z);
                
                Transition.Instance.Move(selectedCube.transform, targetTransfrom.transform, 0f, transitionTime);
                Destroy(targetTransfrom,transitionTime + 1f);
            }
        }
        else //Check if the cube is exceeding the border on the Z axis
        {
            if (selectedCube.transform.position.z > 0) //Top Border
            {
                GameObject targetTransfrom = new GameObject(); //Create a GameObject which contains the Target Transform
                targetTransfrom.transform.position = new Vector3(selectedCube.transform.position.x,selectedCube.transform.position.y,topBorder);
                
                Transition.Instance.Move(selectedCube.transform, targetTransfrom.transform, 0f, transitionTime);
                Destroy(targetTransfrom,transitionTime + 1f);
            }
            else //Bottom Border
            {
                GameObject targetTransfrom = new GameObject(); //Create a GameObject which contains the Target Transform
                targetTransfrom.transform.position = new Vector3(selectedCube.transform.position.x,selectedCube.transform.position.y,bottomBorder);
                
                Transition.Instance.Move(selectedCube.transform, targetTransfrom.transform, 0f, transitionTime);
                Destroy(targetTransfrom,transitionTime + 1f);
            }
        }
        Invoke(nameof(MakeCubesClickable),transitionTime); //Make the cube moveable agan after the end of Transition
    }
    public void MoveCubeToClosestPoint()
    {
        Transition.Instance.Move(selectedCube.transform,cubeStartTransfrom,0f,transitionTime);
        isACubeSelected = false;
        isMovementStarted = false;
        Invoke(nameof(MakeCubesClickable),transitionTime); //Make the cube moveable again after the end of Transition
    }
    public void MoveCubeToClosestPoint(bool isCollidingWithObstacle)
    {
        float moveAxisPoint;
        Vector3 movePoint = new Vector3();
        if (isMoveDirectionVertical)
        {
            moveAxisPoint = RoundToStep(selectedCube.transform.position.z,3);
            movePoint = new Vector3(selectedCube.transform.position.x,selectedCube.transform.position.y,moveAxisPoint);
            Transition.Instance.Move(selectedCube.transform,movePoint, 0f,transitionTime);
        }
        else
        {
            moveAxisPoint = RoundToStep(selectedCube.transform.position.x,3);
            movePoint = new Vector3(moveAxisPoint,selectedCube.transform.position.y,selectedCube.transform.position.z);
            Transition.Instance.Move(selectedCube.transform,movePoint, 0f,transitionTime);
        }
        isACubeSelected = false;
        isMovementStarted = false;
        Invoke(nameof(MakeCubesClickable),transitionTime); //Make the cube moveable again after the end of Transition
    }
    float RoundToStep(float coordinate,float step)
    {
        float offset;
        float calculatedCoordinate;
        if (isMoveDirectionVertical)
        {
            if (_levelGenerator.height % 2 == 1)
                offset = 0f;
            else
            {
                offset = 1.5f;
            }
            if (coordinate > cubeStartTransfrom.z) //From Bottom
            {
                calculatedCoordinate = 0;
                for(int i = -3; i<=3;i++)
                {
                    if (coordinate > i * 3 + offset && coordinate < (i + 1) * 3 + offset)
                    {
                        calculatedCoordinate = i * 3 + offset;
                        break;
                    }
                }
            }
            else //From Top
            {
                calculatedCoordinate = 0;
                for(int i = -3; i<=3;i++)
                {
                    if (coordinate > i * 3 + offset && coordinate < (i + 1) * 3 + offset)
                    {
                        calculatedCoordinate = (i + 1) * 3 + offset;
                        break;
                    }
                }
            }
        }
        else
        {
            if (_levelGenerator.width % 2 == 1)
                offset = 0f;
            else
            {
                offset = 1.5f;
            }
            if (coordinate > cubeStartTransfrom.x) //From Left
            {
                calculatedCoordinate = 0;
                for(int i = -3; i<=3;i++)
                {
                    if (coordinate > i * 3 + offset && coordinate < (i + 1) * 3 + offset)
                    {
                        calculatedCoordinate = i * 3 + offset;
                        break;
                    }
                }
            }
            else //From Right
            {
                calculatedCoordinate = 0;
                for(int i = -3; i<=3;i++)
                {
                    if (coordinate > i * 3 + offset && coordinate < (i + 1) * 3 + offset)
                    {
                        calculatedCoordinate = (i + 1) * 3 + offset;
                        break;
                    }
                }
            }
        }
        return calculatedCoordinate;
    }
}