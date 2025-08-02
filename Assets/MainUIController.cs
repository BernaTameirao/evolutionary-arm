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
    //private DStarPathFinding DStarScript;
    private Gridi GridScript;
    private RobotGrid RobotGridScript;
    private FpsCounter FpsScript;
    private ObjectSelector SelectorScript;
    private MoveObject cameraScript;
    private MoveObject goalScript;

    private int sliderValue;
    private int[] newValuesFromInputs;
    private bool isShowingPath = true;
    private GameObject clickedObject;

    public bool isCollectingData = false;
    private int simulationCounter = 0;
    
    private bool isShowingFog = true;

    void Start()
    {
        EvScript = GetComponent<Evolution>();
        SceneScript = GetComponent<CreateScene>();
        PathFind = GetComponent<PathFinding>();
        //DStarScript = GetComponent<DStarPathFinding>();
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
        //childrenArray[5].text = "Generations per objective: " + uiData[5];
        childrenArray[5].text = "Segment lenght: " + uiData[5];
        childrenArray[6].text = "Minimum obstacle distance (Real): " + uiData[6];
        childrenArray[7].text = "Minimum obstacle distance (Robot Perception): " + uiData[7];
        childrenArray[8].text = "Simulation Counter: " + simulationCounter;
        childrenArray[9].text = "Fitness Type: " + EvScript.whichFitness;
        childrenArray[10].text = "FPS: " + FpsScript.getFPS();

        HandleMovement();

    }
    
    private void HandleMovement(){

        GameObject objectClicked = SelectorScript.getClickedObject();

        if(objectClicked!= null){

            cameraScript.deactivate();

            if(objectClicked != clickedObject && clickedObject != null){

                Destroy(clickedObject.GetComponent<MoveObject>());
            }

            clickedObject = objectClicked;
            if(clickedObject.GetComponent<MoveObject>() == null){

                clickedObject.AddComponent<MoveObject>();
                clickedObject.GetComponent<MoveObject>().cameraTransform = mainCamera.transform;
            }

        } else {

            cameraScript.activate();

            if( clickedObject != null){

                Destroy(clickedObject.GetComponent<MoveObject>());
            }
        }
    }

    // Generic click handler
    private void OnButtonClicked(string buttonName)
    {

        switch (buttonName)
        {
            case "ChangeObstacles":

                EvScript.clearObstacles();
                SceneScript.createRandomObstacles(sliderValue);
                resetSimulation();
                break;

            case "ChangeGeneration":
                EvScript.maxGenerations = newValuesFromInputs[0];
                resetSimulation();
                break;

            case "ChangePopulation":
                EvScript.popSize = newValuesFromInputs[1];
                resetSimulation();
                break;

            case "ChangeMaxStep":
                EvScript.maxStep = newValuesFromInputs[2];
                resetSimulation();
                break;

            case "ShowPath":
                isShowingPath = !isShowingPath;
            
                if(isShowingFog){

                    RobotGridScript.setEnabled(isShowingPath);
                } else {

                    GridScript.setEnabled(isShowingPath);
                }
                break;

            case "ChangeNumSegments":
                SceneScript.destroyRobotArm();
                SceneScript.N = newValuesFromInputs[3];
                SceneScript.spawnObject();
                resetSimulation();
                break;

            case "ResetAStar":
                resetSimulation();
                break;

            case "CollectData":
                EvScript.maxGenerations = 2300;
                simulationCounter = 0;
                isCollectingData = true;
                //EvScript.clearObstacles();
                resetSimulationWithObstacles();
                break;

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

    public void resetSimulation() {

        PathFind.Awake();
        EvScript.Awake();
        PathFind.activate();
    }

    public void resetSimulationWithObstacles() {
        
        EvScript.clearObstacles();
        SceneScript.createRandomObstacles(sliderValue);
        PathFind.Awake();
        EvScript.Awake();
        PathFind.activate();
    }

    public int getSimulationCounter(){

        return simulationCounter;
    }

    public void setSimulationCounter(int n){

        if(n == -1){
            simulationCounter += 1;
        }
        else {
            simulationCounter = n;
        }
    }
}
