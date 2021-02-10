using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DataStructure;

public class CanvasController : MonoBehaviour
{
    private System.Random _random = new System.Random();

    private List<Vector3> positionFactores = new List<Vector3>();

    private float boundingLength = float.NaN;

    private float rescaleFactor = float.NaN;

    private float rescaleReference = 300;

    // Start is called before the first frame update
    void Start()
    {
        BuildPositioVectores(GestureProcessor.Instance.TotalGestures);
        AssignHandModelsToGestures();
    }

    private void BuildPositioVectores(int gestureCount)
    {
        boundingLength = float.Parse(System.Math.Sqrt(Screen.width * Screen.height / gestureCount).ToString());
        int row = System.Convert.ToInt32(Screen.width / boundingLength);
        int col = System.Convert.ToInt32(Screen.height / boundingLength);
        int tempCount = gestureCount;
        List<Vector3> positionFactors = new List<Vector3>();
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (tempCount == 0)
                {
                    break;
                }
                else
                {
                    tempCount -= 1;
                    float maxX = rescaleReference * row / 2;
                    float maxY = rescaleReference * col / 2;
                    positionFactors.Add(new Vector3(maxX - i * rescaleReference, maxY - j * rescaleReference, 0));
                }
            }
        }
        rescaleFactor = boundingLength / rescaleReference;
    }

    private void AssignHandModelsToGestures()
    {
        GameObject haneModelParent = GameObject.Find("HandModels");
        List<Person> gesture_collection = GestureProcessor.Instance.GestureCollection;
        int gesture_index = 0;
        foreach(Person person in gesture_collection)
        {
            foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
            {
                foreach (Gesture gesture in gestureItem.Value)
                {
                    gesture.HandModel = BuildHandModel(haneModelParent);
                    gesture.HandModel.SetActive(false);
                    gesture.HandModel.tag = gesture.Tag;
                    gesture.PositionFactor = positionFactores[gesture_index++];

                    // Assign TagDisplayer's gesture refrence to this gesture class.
                    // Prepair data....

                    gesture.HandModel.transform.localScale *= rescaleFactor;

                    //List<HandPose> handPoses = gesture.GetDominateHand();
                    //HandPose firstPose = handPoses[0];
                    //for (int i = 0; i < firstPose.Joints.Length; i++)
                    //{
                    //    Transform jointClone = gesture.HandModel.transform.GetChild(i); // handModel.GetComponentInChildren<GameObject>();
                    //                                                                    //jointClone.transform.SetParent(handModel.transform);
                    //    jointClone.transform.position = firstPose.Joints[i];
                    //}
                    //break;
                }
            }
        }
    }

    private GameObject BuildHandModel(GameObject parentObject)
    {
        if (parentObject == null)
            throw new System.ArgumentException("Empty Parent Handmodel!");
        GameObject handModel = new GameObject(string.Format("HandModel_{0}", _random.Next(0, 9999).ToString("D4")));
        handModel.transform.SetParent(parentObject.transform);

        List<GameObject> joints;
        GameObject handJoint = Instantiate(GameObject.Find("HandJoint"));
        if (handJoint != null)
        {
            joints = new List<GameObject>();
            for (int i = 0; i < Constants.NUM_JOINTS; i++)
            {
                GameObject clone = Instantiate(handJoint, Vector3.zero, Quaternion.identity);
                clone.name = string.Format("Joint{0}", i);
                Transform transform = clone.GetComponent<Transform>();
                SphereCollider sphereCollider = clone.GetComponent<SphereCollider>();

                float sphere_diameter = sphereCollider.radius * 2;
                transform.position = new Vector3(i * sphere_diameter + 1, 0.5f, 0);
                transform.SetParent(handModel.transform);
                joints.Add(clone);
            }
        }
        else
            throw new System.Exception("Unable to Find HandJoint GameObject!");

        return handModel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
