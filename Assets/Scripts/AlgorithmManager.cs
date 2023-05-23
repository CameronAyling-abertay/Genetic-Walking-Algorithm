using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmManager : MonoBehaviour
{
    private AgentManager agentManager;
    public GameObject walkerPrefab;
    public int agents;
    public int genes;
    public float mutationRate;
    public float timeScale;
    public Camera playerCam;
    public UnityEngine.UI.Text generationText;
    public UnityEngine.UI.Text fitnessText;

    // Start is called before the first frame update
    void Start()
    {
        agentManager = new AgentManager(agents, walkerPrefab, genes, mutationRate, playerCam);
        Time.timeScale = timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        agentManager.Update();

        generationText.text = "Generation: " + agentManager.generation;
        fitnessText.text = "Average Distance:\n" + agentManager.averageDistance;
    }
}
