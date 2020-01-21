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
        public class cargoClass
        {
            //private string name; // field
            //public string Name   // property
            //{
            //    get { return name; }   // get method
            //    set { name = value; }  // set method
            //}
            //contains every information about the cargos, what items they have, in what cargo id, what quantities 
            //and in what position of the cargo array, for easier transfer, if needed.
            public class individualCargo
            {
                //Declare initial variables. Lists are proving to be more versatile than common arrays. Note: this is the reasonable way out. See edit below
                public List<MyTuple<string, string, float>> MaterialQuantity = new List<MyTuple<string, string, float>>();
                public List<long> ID = new List<long>();
                public List<MyFixedPoint> FreeVolume = new List<MyFixedPoint>();
                //SubType, Type, quantity, totalVolume, position, volume of 1 item
                public List<List<MyTuple<string, string, VRage.MyFixedPoint, int>>> MaterialList = new List<List<MyTuple<string, string, VRage.MyFixedPoint, int>>>();
                //I'm sorely lacking in encapsulation quality, but... one step at a time. Still... not bad for 2 day's work

                public individualCargo()
                {
                    ID.Clear();
                    FreeVolume.Clear();
                    MaterialList.Clear();
                    MaterialQuantity.Clear();
                }

                //adds items to the cargo list in a controlled fashion. Serves for transfering
                public void addItem(long vfID, MyFixedPoint vfFreespace, string vfType, string vfSubType, VRage.MyFixedPoint vfQuant, int vfPos)
                {
                    // MaterialList index has to correspond with ID index and FreeVolume index. This way, ID[10] corresponds to the FreeSpace[10] and the MaterialList[10]
                    //To achieve this, items can only be added in this method.
                    MyTuple<string, string, VRage.MyFixedPoint, int> vlNewMats = new MyTuple<string, string, VRage.MyFixedPoint, int>(vfType, vfSubType, vfQuant, vfPos);
                    //check if id in question exists.
                    if (!ID.Contains(vfID))
                    {
                        //it does not, so create. 
                        ID.Add(vfID);
                        FreeVolume.Add(vfFreespace);
                        List<MyTuple<string, string, VRage.MyFixedPoint, int>> listTMP = new List<MyTuple<string, string, VRage.MyFixedPoint, int>>();
                        listTMP.Add(vlNewMats);
                        MaterialList.Add(listTMP);
                    }
                    else
                    {
                        //it does, so increment materials to id position. No need to add or change Id or freespace, since they're the same until a transfer
                        int vlIndex = ID.IndexOf(vfID);
                        //checks if the current SubType already exists in this ID
                        List<MyTuple<string, string, VRage.MyFixedPoint, int>> vlInnerList = MaterialList[vlIndex]; //PM????
                        //it does not, so create
                        vlInnerList.Add(vlNewMats);
                        //Then replace the old list with the new 
                        MaterialList[vlIndex] = vlInnerList;
                        //Note that this system does not increment the quantity of the same material. It lists it so it is easier to access it for transfer
                        //An increment is made, but it is made on the same method that calls this one.
                    }
                    addQuantities(vfSubType, vfType, (float)vfQuant);
                }

                //adds items to the quantity list in a controlled fashion. Serves for printing info
                public void addQuantities(string vfSubType, string vfType, float vfQuantity)
                {
                    int vlIndex = MaterialQuantity.FindIndex(a => a.Item1 == vfType && a.Item2 == vfSubType); // Checks if it does have that type, otherwise no point in continuing
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
            }

            //Declare initial variables. Note that refineries and assemblies don't need this flag because they'll be managed in another way
            private bool impCargo, impReactor, impGenerator, impDrill, impWelder, impGrinder, impConnector = false;
            private MyFixedPoint __currentVolume, __totalVolume = 0;
            public MyFixedPoint CurrentVolume{ get{ return __currentVolume; } }
            public MyFixedPoint TotalVolume { get { return __totalVolume; } }
            public float CargoRacio = 0;
            public int Count = 0;
            public individualCargo cargoLst = new individualCargo();

            //Don't actually need a constructor in this case, but use it to guarantee the values are reinitialized to avoid data contamination
            public cargoClass()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false;
                __currentVolume = 0; __totalVolume = 0; CargoRacio = 0; Count = 0;
            }

            public void Clear()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false;
                __currentVolume = 0; __totalVolume = 0; CargoRacio = 0; Count = 0;
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
            }

            //Cycles through the inventory to catalog the requested items
            private void searchInventory(IMyTerminalBlock vfCargo, List<string> vfType, string vfSubType = "All")
            {
                if ((vfCargo.IsFunctional == true))
                {
                    IMyInventory vlInventory = vfCargo.GetInventory(0);
                    __totalVolume += vlInventory.MaxVolume;
                    __currentVolume += vlInventory.CurrentVolume;
                    MyFixedPoint vlFreeSpace = vlInventory.MaxVolume - vlInventory.CurrentVolume;
                    Count++;
                    var vlItemLst = fillListFromInventory(vlInventory);
                    //each vlI corresponds to a slot in the inventory. items may be divided into different stacks 
                    //despite being the same component type and subtype
                    
                    for (int vlI = 0; vlI < vlItemLst.Count; vlI++)
                    {
                        //check if the item in question is within the search parametres
                        if (foundItemInInventory(vlItemLst[vlI], vfType, vfSubType))
                        {
                            cargoLst.addItem(vfCargo.GetId(), vlFreeSpace, vlItemLst[vlI].Type.TypeId.ToString(), vlItemLst[vlI].Type.SubtypeId.ToString(), vlItemLst[vlI].Amount, vlI);
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

            //Makes the class search the designated cargo type in search of the requested type.
            //DOES NOT HANDLE REFINERIES OR ASSEMBLIES!! That's in another place
            public void getCargoCount(List<IMyTerminalBlock> vfBlockLst, bool vfComp = false, bool vfOres = false, bool vfIngots = false, bool vfTools = false, bool vfAmmo = false) //Every basic cargo container. Small, Medium or Large
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

                            /*Here we debate with the same ancient question: elegance or effectiveness? use this opportunity to gather 
                            info about blocks that besides cargo have other important data or keep things clean and simple at the cost of
                            multiple cycles through the same block? When both can me used, use both, when only one is available, I always choose 
                            effectiveness unless otherwise instructed*/

                        }
                        CargoRacio = calcPercentage((float)__currentVolume, (float)__totalVolume);
                    }
                }
            }
        }
    }
}