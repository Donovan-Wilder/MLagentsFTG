using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using System.IO;

public class FTGAgent : Agent
{
    [SerializeField] public Transform goalTransform;
    public float rayDistance=3f;
    private float raySightDistance;
    private int raySightObject;
    public float moveSpeed = 3f;
    public float rotateAmount= 3f;
    private float totalEpisodes;
    private float successEpisodes;
    private float failedEpisodes;
    private float timedOutEpisodes;



    void FixedUpdate()
    {
        RaycastHit raycastHit;
        if(Physics.Raycast(transform.localPosition,transform.forward,out raycastHit, rayDistance)){
            if(raycastHit.collider.tag == "Wall")
                raySightObject = 1;
            if(raycastHit.collider.tag == "Goal")
                raySightObject = 2;
            
            raySightDistance = raycastHit.distance;
        }else{
            raySightObject =0;
            raySightDistance =0;
        }
        Debug.DrawRay(transform.localPosition,transform.forward*rayDistance);
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-4.25f,4.25f),0f,UnityEngine.Random.Range(-4.25f,4.25f));
        goalTransform.localPosition =new Vector3(UnityEngine.Random.Range(-4.25f,4.25f),0f,UnityEngine.Random.Range(-4.25f,4.25f));
        transform.localRotation.Set(0f,0f,0f,0f);
        transform.rotation=Quaternion.Euler(new Vector3(0,0,0));
        totalEpisodes++;
        if(totalEpisodes!=(successEpisodes+failedEpisodes)-1){
            timedOutEpisodes=totalEpisodes-(successEpisodes+failedEpisodes)-1;
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(transform.localRotation.y);
        sensor.AddObservation(raySightDistance);
        sensor.AddObservation(raySightObject);

        // String raySightObjectString ="";
        // if(raySightObject ==1){
        //     raySightObjectString = "Wall";
        // }else if(raySightObject ==2){
        //     raySightObjectString = "Goal";
        // }else {
        //     raySightObjectString ="Nothing";
        // }
        // Debug.Log($"Observations|| position: ({Math.Round(transform.localPosition.x,2)},{Math.Round(transform.localPosition.y,2)},{Math.Round(transform.localPosition.z,2)}) | rotation: {Math.Round(transform.localRotation.y,2)} | ray:: distance: {Math.Round(raySightDistance, 2)}, object: {raySightObjectString}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetAxis("Jump")>0?1:0;
        float rotate = Input.GetAxisRaw("Horizontal");
        if(rotate < 0){
            discreteActions[1] = 1;
        }
        if(rotate > 0){
            discreteActions[1] = 2;
        }
        if(rotate == 0){
            discreteActions[1] = 0;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.DiscreteActions[0];
        float rotate = 0f; 
        
        if( actions.DiscreteActions[1] == 0){
            rotate =0;
        }
        if( actions.DiscreteActions[1] ==1){
            rotate = -rotateAmount;
        }
        if(actions.DiscreteActions[1] ==2){
            rotate = rotateAmount;
        }
        
        transform.Rotate(transform.up, rotate);
        transform.Translate(0f,0f,move*moveSpeed*Time.deltaTime);
    }


    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Trigger: " + other.tag);
        if(other.transform.tag=="Wall"){
            SetReward(-100);
            failedEpisodes++;
        }
        if(other.tag =="Goal"){
            SetReward(100f);
            successEpisodes++;
        }

        float successPercent =totalEpisodes == 0f?0f:(successEpisodes/totalEpisodes)*100f;
        float failurePercent =totalEpisodes == 0f?0f:(failedEpisodes/totalEpisodes)*100f;
        float timeoutPercent =totalEpisodes == 0f?0f:(timedOutEpisodes/totalEpisodes)*100f;
        Debug.Log($"Total: {totalEpisodes} | Success: {Math.Round(successPercent,2)}%({successEpisodes}) | Failed: {Math.Round(failurePercent,2)}%({failedEpisodes}) | Timeout: {Math.Round(timeoutPercent,2)}%({timedOutEpisodes})");
        EndEpisode();
        
    }

    private Vector3 rotateDirection(Vector3 direction, float turn){

        Quaternion rotation = Quaternion.Euler(0,turn, 0);
        Vector3 rotateDirection = rotation *direction;
        return rotateDirection;
    }
}
