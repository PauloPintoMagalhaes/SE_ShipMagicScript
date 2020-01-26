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
        //determines SE code type from a list of more reader friendly codes
        public string cToSE_Type(string vfUser_Type)
        {
            string vlSE_Type;
            if (vfUser_Type == "Ores") { vlSE_Type = "MyObjectBuilder_Ore"; }
            else if (vfUser_Type == "Ingots") { vlSE_Type = "MyObjectBuilder_Ingot"; }
            else if (vfUser_Type == "Components") { vlSE_Type = "MyObjectBuilder_Component"; }
            else if (vfUser_Type == "Tools") { vlSE_Type = "MyObjectBuilder_PhysicalGunObject"; }
            else if (vfUser_Type == "Ammo") { vlSE_Type = "MyObjectBuilder_AmmoMagazine"; }
            else if (vfUser_Type == "O2Bottles") { vlSE_Type = "MyObjectBuilder_OxygenContainerObject"; }
            else if (vfUser_Type == "H2Bottles") { vlSE_Type = "MyObjectBuilder_GasContainerObject"; }
            else if (vfUser_Type == "Consumables") { vlSE_Type = "MyObjectBuilder_ConsumableItem"; }
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
                if (SE_SubType == "Ice" || SE_SubType == "Stone" || SE_SubType == "Gravel" || SE_SubType == "Organic")
                {
                    nonSE_Name = SE_SubType;
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
                else if (SE_SubType == "OxygenContainerObject") { nonSE_Name = "O2Bottle"; }
                else if (SE_SubType == "GasContainerObject") { nonSE_Name = "H2Bottle"; }
                else if (SE_SubType == "Missile200mm") { nonSE_Name = "Missile200mm"; }
                else if (SE_SubType == "NATO_5p56x45mm") { nonSE_Name = "Ammo045mm"; }
                else if (SE_SubType == "NATO_25x184mm") { nonSE_Name = "Ammo184mm"; }
                else if (SE_SubType == "Canvas") { nonSE_Name = "Canvas"; }
                else if (SE_SubType == "ZoneChip") { nonSE_Name = "ZoneChip"; }
                else if (SE_SubType == "Package") { nonSE_Name = "Package"; }
                else if (SE_SubType == "Datapad") { nonSE_Name = "Datapad"; }
                else if (SE_SubType == "Medkit") { nonSE_Name = "Medkit"; }
                else if (SE_SubType == "CosmicCoffee") { nonSE_Name = "Coffee"; }
                else if (SE_SubType == "ClangCola") { nonSE_Name = "ClangCola"; }
                else if (SE_SubType == "Powerkit") { nonSE_Name = "Powerkit"; }
                else { nonSE_Name = SE_SubType; }
            }
            return nonSE_Name;
        }

        //verifies item volume
        public float cVolume(string SE_Type, string SE_SubType)
        {
            float vlVolume = 0;
            if (SE_Type == "MyObjectBuilder_Ore")
            {
                vlVolume = (SE_SubType == "Scrap" || SE_SubType ==  "OldScrap") ? 0.254f : 0.37f;
                //Ores (including organic) all have the same volume except for scrap
            }
            else if (SE_Type == "MyObjectBuilder_Ingot")
            {
                if (SE_SubType == "Gold") { vlVolume = 0.052f; }
                else if (SE_SubType == "Silver") { vlVolume = 0.10f; }
                else if (SE_SubType == "Nickel" || SE_SubType == "Cobalt" || SE_SubType == "Uranium") { vlVolume = 0.11f; }
                else if (SE_SubType == "Iron") { vlVolume = 0.13f; }
                else if (SE_SubType == "Silicon") { vlVolume = 0.43f; }
                else if (SE_SubType == "Platinum") { vlVolume = 0.5f; }
                else if (SE_SubType == "Magnesium") { vlVolume = 0.58f; }
                else if (SE_SubType == "Scrap") { vlVolume = 0.254f; }
            }
            else if (SE_Type == "MyObjectBuilder_Component")
            {
                if (SE_SubType == "Computer") { vlVolume = 1; }
                else if (SE_SubType == "Explosives" || SE_SubType == "Construction" || SE_SubType == "Girder" || SE_SubType == "SmallTube") { vlVolume = 2; }
                else if (SE_SubType == "SteelPlate") { vlVolume = 3; }
                else if (SE_SubType == "InteriorPlate") { vlVolume = 5; }
                else if (SE_SubType == "Display" || SE_SubType == "Detector") { vlVolume = 6; }
                else if (SE_SubType == "Motor" || SE_SubType == "BulletproofGlass" || SE_SubType == "Superconductor" || SE_SubType == "Reactor") { vlVolume = 8; }
                else if (SE_SubType == "Thrust") { vlVolume = 10; }
                else if (SE_SubType == "MetalGrid") { vlVolume = 15; }
                else if (SE_SubType == "SolarCell") { vlVolume = 20; }
                else if (SE_SubType == "LargeTube") { vlVolume = 38; }
                else if (SE_SubType == "PowerCell") { vlVolume = 45; }
                else if (SE_SubType == "RadioCommunication") { vlVolume = 140; }
                else if (SE_SubType == "Medical") { vlVolume = 160; }
                else if (SE_SubType == "GravityGenerator") { vlVolume = 200; }
                else if (SE_SubType == "Canvas") { vlVolume = 8; }
                else if (SE_SubType == "ZoneChip") { vlVolume = 0.2f; }
            }
            else if (SE_Type == "MyObjectBuilder_PhysicalGunObject")
            {
                if (SE_SubType == "HandDrillItem" || SE_SubType == "HandDrill2Item" || SE_SubType == "HandDrill3Item" || SE_SubType == "HandDrill4Item") { vlVolume = 120; }
                else if (SE_SubType == "WelderItem" || SE_SubType == "Welder2Item" || SE_SubType == "Welder3Item" || SE_SubType == "Welder4Item") { vlVolume = 8; }
                else if (SE_SubType == "AngleGrinderItem" || SE_SubType == "AngleGrinder2Item" || SE_SubType == "AngleGrinder3Item" || SE_SubType == "AngleGrinder4Item") { vlVolume = 20; }
            }
            else if (SE_Type == "MyObjectBuilder_AmmoMagazine")
            {
                if (SE_SubType == "Missile") { vlVolume = 60; }
                else if (SE_SubType == "Ammo045mm") { vlVolume = 0.2f; }
                else if (SE_SubType == "Ammo184mm") { vlVolume = 16; }
            }
            else if (SE_Type == "MyObjectBuilder_OxygenContainerObject" || SE_Type == "MyObjectBuilder_GasContainerObject") { vlVolume = 120; }
            else if (SE_Type == "MyObjectBuilder_ConsumableItem") 
            {
                if (SE_SubType == "Medkit") { vlVolume = 12; }
                else if (SE_SubType == "Powerkit") { vlVolume = 9; }
                else if (SE_SubType == "CosmicCoffee") { vlVolume = 1; }
                else if (SE_SubType == "ClangCola") { vlVolume = 1; }
            }
            else if (SE_Type == "MyObjectBuilder_Package") { vlVolume = 125; }
            else if (SE_Type == "MyObjectBuilder_Datapad") { vlVolume = 0.4f; }

            return vlVolume;
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

        private void printCustomMsg(string vfLCD_Name, List<string> vfTest)
        {
            IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName(vfLCD_Name) as IMyTextPanel;
            if (vlLCD != null)
            {
                string vlMSG = "";
                //build custom message here
                foreach(string line in vfTest)
                {
                    vlMSG += line + "\n";
                }


                //print message
                vlLCD.WriteText(vlMSG);
            }
        }



        //transfering functions/methods . 
        private List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> trimByType (List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> vfOld, string vfType)
        {
            List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> vlNewList = new List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>>();
            //id and freespace do not concern us here. we only check if type exists and if so, create list with it
            vlNewList = vfOld.FindAll(a => a.Item3 == vfType);
            return vlNewList;
        }

        //Filtered so it only tries to do anything if it's connected somewhere else and that else is something that can receive items
        private void drainAllOfType(cargoClass[] data, string vfType, bool vfOrganize = true)
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
                //WIP need to force the facility management of the externalcargo to check for raw materials to pull to production queue
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

        private List<MyTuple<string, string, MyFixedPoint, int>> initialSearchCheck( List<MyTuple<string, string, MyFixedPoint, int>> vfOrigList, List<MyTuple<string, string, float>> vfUseGlobalList)
        {
            List<MyTuple<string, string, MyFixedPoint, int>> vlFound = new List<MyTuple<string, string, MyFixedPoint, int>>(); 
            //checks if the cargo being searched has any of the intended items
            if (vfUseGlobalList == null && vfOrigList != null) 
            {
                //If there is no specific search and origin has items to pick, then its a go for transfer
                vlFound = vfOrigList;
            }
            else if(vfUseGlobalList != null && vfOrigList != null)
            {
                //If there is a specific search and origin has items to pick, then lets search
                foreach (var wantedLine in vfUseGlobalList)
                {
                    //if the item in the list, find everyone and passes the item location to 
                    var vlLine = vfOrigList.FindAll(a=> a.Item1 == wantedLine.Item1 && a.Item2 == wantedLine.Item2);
                    if (vlLine != null || vlLine.Count > 0) { vlFound.AddRange(vlLine); }
                }
            }
            return vlFound; //everything that gets here is to be transfered (checks on freespace and quantity limit come later)
        }

        //I'd really like to have this function on the transferClass, but I can't seem to make GridTerminalSystem functions work outside here =(
        private void searchThroughLists(List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> vfOrigList, List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> vfDestinList, bool vfOrganize = true, List<MyTuple<string, string, float>> vfUseGlobalList = null)
        {
            //We're going to have to change these lists, so we parse them here. We don't need to change anything in the class itself because the next cycle of the programming block will update the info anyway
            var destinationLstLst = vfDestinList;   //originLstLst because its a list within a list, wheel within wheels, plans withing plans, schemes within schemes
            var vlShopList = vfUseGlobalList;
            MyFixedPoint vlMinFreeSpace = 200;
            float vlNewValue;
            bool vlPutAnywhere = false;
            var vlDestLst = new List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>>();
            int vlShopIndex = 0;

            //Honestly, this is the part that I dislike the most. Hopefully, with the changes I made recently, this will become simpler
            foreach (var originLst in vfOrigList)
            {
                vlShopIndex = vlShopList.FindIndex(a => a.Item1 == originLst.Item3 && a.Item2 == originLst.Item4);
                //checks if this item is marked for transfer
                if (vlShopList.Count > 0 || vlShopIndex >= 0)
                {
                    //Found the offending item and so, we must exile it beyond our fair ship nation!
                    if(vfOrganize){
                        //verify if there is a place with the same item subtype and freepace to receive it
                        vlDestLst = destinationLstLst.FindAll(a => a.Item3 == originLst.Item3 && a.Item4 == originLst.Item4 && a.Item2 > vlMinFreeSpace);
                        if (vlDestLst.Count >= 0)
                        {
                            vlNewValue = orderTransferCycle(originLst, vlDestLst, vlShopList[vlShopIndex].Item3);
                            if (vlNewValue == 0)
                            {   //transfered all there was
                                vlShopList.RemoveAt(vlShopIndex);
                                continue;
                            }
                            else
                            {
                                //updates relevant values and tries in the next search type
                                vlShopList[vlShopIndex] = MyTuple.Create(vlShopList[vlShopIndex].Item1, vlShopList[vlShopIndex].Item2, vlNewValue);
                            }
                        }
                        vlDestLst = destinationLstLst.FindAll(a => a.Item3 == originLst.Item3 && a.Item2 > vlMinFreeSpace);
                        if (vlDestLst.Count >= 0)
                        {
                            vlNewValue = orderTransferCycle(originLst, vlDestLst, vlShopList[vlShopIndex].Item3);
                            if (vlNewValue == 0)
                            {   //transfered all there was
                                vlShopList.RemoveAt(vlShopIndex);
                                continue;
                            }
                            else
                            {
                                //updates relevant values and tries in the next search type
                                vlShopList[vlShopIndex] = MyTuple.Create(vlShopList[vlShopIndex].Item1, vlShopList[vlShopIndex].Item2, vlNewValue);
                            }
                        }
                        //if we did not find a space to put it in a controled fashion, we place it anywhere it can fit
                        vlPutAnywhere = true;
                    }
                    if(!vfOrganize || vlPutAnywhere)
                    {
                        if (vlPutAnywhere) { vlPutAnywhere = false; }
                        vlDestLst = destinationLstLst.FindAll(a => a.Item2 > vlMinFreeSpace);
                        if (vlDestLst.Count >= 0)
                        {
                            //No organization required. Find an inventory with freespace and shove it there
                            vlNewValue = orderTransferCycle(originLst, vlDestLst, vlShopList[vlShopIndex].Item3);
                            if (vlNewValue == 0)
                            {   //transfered all there was
                                vlShopList.RemoveAt(vlShopIndex);
                                continue;
                            }
                            else
                            {
                                //No point in updating anymore because there's no more space anywhere, exit cycle
                                break;
                            }
                        }
                        else
                        {
                            //if there is no free space anywhere, break out of the search cycle because there's no point in searching
                            break;
                        }
                    }
                }
            }
        }

        //Cycles through an available lists until the order is complete or fails
        private float orderTransferCycle(MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int> vfOrigList, List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> vfDestinList, float vfQuant)
        {
            float vlNewValue=0;
            float vlTMP=0;
            foreach (var destLine in vfDestinList)
            {
                //attempts to transfer and returns the ammount transfered.
                vlTMP = vfQuant - orderItemTransfer(vfOrigList.Item1, vfOrigList.Item6, destLine.Item1, vfQuant, destLine.Item2, false);
                if(vlTMP <= 0)  { vlNewValue = 0; break;  }
                else { vlNewValue = vlTMP; }
            }
            return vlNewValue;
        }

        //attempts to transfer vfQuantity of an item from vfOrigin in position vfPos and send it to vfDestination
        private float orderItemTransfer(long vfOriginID, int vfOriginPos, long vfDestinID,  float vfQuant, MyFixedPoint vfFreeSpace, bool vfAglutinate = false)
        {
            //This sucks because, since I can't calculate with MyFixedPoint, I have to convert it for calculation, then reconvert it back as the new value 
            //the list should have after the transfer. I need to translate it into a float because the base unit is in the thousands, which is fine for roleplay, but a bit silly programwise
            float vlTMP = 0;
            //Set inventory where the items originate from
            IMyTerminalBlock vlBlock = GridTerminalSystem.GetBlockWithId(vfOriginID);
            IMyInventory vlOrigin;
            vlOrigin = (vlBlock is IMyRefinery || vlBlock is IMyRefinery) ? vlBlock.GetInventory(1) : vlBlock.GetInventory(0);
            //Set inventory where the items will be going to from
            vlBlock = GridTerminalSystem.GetBlockWithId(vfDestinID);
            IMyInventory vlDestin;
            vlDestin = (vlBlock is IMyRefinery || vlBlock is IMyRefinery) ? vlBlock.GetInventory(1) : vlBlock.GetInventory(0);
            //Inventories are established. Now calculate how much of the item we can transfer. 
            /*PM: Keen software fails again by not having an indivivual volume property. Will have to calculate it by hand, so now I'me facing another
            hard decision; do I open another itemlist just to search for the item in question to verify the volume and quantity, or do I add a column in
            cargoClass that includes item volume, total stack volume, or both? 
            Decision: I'm goint to assume its best to convert when transfer is needed than to convert everysingle item of every single inventory*/
            var vlItem = vlOrigin.GetItemAt(vfOriginPos);
            var vlStackVolume = (float)vlItem.Value.Amount * cVolume(vlItem.Value.Type.SubtypeId.ToString(), vlItem.Value.Type.TypeId.ToString());

            //destInv.TransferItemFrom(sourceInv, 0, stackIfPossible: true, amount: transferAmount)
            //???? WIP

            return vlTMP;
        }

        private string filterType(string[] vfMainArg, string vfTypeSearch)
        {
            var search = "";
            if (vfTypeSearch != "Drain_Type" && vfTypeSearch != "Fill_Type") { search = null; } //For safety, if no correct command is found, do nothing
            else
            {
                //Splits the command to discover what type of drain/fill we're using
                string[] vlSplit = vfMainArg[Array.IndexOf(vfMainArg, vfTypeSearch)].Split(':');
                if (vlSplit.Length <= 1) { search = "All"; }
                else 
                {
                    if (vlSplit[1] != "Ore" && vlSplit[1] != "Component" && vlSplit[1] != "Tool" && vlSplit[1] != "Ingot" && vlSplit[1] != "Consumable")
                    {
                        search = null;
                    }
                    else { search = vlSplit[1]; }
                }
            } 
            return search;
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

            printCustomMsg("LCD", vlCargo.TEST);


            //argument controls what the script is supposed to do upon that particular control.
            string[] vlOrders = argument.Split(' '); //seperates the argument into several commands divided by a space
            if (vlOrders.Length > 0) {
                if (vlOrders.Contains("Drain_Type"))
                {
                    cargoClass[] cargoPack = { vlCargo, vlTool };
                    //drain all Items of X type from the selected group of cargo to whatever ship you are connected to. Empty type drains all types
                    drainAllOfType(cargoPack, "Ores");  //Note that if you add reactors and O2 generators, they will be emptied as well.
                }
                if (vlOrders.Contains("Fill_Type"))
                {
                    cargoClass[] cargoPack = { vlCargo, vlTool };
                    //verifies what type of item we're filling with
                    var vlType = filterType(vlOrders, "Type_Fill");
                    if (vlType != null) { fillWithAllOfType(cargoPack, vlType); }
                }
                if (vlOrders.Contains("Fill_List"))
                {
                    cargoClass[] cargoPack = { vlCargo, vlTool };
                    //fills ship with materials until it fulfils. at least, the list in transferClass
                    fillWithList(cargoPack);
                }
                if (vlOrders.Contains("Solar_Synch"))
                {
                    //begins the synchronization of the solar panel arrays (if they exist) 
                    //PM: Have to find a way of seperating things into groups if I have more than one movable solar array

                }
            }
            //Print remaining Info
            //printItemType("Name of LCD", Cargo Variable, Type (see list below), text begins with (use if you want to mark the following print with something), keep old text, Items per line);
            //Ores, Ingots, Components, Tools, Ammo, O2Bottles, H2Bottles, Consumablers 
            //printItemType("LCD", vlCargo, "Ores", "Cargo\n", false, 3);   //Prints in screen named "LCD" the list or Ores contained in vlCargo, starts with "Cargo" in a single line and devides the list in 3 items per line
            //printCargoPercentage("LCD", cargoPack, "PC\n", true, 1);

            //Fill current ship with the 
        }
    }
}