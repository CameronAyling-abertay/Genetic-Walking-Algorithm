using System.Collections.Generic;
using UnityEngine;

public class AgentManager {
    // Start 
    public System.Random random;

    // Premade things
    private GameObject prefab;
    char[] possibleGenes;
    public int generation;

    // Objects directly involved with the algorithm
    int geneNumber;
    public List<Agent> walkers;
    float mutationRate;
    private int populationSize;

    // Fitness and distance variables
    Dictionary<Agent, double> fitness;
    public double averageFitness;
    public float averageDistance;

    // Camera
    public Camera playerCam;

    // Constructor
    public AgentManager(int agentCount, GameObject walkerPrefab, int genes, float mutation, Camera camera)
    {
        // Parse all the data from the setup
        populationSize = agentCount;
        prefab = walkerPrefab;
        geneNumber = genes;
        mutationRate = mutation;
        playerCam = camera;

        // Variable Setup
        random = new System.Random();
        generation = 0;

        walkers = new List<Agent>();
        fitness = new Dictionary<Agent, double>();
        averageFitness = new double();

        possibleGenes = new char[6]{ 'a', 'b', 'c', 'x', 'y', 'z' };

        // Create all agents and chromosomes in their starting conditions
        for (int i = 0; i < agentCount; i++)
        {
            // Create a new agent, add it to the list of agents and create a collection of chromosomes for it
            GameObject newAgent = GameObject.Instantiate(walkerPrefab, new Vector3(0, 0.5f, 0 + i * 2), new Quaternion());
            walkers.Add(newAgent.GetComponent<Agent>());
            char[,] dna = new char[5, geneNumber];

            // Build the chromosomes
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < geneNumber; k++)
                {
                    int nextGene = random.Next() % 6;
                    dna[j, k] = possibleGenes[nextGene];
                }
            }

            // Put the chromosomes into the agent
            walkers[i].chromosome = dna;
            walkers[i].genes = geneNumber;
        }
    }

    // Fitness evaluation function
    void EvaluateFitness()
    {
        // Clear all the fitness values and reset the average
        fitness.Clear();
        averageFitness = 0;

        // Loop through all the agents
        foreach (Agent walker in walkers)
        {
            // Create a variable to hold the agent's fitness
            double agentFitness = 0;

            // Loop through all the agent's children to find the rigidbodies
            foreach (Transform child in walker.transform)
            {
                // Weight the limbs' positions differently based on their importance in the body
                // This is where most of the testing takes place
                if(child.tag == "Head") // Head
                {
                    agentFitness += child.GetComponent<Rigidbody>().position.x * 5;
                }
                else if (child.tag == "Body") // Body
                {
                    agentFitness += child.GetComponent<Rigidbody>().position.x;
                }
                else // Arms and legs
                {
                    agentFitness += child.GetComponent<Rigidbody>().position.x;
                }
            }

            // Another function for testing, this one to do with incentivising the agents to stand upright
            //agentFitness += 20 * walker.height / geneNumber;

            // Add the fitness value to the dictionary for later reference and add the value to the average fitness
            fitness.Add(walker, agentFitness);
            averageFitness += agentFitness;
        }

        // Divide the total fitness sum by the number of agents to find the average
        averageFitness /= walkers.Count;
    }

    // Find the average distance the agents have walked
    void EvaluateDistance()
    {
        // Reset the average
        averageDistance = 0;

        // Loop through every agent
        foreach (Agent walker in walkers)
        {
            // Loop through every child
            foreach (Transform child in walker.transform)
            {
                // Find the body child
                if (child.tag == "Body")
                {
                    // Add the body's distance to the average
                    averageDistance += child.GetComponent<Rigidbody>().position.x;
                }
            }
        }

        // Divide the sum by the population size to find the average
        averageDistance /= populationSize;
    }

    // Create a new generation of agents
    public void NewGeneration()
    {
        // Find the fitnesses and the average distance
        EvaluateFitness();
        EvaluateDistance();

        // Create a new collection of chromosomes and DNA containers to hold them
        char[,,] newGen = new char[populationSize, 5, geneNumber];

        // For every agent that should exist
        for (int i = 0; i < populationSize; i++)
        {
            // Find two parents for them
            Agent parentOne = PickParent();
            Agent parentTwo = PickParent();

            // For every chromosome in the DNA
            for (int j = 0; j < 5; j++)
            {
                // For every gene that should be in the chromosome
                for (int k = 0; k < geneNumber; k++)
                {
                    // Pick a random number between 0 and 1
                    int decider = random.Next() % 2;

                    // If the random number is 0 take the gene from the first parent, if it's 1, take it from the other
                    char nextGene = (decider == 0) ? parentOne.chromosome[j, k] : parentTwo.chromosome[j, k];

                    // Add the gene to the new chromosome
                    newGen[i, j, k] = nextGene;
                }
            }
        }

        // Put the DNA into the agents
        for (int i = 0; i < populationSize; i++)
        {
            // Reset all the agents positions, rotations, and velocities
            walkers[i].Reset();

            // Create a container for the DNA which can be implanted straight into the agent and fill it with the corresponding DNA from the new generation's collection
            char[,] newDNA = new char[5, geneNumber];
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < geneNumber; k++)
                {
                    newDNA[j, k] = newGen[i, j, k];
                }
            }

            // Implant the DNA
            walkers[i].chromosome = newDNA;

            // Randomly mutate some amount of the agents
            if(((float)(random.Next() % 100))/100f <= mutationRate)
            {
                Mutate(walkers[i]);
            }
        }

        // Increment the generation counter
        generation++;
    }

    // Mutation function
    void Mutate(Agent agent)
    {
        // Randomise 1 gene in each chromosome in the DNA
        for(int i = 0; i < 5; i++)
        {
            char newGene = possibleGenes[random.Next() % 6];
            agent.chromosome[i, random.Next() % geneNumber] = newGene;
        }
    }

    // Choose a parent
    Agent PickParent()
    {
        // Create a counter to keep track of how many repeats have occured and a container for the parent
        int repeats = 0;
        Agent parent;

        // Find a parent with an above average fitness
        do
        {
            repeats++;
            parent = walkers[random.Next() % populationSize];

            if(repeats == populationSize / 2)
            {
                return parent;
            }
        } while (fitness[parent] < averageFitness);

        // If it can't find a suitable parent, just use the one it has
        return parent;
    }

    // Update
    public void Update()
    {
        // Find whether the agents are all complete
        bool complete = true;
        foreach (Agent walker in walkers)
        {
            if(!walker.complete)
            {
                complete = false;
                break;
            }
        }

        // If every agent is complete then create a new generation 
        if (complete)
        {
            NewGeneration();
        }

        // Line up the camera with the body of the agent furthest to the right
        float furthestRight = 0;
        foreach (Agent walker in walkers)
        {
            foreach (Transform child in walker.transform)
            {
                if (child.tag == "Body")
                {
                    if (child.position.x > furthestRight)
                    {
                        furthestRight = child.position.x;
                    }
                }
            }
        }
        playerCam.transform.position = new Vector3(furthestRight, 7.5f, -21);
    }
}
