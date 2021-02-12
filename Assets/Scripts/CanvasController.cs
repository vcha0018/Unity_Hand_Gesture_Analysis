using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DataStructure;
using TMPro;

public class CanvasController : MonoBehaviour
{
    private System.Random _random = new System.Random();
    private List<Vector3> positionFactores = new List<Vector3>();
    private float boundingLength = float.NaN;
    private float rescaleFactor = float.NaN;
    private float rescaleReference = 300;
    private bool isInitialized = false;
    private GameObject gestureNameObject;

    private void Awake()
    {
        GameObject handJoint = GameObject.Find("HandJoint");
        BuildPositioVectores(GestureProcessor.Instance.TotalGestures);
        AssignHandModelsToGestures(handJoint);
        handJoint.SetActive(false);
        isInitialized = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeVisibilityOfGestures(true);
        gestureNameObject = GameObject.Find("GestureName");
    }

    private void BuildPositioVectores(int gestureCount)
    {
        if (gestureCount < 1)
            throw new System.ArgumentException("Looks like there are no gestures in the pool.");
        boundingLength = float.Parse(System.Math.Sqrt(Screen.width * Screen.height / gestureCount).ToString());
        //CHECK 0: Ceiling function to fit all
        int row = System.Convert.ToInt32(System.Math.Ceiling(Screen.width / boundingLength));
        int col = System.Convert.ToInt32(System.Math.Ceiling(Screen.height / boundingLength));
        int tempCount = gestureCount;
        positionFactores = new List<Vector3>();
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
                    positionFactores.Add(new Vector3(maxX - i * rescaleReference, maxY - j * rescaleReference, 0));
                }
            }
        }
        rescaleFactor = boundingLength / rescaleReference;
    }

    private void AssignHandModelsToGestures(GameObject handJointTemplate)
    {
        if (positionFactores.Count != GestureProcessor.Instance.TotalGestures)
            throw new System.Exception("Not all gesture has its position assigned.");
        GameObject haneModelParent = GameObject.Find("HandModels");
        List<Person> gesture_collection = GestureProcessor.Instance.GestureCollection;
        int gesture_index = 0;
        foreach (Person person in gesture_collection)
        {
            foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
            {
                foreach (Gesture gesture in gestureItem.Value)
                {
                    gesture.HandModel = BuildHandModel(handJointTemplate, haneModelParent);
                    gesture.HandModel.SetActive(false);
                    gesture.Tag = gestureItem.Key.ToString();
                    gesture.PositionFactor = positionFactores[gesture_index++];
                    foreach (HandJointController childController in gesture.HandModel.GetComponentsInChildren<HandJointController>())
                        childController.gestureName = string.Format("{0} | {1} | {2}", person.Name, gesture.HandType.ToString(), gestureItem.Key.ToString());

                    // Assign TagDisplayer's gesture refrence to this gesture class.
                    // Prepair data....
                    gesture.NormalizeGesture();
                    gesture.TransformJoints();
                    gesture.HandModel.transform.localScale *= rescaleFactor;
                }
            }
        }
    }

    private GameObject BuildHandModel(GameObject handJointTemplate, GameObject parentObject)
    {
        if (parentObject == null)
            throw new System.ArgumentException("Empty Parent Handmodel!");
        GameObject handModel = Instantiate(GameObject.Find("HandModelTemplate")); //new GameObject(string.Format("HandModel_{0}", _random.Next(0, 9999).ToString("D4")));
        handModel.name = string.Format("HandModel_{0}", _random.Next(0, 9999).ToString("D4"));
        handModel.transform.SetParent(parentObject.transform);

        if (handJointTemplate != null)
        {
            for (int i = 0; i < Constants.NUM_JOINTS; i++)
            {
                GameObject clone = Instantiate(handJointTemplate, Vector3.zero, Quaternion.identity);
                clone.name = string.Format("Joint{0}", i);
                Transform transform = clone.GetComponent<Transform>();
                SphereCollider sphereCollider = clone.GetComponent<SphereCollider>();

                float sphere_diameter = sphereCollider.radius * 2;
                transform.position = new Vector3(i * sphere_diameter + 1, 0.5f, 0);
                transform.SetParent(handModel.transform);
            }
        }
        else
            throw new System.Exception("Unable to Find HandJoint GameObject!");

        return handModel;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitialized)
            foreach (Person person in GestureProcessor.Instance.GestureCollection)
                foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
                    foreach (Gesture gesture in gestureItem.Value)
                        gesture.ConditionCheck();
    }

    private void FixedUpdate()
    {
        if (isInitialized)
            foreach (Person person in GestureProcessor.Instance.GestureCollection)
                foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
                    foreach (Gesture gesture in gestureItem.Value)
                        gesture.AnimateInAnimationMode();
    }

    /// <summary>
    /// Draws the bounding box for a gesture
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isInitialized)
            foreach (Person person in GestureProcessor.Instance.GestureCollection)
                foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
                    foreach (Gesture gesture in gestureItem.Value)
                    {
                        Gizmos.color = Color.red;
                        if (gesture.HandModel != null && gesture.HandModel.activeSelf)
                        {
                            Gizmos.DrawWireCube(gesture.Centroid + gesture.PositionFactor * rescaleFactor, gesture.GetBoundingBoxSize() * rescaleFactor);
                        }
                    }
    }

    /// <summary>
    /// Displays a category of gestures and their tag.
    /// </summary>
    public void ChangeVisibilityOfGestures(bool visibility, string byCategoryName = "")
    {
        GestureTypeFormat gType = GestureTypeFormat.None;
        if (byCategoryName != null && byCategoryName != "")
            if(!System.Enum.TryParse(byCategoryName, out gType))
                throw new System.Exception("Cannot able to convert given category name to format.");
        foreach (Person person in GestureProcessor.Instance.GestureCollection)
            foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
                foreach (Gesture gesture in gestureItem.Value)
                {
                    if (gType != GestureTypeFormat.None && gestureItem.Key == gType)
                        gesture.HandModel.SetActive(visibility);
                    else if (byCategoryName == null || byCategoryName == "")
                        gesture.HandModel.SetActive(visibility);
                    else
                        gesture.HandModel.SetActive(!visibility);
                }
    }

    public void Analyse()
    {
        TMP_InputField inputGestureTypeTBox = GameObject.Find("InputGestureType").GetComponent<TMP_InputField>();
        TMP_InputField inputToleranceTBox = GameObject.Find("InputTolerance").GetComponent<TMP_InputField>();
        TMP_Text resultStatusLabel = GameObject.Find("ResultStatus").GetComponent<TMP_Text>();
        
        GestureTypeFormat gType = GestureTypeFormat.None;
        if (System.Enum.TryParse(inputGestureTypeTBox.text.Trim().ToLower(), ignoreCase: true, out gType) && gType != GestureTypeFormat.None)
        {
            double inputToleranceValue = -1;
            double.TryParse(inputToleranceTBox.text.Trim(), out inputToleranceValue);
            KeyValuePair<double, double> toleranceConsensusPair = GestureProcessor.Instance.GetConsensusByGestureType(
                gType,
                double.TryParse(inputToleranceTBox.text.Trim(), out inputToleranceValue) ? inputToleranceValue : -1);
            resultStatusLabel.text = string.Format("Tolerance: {0} | Consesnsus: {1}%", toleranceConsensusPair.Key, toleranceConsensusPair.Value);
        }
        else
            Debug.LogError("Invalid Gesture Type entered!");
    }
}
