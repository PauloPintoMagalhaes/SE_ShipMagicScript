using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Ship_Main : MyGridProgram
    {
        //string conversions From and To SE Type and SubType codes to userfriendly terms
        //This function is not very well made, but it worked on a previous version of the code and I'm too tired to
        //"pimp" it up now. 
        public List<string> cToSE_Key(string thisItem)
        {
            //Some new items may be missing
            string[] components = new string[]{"Construction", "MetalGrid", "InteriorPlate", "SteelPlate", "Girder", "SmallTube", "LargeTube", "Motor", "Display", "Glass",
                "Superconductor", "Computer", "Reactor", "Thrust", "GravityGen", "Medical", "RadioComm", "Detector", "Explosives", "SolarCell", "PowerCell"};
            string[] ores = new string[] { "Ice", "Stone", "Gold Ore", "Iron Ore", "Silver Ore", "Cobalt Ore", "Nickel Ore", "Uranium Ore", "Silicon Ore", "Platinum Ore", "Magnesium Ore" };
            string[] ingots = new string[] { "Gravel", "Gold Ingot", "Silver Ingot", "Nickel Ingot", "Iron Ingot", "Silicon Ingot", "Platinum Ingot", "Magnesium Ingot" };
            string[] tools = new string[] { "Drill", "Drill2", "Drill3", "Drill4", "WelderI", "WelderI2", "WelderI3", "WelderI4", "Grinder", "Grinder2", "Grinder3", "Grinder4" };
            string[] ammo = new string[] { "Missile", "Ammo045mm", "Ammo184mm" };
            string SE_Type = "";
            string SE_SubType = "";
            //Determines Type
            if (components.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Component"; }
            else if (ores.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Ore"; }
            else if (ingots.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Ingot"; }
            else if (tools.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_PhysicalGunObject"; }
            else if (ammo.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_AmmoMagazine"; }
            else if (thisItem == "OxygenBottle") { SE_Type = "MyObjectBuilder_OxygenContainerObject"; }
            else if (thisItem == "HydrogenBottle") { SE_Type = "MyObjectBuilder_GasContainerObject"; }
            else if (thisItem == "ClankCola") { SE_Type = "MyObjectBuilder_ConsumableItem"; }
            else { SE_Type = ""; }
            //Determines SubType
            if (thisItem == "Glass") { SE_SubType = "BulletproofGlass"; }
            else if (thisItem == "GravityGen") { SE_SubType = "GravityGenerator"; }
            else if (thisItem == "RadioComm") { SE_SubType = "RadioCommunication"; }
            else if (thisItem == "Drill") { SE_SubType = "HandDrillItem"; }
            else if (thisItem == "Drill2") { SE_SubType = "HandDrill2Item"; }
            else if (thisItem == "Drill3") { SE_SubType = "HandDrill3Item"; }
            else if (thisItem == "Drill4") { SE_SubType = "HandDrill4Item"; }
            else if (thisItem == "Welder") { SE_SubType = "WelderItem"; }
            else if (thisItem == "Welder2") { SE_SubType = "Welder2Item"; }
            else if (thisItem == "Welder3") { SE_SubType = "Welder3Item"; }
            else if (thisItem == "Welder4") { SE_SubType = "Welder4Item"; }
            else if (thisItem == "Grinder") { SE_SubType = "AngleGrinderItem"; }
            else if (thisItem == "Grinder2") { SE_SubType = "AngleGrinder2Item"; }
            else if (thisItem == "Grinder3") { SE_SubType = "AngleGrinder3Item"; }
            else if (thisItem == "Grinder4") { SE_SubType = "AngleGrinder4Item"; }
            else if (thisItem == "OxygenBottle") { SE_SubType = "OxygenContainerObject"; }
            else if (thisItem == "HydrogenBottle") { SE_SubType = "GasContainerObject"; }
            else if (thisItem == "Missile") { SE_SubType = "Missile200mm"; }
            else if (thisItem == "Ammo045mm") { SE_SubType = "NATO_5p56x45mm"; }
            else if (thisItem == "Ammo184mm") { SE_SubType = "NATO_25x184mm"; }
            else if (thisItem.Contains(" ")) { SE_SubType = null; }
            else { SE_SubType = thisItem; }

            return new List<string> { SE_Type, SE_SubType };
        }

        //determines SE code type from a list of more reader friendly codes
        public string cToSE_Type(string vfNormal_Type)
        {
            string vlSE_Type;
            if (vfNormal_Type == "Ores") { vlSE_Type = "MyObjectBuilder_Ore"; }
            else if (vfNormal_Type == "Ingots") { vlSE_Type = "MyObjectBuilder_Ingot"; }
            else if (vfNormal_Type == "Components") { vlSE_Type = "MyObjectBuilder_Component"; }
            else if (vfNormal_Type == "Tools") { vlSE_Type = "MyObjectBuilder_PhysicalGunObject"; }
            else if (vfNormal_Type == "Ammo") { vlSE_Type = "MyObjectBuilder_AmmoMagazine"; }
            else if (vfNormal_Type == "O2Bottles") { vlSE_Type = "MyObjectBuilder_OxygenContainerObject"; }
            else if (vfNormal_Type == "H2Bottles") { vlSE_Type = "MyObjectBuilder_GasContainerObject"; }
            else if (vfNormal_Type == "Consumables") { vlSE_Type = "MyObjectBuilder_ConsumableItem"; }
            else { vlSE_Type = ""; }
            return vlSE_Type;
        }

        //converts a game subtype to a more understandable, printable name
        public string cFromSE_Key(string SE_SubType, string SE_Type)
        {
            string nonSE_Name;
            //Don't know who had the bright idea to name different items with the same subtype. Makes things unnecessarily hard
            if (SE_Type == "MyObjectBuilder_Ore" || SE_Type == "MyObjectBuilder_Ingot")
            {
                if (SE_SubType == "Ice" || SE_SubType == "Stone" || SE_SubType == "Gravel")
                {
                    nonSE_Name = "MyObjectBuilder_Ore";
                }
                else
                {
                    string vlNewType = (SE_Type == "MyObjectBuilder_Ore") ? "Ore" : "Ing";
                    nonSE_Name = SE_SubType + " " + vlNewType;
                }
            }
            else if (SE_SubType == "ClangCola")
            {
                nonSE_Name = "Clang Cola";
            }
            else
            {
                if (SE_SubType == "BulletproofGlass") { nonSE_Name = "Glass"; }
                else if (SE_SubType == "GravityGenerator") { nonSE_Name = "GravityGen"; }
                else if (SE_SubType == "RadioCommunication") { nonSE_Name = "RadioComm"; }
                else if (SE_SubType == "HandDrillItem") { nonSE_Name = "Drill"; }
                else if (SE_SubType == "HandDrill2Item") { nonSE_Name = "Drill2"; }
                else if (SE_SubType == "HandDrill3Item") { nonSE_Name = "Drill3"; }
                else if (SE_SubType == "HandDrill4Item") { nonSE_Name = "Drill4"; }
                else if (SE_SubType == "WelderItem") { nonSE_Name = "Welder"; }
                else if (SE_SubType == "Welder2Item") { nonSE_Name = "Welder2"; }
                else if (SE_SubType == "Welder3Item") { nonSE_Name = "Welder3"; }
                else if (SE_SubType == "Welder4Item") { nonSE_Name = "Welder4"; }
                else if (SE_SubType == "AngleGrinderItem") { nonSE_Name = "Grinder"; }
                else if (SE_SubType == "AngleGrinder2Item") { nonSE_Name = "Grinder2"; }
                else if (SE_SubType == "AngleGrinder3Item") { nonSE_Name = "Grinder3"; }
                else if (SE_SubType == "AngleGrinder4Item") { nonSE_Name = "Grinder4"; }
                else if (SE_SubType == "OxygenContainerObject") { nonSE_Name = "OxygenBottle"; }
                else if (SE_SubType == "GasContainerObject") { nonSE_Name = "HydrogenBottle"; }
                else if (SE_SubType == "Missile200mm") { nonSE_Name = "Missile200mm"; }
                else if (SE_SubType == "NATO_5p56x45mm") { nonSE_Name = "NATO_5p56x45mm"; }
                else if (SE_SubType == "NATO_25x184mm") { nonSE_Name = "NATO_25x184mm"; }
                else { nonSE_Name = SE_SubType; }
            }
            return nonSE_Name;
        }


        //Methods to determine grid wonership and blocksearch filtering by grid
        //Determines whether a block belongs to the same grid as the programming block calling it or not
        private bool isLocalGrid(IMyTerminalBlock block)
        {
            return block.CubeGrid == Me.CubeGrid;
        }

        private bool isExternalGrid(IMyTerminalBlock block)
        {
            return block.CubeGrid != Me.CubeGrid;
        }

        //searches every connector on this grid. If at least one is connected, returns true, if none are connected returns false
        private bool isConnected()
        {
            bool vlTMP = false;
            var vlConLst = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlConLst, isLocalGrid);
            if (vlConLst != null && vlConLst.Count > 0)
            {
                foreach (IMyShipConnector vlCon in vlConLst)
                {
                    //Status=0: Unconnected, 1: Unconnectable, 2:Connected 
                    if ((int)vlCon.Status == 2 && vlCon.IsFunctional)
                    {
                        //Found connected connector. No point is searching further
                        vlTMP = true;
                        break;
                    }
                }
            }


            return vlTMP;
        }

        //checks if the ship is connected anywhere and if it is, gets the block info in question
        private IMyTerminalBlock getExernalConnector()
        {
            List<IMyTerminalBlock> vlExternal = new List<IMyTerminalBlock>();
            IMyTerminalBlock vlFound = null;
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlExternal, isLocalGrid);
            if (vlExternal != null && vlExternal.Count > 0)
            {
                int vlI = 0;
                foreach (IMyShipConnector vlCon in vlExternal)
                {
                    //Status=0: Unconnected, 1: Unconnectable, 2:Connected 
                    if ((int)vlCon.Status == 2 && vlCon.IsFunctional)
                    {
                        vlFound = vlExternal[vlI];
                        break;
                    }
                    vlI++;
                }
            }
            return vlFound;
        }


        //Cargo listing methods
        //Ideally, I'd like to put this inside the cargoClass itself, but I'm having trouble having the class recognize GridTerminalSystem properties
        //Also, users in Keen forum advised us to keep this inside the main program. Until better ideas pop up, here it goes.
        private List<IMyTerminalBlock> cargoType(string blockType = "All")
        {
            //NOTE:I am purpousefully ignoring the cargo in the cockpits and seats which I am considering a
            //"private zone" of sorts. If anything is to be added then IMyShipCockpit must be added individually
            var vlCargoList = new List<IMyTerminalBlock>();

            //Finds, lists and returns every type of block
            switch (blockType)
            {
                //adds the blocks of a type into a list and returns it
                case "Container":
                    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(vlCargoList, isLocalGrid); break;
                case "Connector":
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlCargoList, isLocalGrid); break;
                case "Drill":
                    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(vlCargoList, isLocalGrid); break;
                case "Welder":
                    GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(vlCargoList, isLocalGrid); break;
                case "Grinder":
                    GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(vlCargoList, isLocalGrid); break;
                case "Reactor":
                    GridTerminalSystem.GetBlocksOfType<IMyReactor>(vlCargoList, isLocalGrid); break;
                case "O2Generator":
                    GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(vlCargoList, isLocalGrid); break;
                default:
                    //this includes the "All" scenario. Joins up all the inventory types in one single list
                    var vlListTMP = new List<IMyTerminalBlock>();
                    //this works because all of these have a "IMyInventory" property that works in the exact same way
                    //As long as I don't try to do anything with this list other than checkin, counting and transfering data, this should be fine
                    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(vlCargoList, isLocalGrid);
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    GridTerminalSystem.GetBlocksOfType<IMyReactor>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    break;
            }
            return vlCargoList;
        }

        private List<IMyTerminalBlock> cargoType_Connected(IMyTerminalBlock vfConnetor, bool includeReac_H2O2 = false)
        {
            //explanations on cargoType()
            var vlCargoList = new List<IMyTerminalBlock>();
            //this includes the "All" scenario. Joins up all the inventory types in one single list
            var vlListTMP = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(vlCargoList, x => x.CubeGrid == vfConnetor.CubeGrid);
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
            if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
            if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
            if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
            if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            //Depending on the ship type we're on, we might want to include or not certain block types
            //ex: I don't want to drain my Drill ship of ice reserves that I need to turn into oxugen or drain my reactors of fuel
            //but I might want to fill my reactor with certain standard number of fuel to ensure I'm not left adrift in space
            if (includeReac_H2O2)
            {
                GridTerminalSystem.GetBlocksOfType<IMyReactor>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
                if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
                if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            }
            return vlCargoList;
        }

        private List<IMyTerminalBlock> cargoType_Facility(IMyTerminalBlock vfConnetor, string blockType = "All")
        {
            //This exists only to give the main ship something to throw its items at. 
            var vlCargoList = new List<IMyTerminalBlock>();
            var vlListTMP = new List<IMyTerminalBlock>();
            //since I can't put the result of isLocalGrid and isExternalGrid into a variable, I'm forced to double the ammount of code

            if (vfConnetor != null)
            {
                if (blockType == "Refinery") { GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, isLocalGrid); }
                else if (blockType == "Assembly") { GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlCargoList, isLocalGrid); }
                else
                {
                    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, isLocalGrid);
                    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                }
            }
            else
            {
                if (blockType == "Refinery") { GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, x => x.CubeGrid == vfConnetor.CubeGrid); }
                else if (blockType == "Assembly") { GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlCargoList, x => x.CubeGrid == vfConnetor.CubeGrid); }
                else
                {
                    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, x => x.CubeGrid == vfConnetor.CubeGrid);
                    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                }
            }

            return vlCargoList;
        }


        
        //Information Print methods
        private string buildPrintMsg(string value1, float value2, int vfI, int itemsPerLine = 1, bool isPercentage = false)
        {
            string vlMSG = string.Format(" {0}: #{1}", value1, value2);
            if (isPercentage) { vlMSG += "%"; }
            vlMSG += (vfI + 1 % itemsPerLine == 0) ? "\n" : " | ";
            return vlMSG;
        }

        private void printGroupInLCD(string vfLCD_Name, cargoClass data, string vfType = "", string vfStartNote = "", int itemsPerLine = 1, bool vfKeepCurrentMsg = false)
        {
            // to my ABSOLUTE dismay, I can't use local functions in SE due to it not accepting C# 7.0. I could have packed all this stuff into one single func and let it sort itself locally
            IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName(vfLCD_Name) as IMyTextPanel;
            if (vlLCD != null)
            {
                string vlMSG = vfStartNote;
                int vlI = 0;
                string vlTMP = "";
                int vlIndex = data.MaterialQuantity.FindIndex(a => a.Item1 == vfType); // Checks if it does have that type, otherwise no point in continuing
                if (vlIndex >= 0 || vfType == "") // Checks if it does have that type, otherwise no point in continuing
                {

                    foreach (MyTuple<string, string, float> line in data.MaterialQuantity)
                    {
                        //Cycles through the list and builds up a message with only the permitted items and in the specified conditions
                        if (vfType == line.Item1 || vfType == "")
                        {
                            vlTMP = cFromSE_Key(line.Item2, line.Item1);
                            vlMSG += buildPrintMsg(vlTMP, line.Item3, vlI, itemsPerLine = 1);
                            vlI++;
                        }
                    }
                    if (vlMSG != "")
                    {
                        //prints the message into the specified LCD keeping the previous message or not, depending on vfKeepCurrentMsg
                        vlLCD.WriteText(vlMSG, vfKeepCurrentMsg);
                    }
                }
            }
        }

        //This isn't strictly necessary, but since some of the fellows that are going to use this aren't programmers, might as well make it easier for them
        private void printItemType(string vfLCD_Name, cargoClass data, string vfType, string vfStartNote = "", bool vfKeepCurrentMsg = false, int itemsPerLine = 1)
        {
            string vlType = cToSE_Type(vfType);
            printGroupInLCD(vfLCD_Name, data, vlType, vfStartNote, itemsPerLine, vfKeepCurrentMsg);
        }

        //prints all types in the indicated screen
        private void printItemAll(string vfLCD_Name, cargoClass data, string vfStartNote = "", bool vfKeepCurrentMsg = false, int itemsPerLine = 1)
        {
            printGroupInLCD(vfLCD_Name, data, "", vfStartNote, itemsPerLine, vfKeepCurrentMsg);
        }

        //prints percentage of individual cargos
        private void printCargoPercentage(string vfLCD_Name, cargoClass[] data, string vfStartNote = "", bool vfKeepCurrentMsg = false, int itemsPerLine = 1)
        {
            IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName(vfLCD_Name) as IMyTextPanel;
            if (vlLCD != null && data != null)
            {
                string vlMSG = vfStartNote;
                int vlI = 0;
                foreach (cargoClass item in data)
                {
                    vlMSG += buildPrintMsg("", item.Racio, vlI, itemsPerLine, true);
                    vlI++;
                }
                if (vlMSG != "")
                {
                    //prints the message into the specified LCD keeping the previous message or not, depending on vfKeepCurrentMsg
                    vlLCD.WriteText(vlMSG, vfKeepCurrentMsg);
                }
            }
        }

        private void printCustomMsg(string vfLCD_Name)
        {
            IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName(vfLCD_Name) as IMyTextPanel;
            if (vlLCD != null)
            {
                string vlMSG = "";
                //build custom message here



                //print message
                vlLCD.WriteText(vlMSG);
            }
        }



        //transfering functions/methods . 
        private List<MyTuple<long, MyFixedPoint, List<MyTuple<string, string, MyFixedPoint, int>>>> trimByType (List<MyTuple<long, MyFixedPoint, List<MyTuple<string, string, MyFixedPoint, int>>>> vfOld, string vfType)
        {
            List<MyTuple<long, MyFixedPoint, List<MyTuple<string, string, MyFixedPoint, int>>>> vlNewList = null;
            List<MyTuple<string, string, MyFixedPoint, int>> vlSubList = null;
            foreach (var listLine in vfOld)
            {
                //id and freespace do not concern us here. we only check if type exists and if so, create list with it
                vlSubList = listLine.Item3.FindAll(a => a.Item1 == cToSE_Type(vfType));
                vlNewList.Add(MyTuple.Create(listLine.Item1, listLine.Item2, vlSubList));
            }

            return vlNewList;
        }
        //Filtered so it only tries to do anything if it's connected somewhere else and that else is something that can receive items
        private void drainAllOfType(cargoClass[] data, string vfType = "All", bool vfOrganize = true)
        {
            if (data != null && isConnected())
            {
                cargoClass externalCargo = new cargoClass();
                externalCargo.getCargo(cargoType_Connected(getExernalConnector()));
                foreach (cargoClass innerCargo in data)
                {
                    //Drains this type of item to indicated cargo
                    if (innerCargo.MaterialList.Count > 0)
                    {
                        //filters innerCargo.MaterialList of indicated vfType
                        var vlFiltered = (vfType == "All") ? innerCargo.MaterialList : trimByType(innerCargo.MaterialList, vfType); 
                        //search in innerList to send to externalList, decide if you send to the fist available cargo vfFirstFree or if 
                        //you first try to find a cargo with that same type of mats), provide a custom list
                        searchThroughLists(vlFiltered, externalCargo.MaterialList, vfOrganize);
                    }
                }
            }
        }

        private void fillWithAllOfType(cargoClass[] data, string vfType, bool vfOrganize = true)
        {
            if (data != null && isConnected())
            {
                List<IMyTerminalBlock> vlExternal = cargoType_Connected(getExernalConnector());
                foreach (cargoClass item in data)
                {
                    //???? WIP
                }
            }
        }

        private void fillWithList(cargoClass[] data, bool vfOrganize = true)
        {
            if (data != null && isConnected())
            {
                //I'm pondering the benefits of making this a statit class, even though it's "bad manners"
                transferClass vlList = new transferClass();
                //Filter the dictionary to remove values <= 0
                vlList.trimTransferList();  //by default, trims zero and negative values. 
                //To change this, vlList.SetToTrimZeroes = False and/or vlList.SetToTrimNegatives = False, and only then vlList.TrimTransfer List

                //Filter dictionary to remove items that are already inside the ship
                foreach (cargoClass innerCargo in data)
                {
                    vlList.updateTransferList(innerCargo.MaterialQuantity);
                }
                //No point in searching or transfering anything if the transfer list is empty.
                //If this happens it either means the user messed up or the ship already has every listed item
                if (vlList.TransferList.Count > 0 && vlList.TransferList.Any(a=> a.Item3 > 0)) {
                    IMyTerminalBlock vlReferenceConn = getExernalConnector();   //gets a reference connector block to determine which grid it belongs to
                    //Search for external cargo
                    cargoClass externalCargo = new cargoClass();
                    externalCargo.getCargo(cargoType_Connected(vlReferenceConn));
                    //adds facility cargos to the mix. Note that in this mode you should only be allowed to take items from inventory(1) which corresponds to processed materials
                    externalCargo.getCargo(cargoType_Facility(vlReferenceConn));
                    
                    foreach (cargoClass innerCargo in data)
                    {
                        if (innerCargo.MaterialList.Count > 0)
                        {
                            //search in externalCargo to send to innerCargo, decide if you send to the fist available cargo vfFirstFree or if 
                            //you first try to find a cargo with that same type of mats), provide a custom list
                            searchThroughLists(externalCargo.MaterialList, innerCargo.MaterialList, vfOrganize, vlList.TransferList);
                        }
                    }
                }
            }
        }

        private List<MyTuple<string, string, MyFixedPoint, int>> searchConditions( List<MyTuple<string, string, MyFixedPoint, int>> vfOrigList, List<MyTuple<string, string, float>> vfUseGlobalList)
        {
            List<MyTuple<string, string, MyFixedPoint, int>> vlFound = null; 
            //checks if the cargo being searched has any of the intended items
            if (vfUseGlobalList == null && vfOrigList != null) 
            {
                //If there is no specific search and origin has items to pick, then its a go for transfer
                vlFound = vfOrigList;
            }
            else if(vfUseGlobalList != null && vfOrigList != null)
            {
                //If there is a specific search and origin has items to pick, then lets search
                foreach (var originLine in vfOrigList)
                {
                    //if the item in the list, find everyone and passes the item location to 
                    var vlLine = vfUseGlobalList.Find(a=> a.Item1 == originLine.Item1 && a.Item2 == originLine.Item2);
                    if (vlLine.)
                    {

                    }
                    
                }
            }
            return vlFound;
        }

        private void searchThroughLists(List<MyTuple<long, MyFixedPoint, List<MyTuple<string, string, MyFixedPoint, int>>>> vfOrigList, List<MyTuple<long, MyFixedPoint, List<MyTuple<string, string, MyFixedPoint, int>>>> vfDestinList, bool vfOrganize = true, List<MyTuple<string, string, float>> vfUseGlobalList = null)
        {
            //We're going to have to change these lists, so we parse them here. We don't need to change anything in the class itself because the next cycle of the programming block will update the info anyway
            var originLstLst = vfOrigList; 
            var destinationLstLst = vfDestinList;
            int vlOrigIndex = 0;
            //int vlDestIndex = 0;
            //int vlItemIndex = 0;

            //Honestly, this is the part that I dislike the most
            foreach (var originLst in originLstLst)
            {
                //checks conditions for transfer based on search parametres
                if (searchConditions(originLst.Item3, vfUseGlobalList).Count > 0)
                {
                    //Checks if this cargo has any item that we need to transfer. I'd really love to be able to do local functions right now. SE doesn't accept net Core, only net Framework
                
                                           
                    //WIP this is starting to get a little complicated. Maybe it's best if I start making this particular function by the end result functions, and not the process order


                    //    //Else, if it found something, verify each of its items and attempts to find a match on the other list
                    //    if (!vfOrganize)
                    //    {
                    //        //Cycles through the list in search of any item of the following category
                    //        foreach (var destinationLst in destinationLstLst)
                    //        {
                    //            if (destinationLst.Item3 != null)
                    //            {
                    //                //skips empty lists
                    //                vlItemIndex = destinationLst.Item3.FindIndex(a => a.Item1 == originLine.Item1 && a.Item2 == originLine.Item2);
                    //                if (vlItemIndex >= 0) // Checks if it does have that type, otherwise no point in continuing  
                    //                {
                    //                    //We found an item to transfer. From here, we need the following information.
                    //                    //ID of the block to where it is now, ID of the block to receive it, inventory position where the item is now.
                    //                    var vlItemsTransfered = attemptItemTransfer(originLst.Item1, destinationLst.Item1, originLst.Item3[vlItemIndex].Item4);
                    //                    //???? WIP
                    //                }
                    //            }
                    //        }
                    //        vlDestIndex++;
                    //    }
                }
                vlOrigIndex++;
            }
        }

        //attempts to transfer vfQuantity of an item from vfOrigin in position vfPos and send it to vfDestination
        private MyFixedPoint attemptItemTransfer(long vfOriginID, long vfDestinID, int vfPos, float vfQuantity = 0)
        {
            MyFixedPoint vlTMP = 0;
            IMyInventory vlOrigin = GridTerminalSystem.GetBlockWithId(vfOriginID).GetInventory(0);
            //???? WIP

            return vlTMP;
        }



        //Main function... where the party starts and ends
        public void Main(string argument, UpdateType updateSource)
        {
            //Note that all these classes only take data from the ship where the propgramming block is being run
            cargoClass vlCargo = new cargoClass();
            //Fills up variabbles with different type of content. If empty, it will search All types of cargo containers at once
            //Options are: "Container", "Connector", "Drill", "Welder", "Grinder" and "Reactor" and "O2Generator"
            //You can wipe the variable clean with vlCargo.Clear();
            vlCargo.getCargo(cargoType("Container"));  //Already declared above
            vlCargo.getCargo(cargoType("Connector"));  //Note that Ejectors count as connectors too
            cargoClass vlReactor = new cargoClass();
            vlReactor.getCargo(cargoType("Reactor"));
            cargoClass vlTool = new cargoClass();
            vlTool.getCargo(cargoType("Drill"));

            //Print remaining Info
            //printItemType("Name of LCD", Cargo Variable, Type (see list below), text begins with (use if you want to mark the following print with something), keep old text, Items per line);
            //Ores, Ingots, Components, Tools, Ammo, O2Bottles, H2Bottles, Consumablers 
            printItemType("LCD", vlCargo, "Ores", "Cargo\n", false, 3);   //Prints in screen named "LCD" the list or Ores contained in vlCargo, starts with "Cargo" in a single line and devides the list in 3 items per line
            cargoClass[] cargoPack = { vlCargo, vlTool };
            printCargoPercentage("LCD", cargoPack, "PC\n", true, 1);

            //drain all Items of X type from the selected group of cargo to whatever ship you are connected to. Empty type drains all types
            drainAllOfType(cargoPack, "Ores");  //Note that if you add reactors and O2 generators, they will be emptied as well.
            //Fill current ship with the 
        }
    }
}