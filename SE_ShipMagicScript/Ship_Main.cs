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
        //This list decided which items you want to automatically pull to your ship IF, there is space or connection available.
        Dictionary<string, float> desiredMaterials = new Dictionary<string, float> { 
            //Components
			{"Construction", 10000}, {"MetalGrid", 2000}, {"InteriorPlate", 10000}, {"SteelPlate", 10000},
            {"Girder", 2000}, {"SmallTube", 10000}, {"LargeTube", 2000}, {"Motor", 3000}, {"Display", 500}, {"Glass", 5000},
            {"Superconductor", 500}, {"Computer", 3000}, {"Reactor", 2000}, {"Thrust", 2000}, {"GravityGen", 1000}, {"Medical", 500},
            {"RadioComm", 300}, {"Detector", 300}, {"Explosives", 300}, {"SolarCell", 3000}, {"PowerCell", 3000}, 
			//Ores
			{"Ice", 0}, {"Stone", 0}, {"Gold Ore", 0}, {"Iron Ore", 0}, {"Silver Ore", 0}, {"Cobalt Ore", 0}, {"Nickel Ore", 0}, {"Uranium Ore", 0},
            {"Silicon Ore", 0}, {"Platinum Ore", 0}, {"Magnesium Ore", 0}, {"Gravel", 0}, {"Gold Ingot", 0}, {"Silver Ingot", 0}, {"Nickel Ingot", 0},
            {"Iron Ingot", 0}, {"Silicon Ingot", 0}, {"Platinum Ingot", 0}, {"Magnesium Ingot", 0}, 
			//Tools and bottles
			{"Drill",  0}, {"Drill2", 0}, {"Drill3", 0}, {"Drill4", 0},
            {"Welder", 0}, {"Welder2", 0}, {"Welder3", 0}, {"Welder4", 0},
            {"Grinder", 0}, {"Grinder2", 0}, {"Grinder3", 0}, {"Grinder4", 0},
            {"OxygenBottle", 0}, {"HydrogenBottle", 0}, 
			//Ammo
			{"Missile", 0}, {"Ammo045mm", 0}, {"Ammo184mm", 0},
            //Consumables
            {"ClangCola", 0}
            //List is missing Uranium Ingot because it would have been sucked up by a random reactor, anyway.
        };


        //string conversions From and To SE Type and SubType codes to userfriendly terms
        //This function is not very well made, but it worked on a previous version of the code and I'm too tired to
        //"pimp" it up now. 
        private string[] cToSE_Key(string thisItem)
        {
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

            return new string[] { SE_SubType, SE_Type };
        }

        //determines SE code type from a list of more reader friendly codes
        private string cToSE_Type(string vfNormal_Type)
        {
            string vlSE_Type = "";
            
            if (vfNormal_Type == "Ores") { vlSE_Type = "MyObjectBuilder_Ore"; }
            else if (vfNormal_Type == "Ingots") { vlSE_Type = "MyObjectBuilder_Ingot"; }
            else if (vfNormal_Type == "Components") { vlSE_Type = "MyObjectBuilder_Component"; }
            else if (vfNormal_Type == "Tools") { vlSE_Type = "MyObjectBuilder_PhysicalGunObject"; }
            else if (vfNormal_Type == "Ammo") { vlSE_Type = "MyObjectBuilder_AmmoMagazine"; }
            else if (vfNormal_Type == "O2Bottles") { vlSE_Type = "MyObjectBuilder_OxygenContainerObject"; }
            else if (vfNormal_Type == "H2Bottles") { vlSE_Type = "MyObjectBuilder_GasContainerObject"; }
            else if (vfNormal_Type == "Consumables") { vlSE_Type = "MyObjectBuilder_ConsumableItem"; }
            else { vfNormal_Type = ""; }

            return vlSE_Type;
        }

        //converts a game subtype to a more understandable, printable name
        private string cFromSE_Key(string SE_SubType, string Type)
        {
            string nonSE_Name = "";
            //Don't know who had the bright idea to name different items with the same subtype. Makes things unnecessarily hard
            if (Type == "MyObjectBuilder_Ore" || Type == "MyObjectBuilder_Ingot")
            {
                if (SE_SubType == "Ice" || SE_SubType == "Stone" || SE_SubType == "Gravel")
                {
                    nonSE_Name = "MyObjectBuilder_Ore";
                }
                else
                {
                    string vlNewType = (Type == "MyObjectBuilder_Ore") ? "Ore" : "Ing";
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
            if(vlConLst != null && vlConLst.Count > 0)
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
                    vlI ++;
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

        private List<IMyTerminalBlock> externalCargo()
        {
            //This exists only to give the main ship something to throw its items at. 
            var vlCargoList = new List<IMyTerminalBlock>();
            var vlListTMP = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(vlCargoList, isExternalGrid);
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlListTMP, isExternalGrid);
            if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }

            return vlCargoList;
        }

        private List<IMyTerminalBlock> connectedCargo(IMyTerminalBlock vfConnetor, bool includeReac_H2O2 = false, bool includeProduction = false)
        {
            //explanations on cargoType()
            var vlCargoList = new List<IMyTerminalBlock>();
            //this includes the "All" scenario. Joins up all the inventory types in one single list
            var vlListTMP = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(vlCargoList, x=> x.CubeGrid == vfConnetor.CubeGrid);
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
            if (includeProduction)
            {
                GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
                if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlListTMP, x => x.CubeGrid == vfConnetor.CubeGrid);
                if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
            }
            return vlCargoList;
        }

        private List<IMyTerminalBlock> facilityCargo(string blockType = "All", bool isLocal = false)
        {
            //This exists only to give the main ship something to throw its items at. 
            var vlCargoList = new List<IMyTerminalBlock>();
            var vlListTMP = new List<IMyTerminalBlock>();
            //since I can't put the result of isLocalGrid and isExternalGrid into a variable, I'm forced to double the ammount of code
            if (isLocal)
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
                if (blockType == "Refinery") { GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, isExternalGrid); }
                else if (blockType == "Assembly") { GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlCargoList, isExternalGrid); }
                else
                {
                    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(vlCargoList, isExternalGrid);
                    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(vlListTMP, isExternalGrid);
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
            printGroupInLCD(vfLCD_Name, data, "", vfStartNote, itemsPerLine,  vfKeepCurrentMsg);
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


        //transfering functionsmethods . 
        //Filtered so it only tries to do anything if it's connected somewhere else and that else is something that can receive items
        private void drainAllOfType(cargoClass[] data, string vfType)
        {
            if (data != null && isConnected())
            {
                List<IMyTerminalBlock> vlExternal = connectedCargo(getExernalConnector());
                foreach (cargoClass item in data)
                {
                    //Drains this type of item to indicated cargo
                    item.drainAllOfType(cToSE_Type(vfType), vlExternal);
                }
            }
            
        }

        private void fillWithAllOfType(cargoClass[] data, string vfType)
        {
            if (data != null && isConnected())
            {
                List<IMyTerminalBlock> vlExternal = connectedCargo(getExernalConnector());
                foreach (cargoClass item in data)
                {
                    item.fillWithAllOfType(cToSE_Type(vfType), vlExternal);
                }
            }
        }

        private void fillWithList(cargoClass[] data, Dictionary<string, float> vfList)
        {
            if (data != null && isConnected())
            {
                List<IMyTerminalBlock> vlExternal = connectedCargo(getExernalConnector());
                foreach (cargoClass item in data)
                {
                    item.fillWithList(vfList, vlExternal);
                }
            }
        }


        //Main function... where the party starts and ends
        public void Main(string argument, UpdateType updateSource)
        {
            //Note that all these classes only take data from the ship where the propgramming block is being run
            cargoClass vlCargo = new cargoClass();
            //Fills up variabbles with different type of content. If empty, it will search All types of cargo containers at once
            //Options are: "Container", "Connector", "Drill", "Welder", "Grinder" and "Reactor" and "O2Generator"
            //You can wipe the variable clean with vlCargo.Clear();
            vlCargo.getCargoCount(cargoType("Container"));  //Already declared above
            vlCargo.getCargoCount(cargoType("Connector"));  //Note that Ejectors count as connectors too
            cargoClass vlReactor = new cargoClass();
            vlReactor.getCargoCount(cargoType("Reactor"));
            cargoClass vlTool = new cargoClass();
            vlTool.getCargoCount(cargoType("Drill"));

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
