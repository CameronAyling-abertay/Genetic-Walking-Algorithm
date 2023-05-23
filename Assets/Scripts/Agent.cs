using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Public variables for use with other entities
    public char[,] chromosome;
    public bool complete;
    public int genes;
    public float height;

    // Private objects for internal use
    private int index;
    private float speed;
    private float torque;

    // Default positions
    Quaternion[] defaultRotations;
    Vector3[] defaultPositions;

    void Start()
    {
        // Variable setup
        index = 0;
        speed = 500000f;
        height = 0;

        complete = false;

        defaultRotations = new Quaternion[10];
        defaultPositions = new Vector3[10];

        // Log the default positions and rotations
        int i = 0;
        foreach (Transform child in transform)
        {
            defaultRotations[i] = child.rotation;
            defaultPositions[i] = child.position;
            i++;
        }
    }

    // Function to reset the agent to default
    public void Reset()
    {
        // Reset every velocity and change the transforms' rotations and positions to the default
        int i = 0;
        foreach(Transform child in transform)
        {
            child.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            child.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, 0f, 0f);
            child.rotation = defaultRotations[i];
            child.position = defaultPositions[i];
            i++;
        }

        // Reset the height tracker
        height = 0;

        // Unset the complete flag
        complete = false;
    }

    // Runs every frame
    void FixedUpdate()
    {
        // If the agent hasn't run through the entire chromosome
        if (!complete)
        {
            // Track which limb is currently to be worked upon
            // My workaround for being unable to track 2 variables with a foreach loop
            int limbNum = 0;

            // Iterate through every child
            foreach (Transform child in transform)
            {
                // Find if the child is the head or a limb
                if(child.tag == "Head" || child.tag == "Limb")
                {
                    // Get the child's rigidbody
                    Rigidbody rb = child.GetComponent<Rigidbody>();

                    // Find the gene which should be used as an instruction and use it to inform how much torque should be applied to the limb
                    char gene = chromosome[limbNum, index];
                    switch (gene)
                    {
                        // Go Clockwise
                        case 'a': // Small amount
                            torque = 1f * Time.deltaTime * speed;
                            break;
                        case 'b': // Medium amount
                            torque = 5f * Time.deltaTime * speed;
                            break;
                        case 'c': // Large amount
                            torque = 10f * Time.deltaTime * speed;
                            break;

                        // Go Anticlockwise
                        case 'x': // Small amount
                            torque = -1f * Time.deltaTime * speed;
                            break;
                        case 'y': // Medium amount
                            torque = -5f * Time.deltaTime * speed;
                            break;
                        case 'z': // Large amount
                            torque = -10f * Time.deltaTime * speed;
                            break;

                        // Default for debug purposes to find out whether it's possible for the gene parsed to not be one of the ones already checked
                        default:
                            Debug.Log("Hit default");
                            break;
                    }

                    // Apply the torque around the Z axis as the agents are configured that they are side-on
                    rb.AddTorque(child.forward * torque);

                    // Update the height tracker
                    if(child.tag == "Head")
                    {
                        height += child.transform.position.y;
                    }

                    // Increment the limb counter for the next limb
                    limbNum++;
                }
            }
            
            // Increment the index for which gene within the chromosome comes next
            index++;
        }

        // If the agent has carried out the same number of genes or more as are in its DNA then mark itself as complete
        if (index >= genes)
        {
            index = 0;
            complete = true;
        }
    }
}
