using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(UmaActions))]
[RequireComponent(typeof(UmaHealth))]
public class UmaAgent : Agent
{
    [Header("Config")]
    public UmaActions petActions;
    public UmaHealth petHealth;
    public List<string> possiblePetOrders;
    public bool isTraining = true;
    public StatsMenu statsMenu = null;
    private Dictionary<string, float> orderInputs = new();

    public void ReactTo(Dictionary<string, float> orders)
    {
        if(isTraining) return;
        SetActiveOrders(orders);
        RequestDecision();
        RequestAction();
    }

    public void SetActiveOrders(Dictionary<string, float> orders){
        orderInputs.Clear();
        foreach(string order in possiblePetOrders)
        {
            if(orders.ContainsKey(order)) orderInputs.Add(order, orders[order]);
            else orderInputs.Add(order, 0);
        }
    }


    public override void Initialize()
    {
        petActions = GetComponent<UmaActions>();
        petHealth = GetComponent<UmaHealth>();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode starting");
        orderInputs = new();
        petHealth.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (isTraining)
        {
            int stage = Mathf.RoundToInt(
                Academy.Instance.EnvironmentParameters
                    .GetWithDefault("stage", 2f));
            Dictionary<string, float> trainingOrders = new();
            if(stage == 0)
            {
                //first stage, learn to survive
                foreach(string order in possiblePetOrders)
                    trainingOrders.Add(order, 0f);
            }
            else if(stage == 1)
            {
                //pick random action
                List<string> orders = petActions.GetRandomAction().ordersMet;
                //get its associated orders
                //set those to 1, rest to 0
                foreach(string order in possiblePetOrders)
                {
                    if(orders.Contains(order)) trainingOrders.Add(order, 1);
                    else trainingOrders.Add(order, 0f);
                }
            }
            else
            {
                List<string> orders = petActions.GetRandomAction().ordersMet;
                string pickedOrder = orders[Random.Range(0, orders.Count - 1)];
                string secondOrder = orders[Random.Range(0, orders.Count - 1)];
                //get its associated orders
                //set those to 1, rest to 0
                foreach(string order in possiblePetOrders)
                {
                    if(pickedOrder.Equals(order)) trainingOrders.Add(order, Random.Range(0.6f, 1f));
                    else if(secondOrder.Equals(order)) trainingOrders.Add(order, Random.Range(0.2f, 0.5f));
                    else trainingOrders.Add(order, Random.Range(0, 0.15f));
                }
                /*
                foreach(string order in possiblePetOrders)
                    trainingOrders.Add(order, Random.Range(0f, 1f));
                int ordersAtZero = Random.Range(0, possiblePetOrders.Count - 1);

                // Shuffle keys to choose random entries to set to 0
                List<string> keysCopy = new(trainingOrders.Keys);
                for (int i = 0; i < keysCopy.Count; i++)
                {
                    string temp = keysCopy[i];
                    int randomIndex = Random.Range(i, keysCopy.Count);
                    keysCopy[i] = keysCopy[randomIndex];
                    keysCopy[randomIndex] = temp;
                }

                // Set the designated number of entries to 0
                for (int i = 0; i < ordersAtZero; i++)
                {
                    trainingOrders[keysCopy[i]] = 0f;
                }   */
            }

            SetActiveOrders(trainingOrders);
        }

        sensor.AddObservation(1-petHealth.EnergyPercentage);
        sensor.AddObservation(1-petHealth.HealthPercentage);
        foreach(float order in orderInputs.Values)
        {
            sensor.AddObservation(order);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(orderInputs.Count == 0)
        {
            Debug.Log("Action received with no inputs");
            return;            
        }
        float max = float.MinValue;
        int highestIdx = -1;
        for(int i = 0; i < petActions.TotalActionCount(); i++)
        {
            float idxVal = Mathf.Clamp(actions.ContinuousActions[i], -1f, 1f);
            if(idxVal >= max)
            {
                max = idxVal;
                highestIdx = i;
            }
        }
        
        //do actions.
        petHealth.Tick();
        
        if(highestIdx < 0)
        {
            //if the agent didnt pick a winner, punish them.
            AddReward(-1f);
            return;   
        }
        
        List<string> ordersMet = petActions.InvokeAction(highestIdx, petHealth, gameObject);
        foreach(KeyValuePair<string, float> orderPair in orderInputs)
        {
            string orderKey = orderPair.Key;
            float orderWeight = orderPair.Value;
            if(ordersMet.Contains(orderKey)){
                //if the invoked action met this order, reward based on the weight1
                AddReward(orderWeight);
            }
            else{
                //if the invoked action didnt meet this order, punish based on order weight
                AddReward(-orderWeight);
            }
        }
        if (petHealth.IsDead())
        {
            //punish
            SetReward(-200f);
            Debug.Log("Pet died");
            EndEpisode();
        }
        statsMenu?.UpdateStats(petHealth);
        //after each action done, clear the input stack.
        orderInputs.Clear();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
    }
}
