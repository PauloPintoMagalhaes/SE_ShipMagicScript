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
        /************************************************************************************************************
        *                                   Space Engineers Ship Magic Script                                       *
        *                   Made by Paulo Magalhães aka Skaer aka MangaYuurei aka Ghost of Magellan                 *
        *************************************************************************************************************                   
        *                                                   Purpose                                                 *
        *              Create assistance scripts for Space Engineers ships and learn C# in the process              *   
        *   One script runs in every ship. User only requires to configure the "Main" according to the need even    *
        *   without programming knowledge. Just follow the instructions and keep the "Vital" section of the main    *
        *   undisturbed and you'll be just fine. If you're a programmer, this might be interesting for you:         *
        *   https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts         *
        *                                                                                                           *
        ************************************************************************************************************/

        public Ship_Main()
        {
            //auto runs the script without the need for a timer block
            Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update100;
            //still testing if 100 ticks are the correct timing
            //Note: this might not be appropriate for the refinery/assembly management program since we're going 
            //to wait a lot more than 100 ticks to update them (15 minutes at the least), but should be appropriate
            //a ship cargo update script
        }

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


        private void printType(string LCD_Name, string printType)
        {
            // to my ABSOLUTE dismay, I can't use local functions in SE due to it not accepting C# 7.0
        }
        private void printCargo(string LCD_Name)
        {
            //Did I meantion how dismayed I am for not being able to tuck this away as a local function? Well, I am...
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //Vital code. DO NOT TOUCH!
            cargoClass vlCargo = new cargoClass();  //Technically, this could have been a static, but I may want to group different cargos separately


            //**************************************
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
            //**************************************
            vlCargo.getCargoCount(cargoType("Container"));  //Already declared above
            vlCargo.getCargoCount(cargoType("Connector"));  //Note that Ejectors count as connectors too
            cargoClass vlReactor = new cargoClass();
            vlReactor.getCargoCount(cargoType("Reactor"));
            cargoClass vlTool = new cargoClass();
            vlTool.getCargoCount(cargoType("Drill"));
        }
    }
}
