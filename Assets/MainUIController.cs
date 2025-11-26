using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public class MainUIController : MonoBehaviour
{
    public UIDocument uiDocument;
    public GameObject mainCamera;

    private VisualElement root;
    private Label[] childrenArray;

    private Evolution EvScript;
    private CreateScene SceneScript;
    private PathFinding PathFind;
    private Gridi GridScript;
    private RobotGrid RobotGridScript;
    private FpsCounter FpsScript;
    private ObjectSelector SelectorScript;
    private MoveObject cameraScript;
    private MoveObject goalScript;

    private int sliderValue;
    private int[] newValuesFromInputs;
    private bool isShowingPath = true;
    private GameObject previousObjectClicked;

    public bool isCollectingData = false;
    private int simulationCounter = 0;
    
    private bool isShowingFog = false;

    void Start()
    {
        EvScript = GetComponent<Evolution>();
        SceneScript = GetComponent<CreateScene>();
        PathFind = GetComponent<PathFinding>();
        GridScript = GetComponent<Gridi>();
        RobotGridScript = GetComponent<RobotGrid>();
        FpsScript = GetComponent<FpsCounter>();
        SelectorScript = GetComponent<ObjectSelector>();
        cameraScript = mainCamera.GetComponent<MoveObject>();

        // Access the root VisualElement
        root = uiDocument.rootVisualElement;

        /////////////////////////////////////////////////////////////////////////////////////
        // Get all buttons and assign a common click handler
        var buttons = root.Query<Button>().ToList();
        foreach (var button in buttons)
        {
            button.clicked += () => OnButtonClicked(button.name);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        SliderInt volumeSlider = root.Q<SliderInt>("NumObstacles");

        // Get the initial value
        sliderValue = volumeSlider.value;

        // Add a callback to handle value changes
        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            sliderValue = volumeSlider.value;
        });

        //////////////////////////////////////////////////////////////////////////////////////
        Foldout foldout = root.Q<Foldout>("DataFoldOut");
        childrenArray = foldout.Children().OfType<Label>().ToArray();

        //////////////////////////////////////////////////////////////////////////////////////
        var textInputs = root.Query<IntegerField>().ToList();
        newValuesFromInputs = new int[textInputs.Count];
        
        int counter = 0;
        foreach (var input in textInputs)
        {

            // Capture the current value of counter
            int currentIndex = counter;

            input.RegisterValueChangedCallback(evt =>
            {
                newValuesFromInputs[currentIndex] = evt.newValue; // Use captured currentIndex
            });

            counter++;
        }

        //////////////////////////////////////////////////////////////////////////////////////

        var evaluationDropdown = root.Q<DropdownField>("EvaluationTypes");
        // Populate the dropdown with options
        evaluationDropdown.choices = new List<string> {"Option 1", "Option 2", "Option 3", "Option 4", "Option 5"};
        // Set the default value
        evaluationDropdown.value = "Option 1";

        // Register a callback for when the user changes the selection
        evaluationDropdown.RegisterValueChangedCallback(evt =>
        {
            switch(evt.newValue){

                case "Option 1":
                    EvScript.whichFitness = 1;
                    break;

                case "Option 2":
                    EvScript.whichFitness = 2;
                    break;

                case "Option 3":
                    EvScript.whichFitness = 3;
                    break;

                case "Option 4":
                    EvScript.whichFitness = 4;
                    break;

                case "Option 5":
                    EvScript.whichFitness = 5;
                    break;

                default:
                    break;
            }
        });
    }

    void Update()
    {
        
        float[] uiData = EvScript.getUIData();

        childrenArray[0].text = "Time elapsed(ms): " + uiData[0];
        childrenArray[1].text = "Generations: " + uiData[1];
        childrenArray[2].text = "Number of joints: " + uiData[2];
        childrenArray[3].text = "Population size: " + uiData[3];
        childrenArray[4].text = "MaxStep: " + uiData[4];
        childrenArray[5].text = "Segment lenght: " + uiData[5];
        childrenArray[6].text = "Goal Distance: " + uiData[6];
        childrenArray[7].text = "Minimum obstacle distance (Real): " + uiData[7];
        childrenArray[8].text = "Minimum obstacle distance (Robot Perception): " + uiData[8];
        childrenArray[9].text = "Simulation Counter: " + simulationCounter;
        childrenArray[10].text = "Fitness Type: " + EvScript.whichFitness;
        childrenArray[11].text = "FPS: " + FpsScript.getFPS();

        HandleMovement();

    }
    
    /// <summary>
    /// Controls the movement of the objects clicked by the user, and the camera.
    private void HandleMovement(){
        
        // Gets the object clicked by the user using raycast
        GameObject objectClicked = SelectorScript.getClickedObject();

        if(objectClicked != null){
            
            // Camera movement is deactivated
            cameraScript.deactivate();
            
            // If the object clicked is different from the previous, destroy its script to prevent unwanted movement.
            if(objectClicked != previousObjectClicked && previousObjectClicked != null){

                Destroy(previousObjectClicked.GetComponent<MoveObject>());
            }
            
            previousObjectClicked = objectClicked;

            // Adds the movement script if it didn't exist before
            if(previousObjectClicked.GetComponent<MoveObject>() == null){

                previousObjectClicked.AddComponent<MoveObject>();
                previousObjectClicked.GetComponent<MoveObject>().cameraTransform = mainCamera.transform;
            }

        } else {
            
            // Camera movement is activated if no object is clicked by the user
            cameraScript.activate();

            // Destroy the script of the previous object to prevent unwanted movement.
            if(previousObjectClicked != null){

                Destroy(previousObjectClicked.GetComponent<MoveObject>());
            }
        }
    }

    /// <summary>
    /// Generic click handler
    /// <parameters>
    /// buttonName (string): Name of the button.
    private void OnButtonClicked(string buttonName)
    {

        switch (buttonName)
        {
            // Used to change the number of obstacles in the environment
            case "ChangeObstacles":

                resetSimulationWithObstacles();
                break;

            // Used to change the number of max generations
            case "ChangeGeneration":
                EvScript.maxGenerations = newValuesFromInputs[0];
                resetSimulation();
                break;  

            // Used to change the number of individuals in a population
            case "ChangePopulation":
                EvScript.popSize = newValuesFromInputs[1];
                resetSimulation();
                break;

            // Used to change the max mutation value
            case "ChangeMaxStep":
                EvScript.maxStep = newValuesFromInputs[2];
                resetSimulation();
                break;

            // Used to show/hide the a* path
            case "ShowPath":
                isShowingPath = !isShowingPath;
            
                if(isShowingFog){

                    RobotGridScript.setEnabled(isShowingPath);
                } else {

                    GridScript.setEnabled(isShowingPath);
                }
                break;

            // Used to change the number of segments on the robotic arm
            case "ChangeNumSegments":
                SceneScript.destroyRobotArm();
                SceneScript.N = newValuesFromInputs[3];
                SceneScript.spawnObject();
                resetSimulation();
                break;

            // Reset the simulation
            case "ResetAStar":
                resetSimulation();
                break;

            // Used to start collecting data
            case "CollectData":
                EvScript.maxGenerations = 8000;
                simulationCounter = 0;
                isCollectingData = true;
                //EvScript.clearObstacles();
                resetSimulationWithObstacles();
                break;

            // Used to activate/deactivate the fog of war
            case "FogOfWar":
                isShowingFog = !isShowingFog;

                if(isShowingFog){
                    
                    RobotGridScript.setEnabled(isShowingPath);
                    GridScript.setEnabled(false);
                } else {

                    GridScript.setEnabled(isShowingPath);
                    RobotGridScript.setEnabled(false);
                }

                EvScript.setFogOfWar(isShowingFog);
                resetSimulation();
                break;

            default:
                Debug.Log("Unknown button clicked");
                break;
        }
    }

    /// <summary>
    /// Soft resets the simulation and keeps the same environment and grid.
    public void resetSimulation() {

        PathFind.Awake();
        EvScript.Awake();
        PathFind.activate();
    }

    /// <summary>
    /// Hard resets the simulation, changes the environment and remakes the grid.
    public void resetSimulationWithObstacles() {
        
        EvScript.clearObstacles();
        SceneScript.createRandomObstacles(sliderValue, !SceneScript.isCreatingTunnels);
        PathFind.Awake();
        EvScript.Awake();
        PathFind.activate();
    }

    /// <summary>
    /// Returns the simulation counter.
    public int getSimulationCounter(){

        return simulationCounter;
    }

    /// <summary>
    /// Sets a value into the simulation counter.
    /// <parameters>
    /// n (int): the value.
    public void setSimulationCounter(int n){

        if(n == -1){
            simulationCounter += 1;
        }
        else {
            simulationCounter = n;
        }
    }
}
