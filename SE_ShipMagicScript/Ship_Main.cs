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
        //Determines whether a block belongs to the same grid as the programming block calling it or not
        private bool isLocalGrid(IMyTerminalBlock block)
        {
            return block.CubeGrid == Me.CubeGrid;
        }

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
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(vlListTMP, isLocalGrid);
                    if (vlListTMP != null || vlListTMP.Count > 0) { vlCargoList.AddRange(vlListTMP); }
                    break;
            }
            return vlCargoList;
        }

        private void printGroupInLCD(string vfLCD_Name, cargoClass data, string vfType = "", int itemsPerLine = 1, bool vfKeepCurrentMsg = false)
        {
            // to my ABSOLUTE dismay, I can't use local functions in SE due to it not accepting C# 7.0
            IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName(vfLCD_Name) as IMyTextPanel;
            if (vlLCD != null)
            {
                string vlMSG = "";
                int vlI = 0;
                int vlIndex = data.cargoLst.MaterialQuantity.FindIndex(a => a.Item1 == vfType); // Checks if it does have that type, otherwise no point in continuing
                if (vlIndex >=0 || vfType == "") // Checks if it does have that type, otherwise no point in continuing
                {
                    Echo("found general type");
                    foreach (MyTuple<string, string, float> line in data.cargoLst.MaterialQuantity)
                    {
                        //Cycles through the list and builds up a message with only the permitted items and in the specified conditions
                        if (vfType == line.Item1 || vfType == "")
                        {
                            vlMSG = vlMSG + string.Format(" {0}: #{1}", line.Item2, line.Item3);
                            vlMSG = (vlI+1 % itemsPerLine == 0) ? vlMSG + "\n" : vlMSG + " | ";
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
        private void printItemType(string vfLCD_Name, cargoClass data, string vfType, bool vfKeepCurrentMsg = false, int itemsPerLine = 1)
        {
            printGroupInLCD(vfLCD_Name, data, vfType, itemsPerLine, vfKeepCurrentMsg);
        }

        private void printItemAll(string vfLCD_Name, cargoClass data, bool vfKeepCurrentMsg = false, int itemsPerLine = 1)
        {
            printGroupInLCD(vfLCD_Name, data, "", itemsPerLine, vfKeepCurrentMsg);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //Vital code. DO NOT TOUCH!
            cargoClass vlCargo = new cargoClass();  //Technically, this could have been a static, but I may want to group different cargos separately

            //Non Vital code. Change as needed.
            //Options are: "Container", "Connector", "Drill", "Welder", "Grinder", "Reactor" and "O2Generator"
            //Incorrect naming or empty parameter groups all cargo together
            //vlCargo.getCargoCount(cargoType("Container"));
            //if you want to separate groups of cargo then you must either create new variables or print and clear each seperately
            //vlCargo.Clear();
            //vlCargo2.getCargoCount(cargoType("Reactor"));
            //you can also call a different cargo type into the same group and it will sum the mats into the same variable
            //vlCargo.getCargoCount(cargoType("Container"));
            //vlCargo.getCargoCount(cargoType("Connector"));
            //
            vlCargo.getCargoCount(cargoType("Container"));  //Already declared above
            vlCargo.getCargoCount(cargoType("Connector"));  //Note that Ejectors count as connectors too
            cargoClass vlReactor = new cargoClass();
            vlReactor.getCargoCount(cargoType("Reactor"));
            cargoClass vlTool = new cargoClass();
            vlTool.getCargoCount(cargoType("Drill"));


            //testing stuff
            printItemAll("LCD", vlCargo);
            //IMyTextPanel vlLCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
            //foreach (MyTuple<string, string, float> item in vlCargo.cargoLst.MaterialQuantity)
            //{
            //    vlLCD.WriteText(string.Format("{0} {1} {2}", item.Item1, item.Item2, item.Item3), true);
            //}
            //foreach (string item in vlCargo.TEST1)
            //{
            //    vlLCD.WriteText(item.ToString() + "\n", true);
            //}

        }
    }
}
