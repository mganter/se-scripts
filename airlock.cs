public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

const string intentionBlock = "Airlock Intention State";
const bool PRESSURIZE = true;

const string airVentName = "Airlock Ventilation";
const string outboundDoorGroup = "Airlock Door Outside";
const string inboundDoorGroup = "Airlock Door Inside";
const string oxygenStorageName = "Airlock Oxygen Tank";
const string ligthingGroupName = "Airlock Lighting";


public void Main(string argument, UpdateType updateSource)
{
    bool shouldBePressurized = getPressurizeIntention();
    Echo("Pressurize: " + shouldBePressurized);

    if (shouldBePressurized) Pressurize(); else Depressurize();
}

public void Pressurize()
{
    List<IMyDoor> outboundDoors = getDoorsByGroup(outboundDoorGroup);
    List<IMyDoor> inboundDoors = getDoorsByGroup(inboundDoorGroup);

    bool operationRunning = !closeDoors(outboundDoors);
    if (operationRunning) {
        EnableLightingAlert();
    }

    IMyAirVent airVent = getAirVent();
    if (!airVent.CanPressurize)
    {
        closeDoors(inboundDoors);
        return;
    };
    airVent.Depressurize = false;

    if (airVent.GetOxygenLevel() >= .7)
    {
        operationRunning = !openDoors(inboundDoors);
        if(!operationRunning) DisableLightingAlert(Color.Green);
    }
}


public void Depressurize()
{
    List<IMyDoor> outboundDoors = getDoorsByGroup(outboundDoorGroup);
    List<IMyDoor> inboundDoors = getDoorsByGroup(inboundDoorGroup);

    bool operationRunning = !closeDoors(inboundDoors);
    if (operationRunning) {
        EnableLightingAlert();
    }

    IMyAirVent airVent = getAirVent();
    airVent.Depressurize = true;

    IMyGasTank oxygenStorage = getOxygenStorage();
    if (airVent.GetOxygenLevel() <= .01 || oxygenStorage.FilledRatio > .9)
    {
        operationRunning = !openDoors(outboundDoors);
        if(!operationRunning) DisableLightingAlert(Color.Red);
    }
}

public void EnableLightingAlert(){
    List<IMyLightingBlock> blocks = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlockGroupWithName(ligthingGroupName).GetBlocksOfType<IMyLightingBlock>(blocks, block => block.CubeGrid == Me.CubeGrid);

    for(int i = 0; i<blocks.Count; i++){
        blocks[i].Color = Color.OrangeRed;
        blocks[i].BlinkIntervalSeconds = 1;
        blocks[i].BlinkLength = .5F;
    }
}

public void DisableLightingAlert(Color color){
    List<IMyLightingBlock> blocks = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlockGroupWithName(ligthingGroupName).GetBlocksOfType<IMyLightingBlock>(blocks, block => block.CubeGrid == Me.CubeGrid);

    for(int i = 0; i<blocks.Count; i++){
        blocks[i].Color = color;
        blocks[i].BlinkIntervalSeconds = 0;
        blocks[i].BlinkLength = 1F;
    }
}

public bool openDoors(List<IMyDoor> doors)
{
    bool ret = true;
    for (int i = 0; i < doors.Count; i++)
    {
        if(doors[i].Status != DoorStatus.Open) {
            doors[i].Enabled = true;
            doors[i].OpenDoor();
            ret = false;
        } else {
            doors[i].Enabled = false;
        }
    }
    return ret;
}

public bool closeDoors(List<IMyDoor> doors)
{
    bool ret = true;
    for (int i = 0; i < doors.Count; i++)
    {
        if(doors[i].Status != DoorStatus.Closed) {
            doors[i].Enabled = true;
            doors[i].CloseDoor();
            ret = false;
        } else {
            doors[i].Enabled = false;
        }
    }
    return ret;
}

public List<IMyDoor> getDoorsByGroup(string groupName)
{
    List<IMyDoor> doorBlocks = new List<IMyDoor>();
    GridTerminalSystem.GetBlockGroupWithName(groupName).GetBlocksOfType<IMyDoor>(doorBlocks, block => block.CubeGrid == Me.CubeGrid);
    return doorBlocks;
}

public IMyAirVent getAirVent()
{
    List<IMyAirVent> airvent = new List<IMyAirVent>();
    GridTerminalSystem.GetBlocksOfType<IMyAirVent>(airvent, block => airVentName.Equals(block.CustomName) && block.CubeGrid == Me.CubeGrid);
    return airvent[0];
}

public IMyGasTank getOxygenStorage()
{
    List<IMyGasTank> oxygenStorage = new List<IMyGasTank>();
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(oxygenStorage, block => oxygenStorageName.Equals(block.CustomName) && block.CubeGrid == Me.CubeGrid);
    if (oxygenStorage.Count == 0) Echo("No Oxygen Storage Found. Please name the block correctly.");
    return oxygenStorage[0];
}

public bool getPressurizeIntention()
{
    List<IMyFunctionalBlock> intentionBlocks = new List<IMyFunctionalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(intentionBlocks, block => intentionBlock.Equals(block.CustomName) && block.CubeGrid == Me.CubeGrid);
    if (intentionBlocks.Count == 0) Echo("No State Block Found. Please name the block correctly.");
    return intentionBlocks[0].Enabled == PRESSURIZE;
}
