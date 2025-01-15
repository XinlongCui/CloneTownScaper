//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TS
{
    public class WaveCollapseFunction : MonoBehaviour
    {
        private void Start()
        {
            Vertex.NeedCollapse += NeedCollapse;
        }
        private void NeedCollapse(Vertex collapseVertex)
        {
            collapseVertex.GetGoingToCollapseCube(collapseCubes);
            //先尝试小范围
            GetSmallRangeCollapseCubes(collapseCubes);
            bool isSmallRangeTrySucceed = WFC();
            //所有的与之相连的大范围
            if (!isSmallRangeTrySucceed)
            {
                GetBigRangeCollapseCubes(collapseCubes.First());
                bool isBigRangeTrySucceed = WFC();
            }

            history = new Stack<Operation>();
            collapseCubes = new HashSet<Cube>();
            propagateCubes = new Stack<Cube>();
            System.GC.Collect();
        }


        Stack<Operation> history = new Stack<Operation>();//用于回溯
        public HashSet<Cube> collapseCubes = new HashSet<Cube>();
        public Stack<Cube> propagateCubes = new Stack<Cube>();
        public bool WFC()
        {
            while (collapseCubes.Count > 0) {
                while (propagateCubes.Count == 0) {//有传播则传播，没传播有坍缩则坍缩
                    if (Collapse() == false)
                    {
                        Backtrack();
                        if (history.Count == 0) { return false; }
                        continue;
                    }
                }
                Propagate();
            }
            return true;
        }
        public bool Collapse()
        {
            Cube nowCollapseCube = GetMinPossibleModulesCube();

            if (nowCollapseCube.bit == "00000000" || nowCollapseCube.bit == "11111111") { nowCollapseCube.SetG_Module(null); }
            else if (nowCollapseCube.possibleModules.Count == 0)//产生冲突
            {   
                if (history.Count == 0 || history.Peek().opreatedCube != nowCollapseCube) {    
                    history.Push(new Operation()
                    {
                        opreatedCube = nowCollapseCube,
                        isCollapseOrPropagate = 'C',
                        removedModules = nowCollapseCube.possibleModules.ConvertAll(x => x)
                    });
                }
                history.Push(new Operation()
                {
                    opreatedCube = nowCollapseCube,
                    isCollapseOrPropagate = 'C',
                    removedModules = new List<Module>()
                });
                return false;
            }
            else
            {
                Module randomModule = nowCollapseCube.possibleModules[Random.Range(0, nowCollapseCube.possibleModules.Count)];             
                nowCollapseCube.SetG_Module(randomModule);
                
                //添加坍缩前cube的完整possibleModule
                if (history.Count==0 || history.Peek().opreatedCube != nowCollapseCube)
                {
                    history.Push(new Operation()
                    {
                        opreatedCube = nowCollapseCube,
                        isCollapseOrPropagate = 'C',
                        removedModules = nowCollapseCube.possibleModules.ConvertAll(x => x)
                    });
                }
                //添加 去除了某个产生冲突的possibleModule之后的 坍缩历史
                nowCollapseCube.possibleModules.Remove(randomModule);//一旦确定一个，相当于剩下的都被移除了
                List<Module> removedModules = nowCollapseCube.possibleModules;
                nowCollapseCube.possibleModules = new List<Module>() { randomModule };
                history.Push(new Operation()
                {
                    opreatedCube = nowCollapseCube,
                    isCollapseOrPropagate = 'C',
                    removedModules = removedModules.ConvertAll(x=>x)//添加移除Module的历史
                });               
            }
            //加入传播“队列”
            propagateCubes.Push(nowCollapseCube);//即使是全零的也要加入，通过它找其邻居
            //移出坍缩队列
            collapseCubes.Remove(nowCollapseCube);
            return true;
        }
        public void Propagate()
        {
            Cube nowPropagateCube = propagateCubes.Pop();
            List<Module> nowPropagateCubePossibleModules = nowPropagateCube.possibleModules;
            if (nowPropagateCubePossibleModules.Count == 0) return;//因为传播导致没有可能的模块，这里不进行处理，会由坍缩捕捉到进行处理

            Dictionary<int, List<string>> allowedSockets = new Dictionary<int, List<string>>();//6个方向，每个方向上的sockets（可能有多个Module）
            for(int i = 0; i < 6; i++) { allowedSockets.Add(i, new List<string>()); }
            foreach (Module module in nowPropagateCubePossibleModules) {     
                for (int i = 0; i < 6; i++)
                {
                    allowedSockets[i].AddRange(Module.socketMatchRules[module.sockets[i]]);//允许一对多的情况下
                }
            }

            if (nowPropagateCube.neighborCubes.Count == 0) nowPropagateCube.SetNeighborCubes();
            for(int i = 0; i < 6; i++)
            {
                Cube neighborCube = nowPropagateCube.neighborCubes[i]; 
                if (neighborCube == null) continue;//有可能某一方向上没有邻居


                List<Module> removedModules = new List<Module>();
                for(int moduleIndex=neighborCube.possibleModules.Count - 1; moduleIndex >=0; moduleIndex--)//这里遍历的同时可能会移除，所以倒着遍历
                {
                    Module module = neighborCube.possibleModules[moduleIndex];
                    if (!allowedSockets[i].Contains(module.sockets[Module.socketCompareRules[i]]))
                    {                     
                        removedModules.Add(module);
                        neighborCube.possibleModules.Remove(module);
                    }
                }
                if (removedModules.Count > 0)//状态改变，要继续传播其影响
                {
                    //加入传播“队列”
                    propagateCubes.Push(neighborCube);
                    //如果不在坍缩队列中,加入坍缩队列
                    if(!collapseCubes.Contains(neighborCube)) collapseCubes.Add(neighborCube);
                    //添加传播历史
                    history.Push(new Operation()
                    {
                        opreatedCube = neighborCube,
                        isCollapseOrPropagate = 'P',
                        removedModules = removedModules//添加移除Module的历史
                    });
                }
            }
        }
        public void Backtrack()
        {
            //坍缩操作之前保存坍缩cube的所有状态，用于 当待坍缩的cube的所有可能的module都会产生冲突时，返回上一级坍缩过程
            //因为在尝试待坍缩的cube的所有可能的module，产生冲突的module都已经移除了
            Operation collapseFailedOperation = history.Pop();
            Operation beforeCollapseFailedOperation = history.Pop();
            collapseFailedOperation.opreatedCube.possibleModules = beforeCollapseFailedOperation.removedModules;

            while (history.Count !=0 && history.Peek().isCollapseOrPropagate != 'C') //找到上一个坍缩的，换种module再次尝试
            {
                Cube operatedCube = history.Peek().opreatedCube;
                List<Module> removedModules = history.Pop().removedModules;
                foreach (Module module in removedModules) {
                    operatedCube.possibleModules.Add(module);//将移除的Module重新添加回去（还原状态）
                }
            }
            
            if (history.Count != 0) {
                Cube collapseCube = history.Peek().opreatedCube;
                collapseCube.possibleModules.Clear();//只有一个，但是失败了，换其他的进行尝试
                foreach (Module module in history.Pop().removedModules)
                {
                    collapseCube.possibleModules.Add(module);//将移除的Module重新添加回去（还原状态）
                }
                collapseCubes.Add(collapseCube);//添加回去，再次尝试其他进行坍缩
            }
        }

        
        public Cube GetMinPossibleModulesCube(){
            Cube minPossibleModulesCube = collapseCubes.First();
            int minPossibleModulesNum = minPossibleModulesCube.possibleModules.Count;
            foreach (Cube cube in collapseCubes)
            {
                if(cube.possibleModules.Count < minPossibleModulesNum)
                {
                    minPossibleModulesCube = cube;
                    minPossibleModulesNum = minPossibleModulesCube.possibleModules.Count;
                }
            }
            return minPossibleModulesCube;
        }
        public void GetBigRangeCollapseCubes(Cube startCube)
        {
            HashSet<Cube> cubes = new HashSet<Cube>() { startCube};
            while (cubes.Count > 0) {
                Cube cube = cubes.First();
                cubes.Remove(cube);
                collapseCubes.Add(cube);
                cube.UpdateBit();//重新获取可能module

                if (cube.neighborCubes.Count == 0) { cube.SetNeighborCubes(); }
                foreach (Cube neighborCube in cube.neighborCubes.Values) { 
                    if(neighborCube!=null && !collapseCubes.Contains(neighborCube)) cubes.Add(neighborCube);
                }
            }
        }

        public void GetSmallRangeCollapseCubes(HashSet<Cube> goingToCollapseCubes)
        {
            HashSet<Cube> cubes = new HashSet<Cube>() {  };
            foreach(Cube cube in goingToCollapseCubes) { cubes.Add(cube); cube.module = null;}//有些可能是添加/删除了新点，有模型，但也需要重置 

            while (cubes.Count > 0)
            {
                Cube cube = cubes.First();
                cubes.Remove(cube);
                cube.UpdateBit();//重新获取可能module
                collapseCubes.Add(cube);

                if (cube.module == null)//已经确定模型的邻居，不在继续判断其邻居
                {
                    if (cube.neighborCubes.Count == 0) { cube.SetNeighborCubes(); }
                    foreach (Cube neighborCube in cube.neighborCubes.Values)
                    {
                        if (neighborCube != null && !collapseCubes.Contains(neighborCube)) cubes.Add(neighborCube);
                    }
                }
            }
        }


    }   
    struct Operation
    {
        public Cube opreatedCube;
        public char isCollapseOrPropagate;//'C' for collapse,'P' for propagate
        public List<Module> removedModules;
    }
}
