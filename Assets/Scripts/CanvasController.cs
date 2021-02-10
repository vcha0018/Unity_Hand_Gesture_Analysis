using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DataStructure;

public class CanvasController : MonoBehaviour
{
    private System.Random _random = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        //GameObject handModel = new GameObject("PrimeHandModel"); // Instantiate(GameObject.FindGameObjectWithTag("HandModelTag"));
        //handModel.transform.SetParent(this.transform.parent.transform);

        List<Person> gesture_collection = GestureProcessor.Instance.GestureCollection;
        if (gesture_collection.Count > 0)
        {
            Person person = gesture_collection[0];
            foreach (Gesture gesture in person.Gestures.First().Value)
            {
                GameObject handModel = BuildHandModel(this.transform);
                
                List<HandPose> handPoses = gesture.GetDominateHand();
                HandPose firstPose = handPoses[0];
                for (int i = 0; i < firstPose.Joints.Length; i++)
                {
                    Transform jointClone = handModel.transform.GetChild(i); // handModel.GetComponentInChildren<GameObject>();
                    //jointClone.transform.SetParent(handModel.transform);
                    jointClone.transform.position = firstPose.Joints[i];
                }
                break;
            }
        }
    }

    private GameObject BuildHandModel(Transform parentTransform)
    {
        GameObject handModel = new GameObject(string.Format("HandModel_{0}", _random.Next(0, 9999).ToString("D4")));
        handModel.transform.SetParent(parentTransform);

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
