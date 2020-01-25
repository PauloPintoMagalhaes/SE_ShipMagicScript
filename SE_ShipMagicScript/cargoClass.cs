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
    partial class Ship_Main
    {
        public class cargoClass : MyGridProgram
        {
            public class individualCargo
            {
                //Declare initial variables. Lists are proving to be more versatile than common arrays. Note: this is the reasonable way out. See edit below
                public List<MyTuple<string, string, float>> MaterialQuantity = new List<MyTuple<string, string, float>>();
                //After this misbegotten list, there are only two possible conclusions: Either I'm a complete moron, or I'm a genius. Edit: given on how this didn't explode in my face, I'll go with genius, for now
                public List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> MaterialList = new List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>>();
                //This list the inventory ID's where all the items of a certain type are stored. Made this to accelerate the search speed and code simplicity of the transfer functions
                public List<MyTuple<string, string, List<long>>> MaterialCheck = new List<MyTuple<string, string, List<long>>>();
                public individualCargo()
                {
                    MaterialList.Clear();
                    MaterialQuantity.Clear();
                }

                //adds items to the cargo list in a controlled fashion. Serves for transfering
                public void addItem(long vfID, MyFixedPoint vfFreespace, string vfType, string vfSubType, MyFixedPoint vfQuant, int vfPos)
                {
                    /*I have tried to make a tuple list within a tuple list to save memory and spare repeated IDs and Freespaces, but the search for items became
                    too complex and cumbersome. I will get the wasted memory back in CPU power and ease of programming and usage, so it's fine, I suppose*/ 
                    //check if id in question exists.
                    MaterialList.Add(MyTuple.Create(vfID, vfFreespace, vfType, vfSubType, vfQuant, vfPos));

                    //if type is empty it means we're just marking the id and free space in the list. No need to update the quantity of an item that does not exist
                    if (vfType != "") { 
                        addQuantities(vfSubType, vfType, (float)vfQuant);
                        addCheck(vfSubType, vfType, vfID);
                    }

                }
                //adds items to the quantity list in a controlled fashion. Serves for printing info
                public void addQuantities(string vfSubType, string vfType, float vfQuantity)
                {
                    int vlIndex = MaterialQuantity.FindIndex(a => a.Item1 == vfType && a.Item2 == vfSubType); 
                    // Checks if it does have that type, otherwise no point in continuing
                    //Also, we must ensure there is only one single combination of vfType and vfSubtype
                    if (vlIndex >= 0)
                    {
                        //Found the item in question. increment
                        MyTuple<string, string, float> vlTMP = MaterialQuantity[vlIndex];
                        MaterialQuantity[vlIndex] = MyTuple.Create(vlTMP.Item1, vlTMP.Item2, vlTMP.Item3 + vfQuantity);
                    }
                    else //returns -1 if not found
                    {
                        //Didn't find the item in question. adds
                        MaterialQuantity.Add(MyTuple.Create(vfType, vfSubType, vfQuantity));
                    }
                }
                public void addCheck(string  vfSubType, string vfType, long vfID)
                {
                    List<long> vlID_List = new List<long> ();
                    int vlIndex = MaterialCheck.FindIndex(a => a.Item1 == vfType && a.Item2 == vfSubType);
                    // Checks if it does have that type, otherwise no point in continuing
                    //Also, we must ensure there is only one single combination of vfType and vfSubtype
                    if (vlIndex >= 0)
                    {
                        //Found the item in question.
                        vlID_List = MaterialCheck[vlIndex].Item3;
                        vlID_List.Add(vfID);    //adds the new ID
                        MaterialCheck[vlIndex] = MyTuple.Create(MaterialCheck[vlIndex].Item1, MaterialCheck[vlIndex].Item2, vlID_List); //Replaces it on the list

                    }
                    else //returns -1 if not found
                    {
                        //Didn't find the item in question. adds
                        vlID_List.Add(vfID);    //adds the new ID
                        MaterialCheck.Add(MyTuple.Create(vfType, vfSubType, vlID_List));
                    }
                }
            }

            //Declare initial variables. Note that refineries and assemblies don't need this flag because they'll be managed in another way
            private bool impCargo, impReactor, impGenerator, impDrill, impWelder, impGrinder, impConnector, impRefinery, impAssembly = false;
            private MyFixedPoint __currentVolume, __totalVolume = 0;
            private float __CargoRacio = 0;
            private int __count = 0;
            private individualCargo cargoLst = new individualCargo();

            //Define encapsulation variables to prevent users from messing with the information
            //(A bit trivial for this script, since the info is rewritten before executing anything significant, but it's good practice)
            public MyFixedPoint CurrentVolume { get { return __currentVolume; } }
            public MyFixedPoint TotalVolume { get { return __totalVolume; } }
            public float Racio { get { return __CargoRacio; } }
            public int Count { get { return __count; } }
            public List<MyTuple<string, string, float>> MaterialQuantity { get { return cargoLst.MaterialQuantity; } }
            public List<MyTuple<long, MyFixedPoint, string, string, MyFixedPoint, int>> MaterialList { get { return cargoLst.MaterialList; } }

            public List<string> TEST = new List<string>();

            //Don't actually need a constructor in this case, but use it to guarantee the values are reinitialized to avoid data contamination
            public cargoClass()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false; impRefinery = false; impAssembly = false;
                __currentVolume = 0; __totalVolume = 0; __CargoRacio = 0; __count = 0;
            }

            public void Clear()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false;
                __currentVolume = 0; __totalVolume = 0; __CargoRacio = 0; __count = 0;
            }

            private float calcPercentage(float value, float total)
            {
                return (total == 0) ? 0 : value / total * 100;
            }

            private bool foundItemInInventory(MyInventoryItem vfInventory, List<string> vfType, string vfSubType = "All")
            {
                bool vlTMP = false;
                //I could do this with less conditions but this way it's easier to understand
                if ((vfSubType != "All" && vfInventory.Type.SubtypeId.ToString() == vfSubType)
                    || (vfType[0] != "All" && vfSubType == "All" && vfType.Contains(vfInventory.Type.TypeId.ToString()))
                    || (vfType[0] == "All" && vfSubType == "All"))
                {
                    //if the item in this slot corresponds to a specific subtype, you can pass it
                    //if the item in this slot corresponds to a specific type, you can pass it regardless of subtype
                    //all items found can be passed
                    vlTMP = true;
                }
                return vlTMP;
            }

            //checks if this cargo type has already been searched through
            private bool isRepeatedSearch(Type vfBlock)
            {
                bool vlTMP = false;
                if (vfBlock == typeof(IMyCargoContainer) && impCargo) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipConnector) && impConnector) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipDrill) && impDrill) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipWelder) && impWelder) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipGrinder) && impGrinder) { vlTMP = true; }
                else if (vfBlock == typeof(IMyReactor) && impReactor) { vlTMP = true; }
                else if (vfBlock == typeof(IMyGasGenerator) && impGenerator) { vlTMP = true; }
                else if (vfBlock == typeof(IMyRefinery) && impRefinery) { vlTMP = true; }
                else if (vfBlock == typeof(IMyAssembler) && impAssembly) { vlTMP = true; }
                return vlTMP;
            }

            //checks what type was used and marks it as checked so the search for this cargo type is not repeated
            private void markAsRepeated(Type vfBlock)
            {
                if (vfBlock == typeof(IMyCargoContainer)) { impCargo = true; }
                else if (vfBlock == typeof(IMyShipConnector)) { impConnector = true; }
                else if (vfBlock == typeof(IMyShipDrill)) { impDrill = true; }
                else if (vfBlock == typeof(IMyShipWelder)) { impWelder = true; }
                else if (vfBlock == typeof(IMyShipGrinder)) { impGrinder = true; }
                else if (vfBlock == typeof(IMyReactor)) { impReactor = true; }
                else if (vfBlock == typeof(IMyGasGenerator)) { impGenerator = true; }
                else if (vfBlock == typeof(IMyRefinery) && impRefinery) { impRefinery = true; }
                else if (vfBlock == typeof(IMyAssembler) && impAssembly) { impAssembly = true; }
            }

            //Cycles through the inventory to catalog the requested items
            private void searchInventory(IMyTerminalBlock vfCargo, List<string> vfType, string vfSubType = "All")
            {
                if ((vfCargo.IsFunctional))
                {
                    //Items in facilities can only be listed and accessed in general functions at their proccessed items inventory. Don't touch the raw stuff
                    int index;
                    if (vfCargo is IMyRefinery || vfCargo is IMyAssembler) { index = 1; }
                    else { index = 0; }
                    IMyInventory vlInventory = vfCargo.GetInventory(index);
                    __totalVolume += vlInventory.MaxVolume;
                    __currentVolume += vlInventory.CurrentVolume;
                    MyFixedPoint vlFreeSpace = vlInventory.MaxVolume - vlInventory.CurrentVolume;
                    __count++;
                    var vlItemLst = fillListFromInventory(vlInventory);
                    //each vlI corresponds to a slot in the inventory. items may be divided into different stacks 
                    //despite being the same component type and subtype

                    for (int vlI = 0; vlI < vlItemLst.Count; vlI++)
                    {
                        //check if the item in question is within the search parametres
                        if (foundItemInInventory(vlItemLst[vlI], vfType, vfSubType))
                        {
                            TEST.Add(vlItemLst[vlI].Type.ToString());
                            //The way 6 is ridiculous. To directly compare if the itemlist has an item, I have to construct an entire IMyInventoryItem type.
                            //I can't compare it with a simple item ID, which is STUPID! Instead I have to make the following 
                            cargoLst.addItem(vfCargo.GetId(), vlFreeSpace, vlItemLst[vlI].Type.TypeId.ToString(), vlItemLst[vlI].Type.SubtypeId.ToString(), vlItemLst[vlI].Amount, vlI);
                        }
                        else
                        {
                            //adds empty string just to mark the ID and freespace
                            cargoLst.addItem(vfCargo.GetId(), vlFreeSpace, "", "", 0, 0);
                        }
                    }
                }
            }

            //auxiliary function to skil a boring step from the process of declaring the inventory lists
            private List<MyInventoryItem> fillListFromInventory(IMyInventory vfCargo)
            {
                List<MyInventoryItem> itemInv = new List<MyInventoryItem>();
                vfCargo.GetItems(itemInv);
                return itemInv;
            }

            //Turns a bool group into a list of item types. I could have done this directly, but I judged it would be easier for a non programmer to use "getCargoCount" with booleans
            private List<string> buildTypeArray(bool vfComp = false, bool vfOres = false, bool vfIngots = false, bool vfTools = false, bool vfAmmo = false)
            {
                List<string> vlTMP = new List<string>();
                if (vfComp) { vlTMP.Add("Component"); }
                if (vfOres) { vlTMP.Add("Ore"); }
                if (vfIngots) { vlTMP.Add("Ingot"); }
                if (vfTools) { vlTMP.Add("Tool"); }
                if (vfAmmo) { vlTMP.Add("Ammo"); }
                if (!vfComp && !vfOres && !vfIngots && !vfTools && !vfAmmo) { vlTMP.Add("All"); }
                return vlTMP;
            }


            //******Accessible by outside functions*******//
            //Makes the class search the designated cargo type in search of the requested type.
            //DOES NOT HANDLE REFINERIES OR ASSEMBLIES!! That's in another place
            public void getCargo(List<IMyTerminalBlock> vfBlockLst, bool vfComp = false, bool vfOres = false, bool vfIngots = false, bool vfTools = false, bool vfAmmo = false) //Every basic cargo container. Small, Medium or Large
            {
                List<string> vlTypeLst = buildTypeArray(vfComp, vfOres, vfIngots, vfTools, vfAmmo);
                //No point trying to do anything if list is empty, found an error or already searched this cargo type.
                if (null != vfBlockLst && vfBlockLst.Count != 0 && !impCargo)
                {
                    if (!isRepeatedSearch(vfBlockLst[0].GetType()))
                    {
                        markAsRepeated(vfBlockLst[0].GetType());
                        for (int vlI = 0; vlI < vfBlockLst.Count; vlI++)
                        {
                            //searches each individual inventory block for the required items
                            searchInventory(vfBlockLst[vlI], vlTypeLst);
                        }
                        __CargoRacio = calcPercentage((float)__currentVolume, (float)__totalVolume);
                    }
                }
            }
        }
    }
}