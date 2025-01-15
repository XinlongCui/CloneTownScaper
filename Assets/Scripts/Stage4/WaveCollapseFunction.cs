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
            //�ȳ���С��Χ
            GetSmallRangeCollapseCubes(collapseCubes);
            bool isSmallRangeTrySucceed = WFC();
            //���е���֮�����Ĵ�Χ
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


        Stack<Operation> history = new Stack<Operation>();//���ڻ���
        public HashSet<Cube> collapseCubes = new HashSet<Cube>();
        public Stack<Cube> propagateCubes = new Stack<Cube>();
        public bool WFC()
        {
            while (collapseCubes.Count > 0) {
                while (propagateCubes.Count == 0) {//�д����򴫲���û������̮����̮��
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
            else if (nowCollapseCube.possibleModules.Count == 0)//������ͻ
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
                
                //���̮��ǰcube������possibleModule
                if (history.Count==0 || history.Peek().opreatedCube != nowCollapseCube)
                {
                    history.Push(new Operation()
                    {
                        opreatedCube = nowCollapseCube,
                        isCollapseOrPropagate = 'C',
                        removedModules = nowCollapseCube.possibleModules.ConvertAll(x => x)
                    });
                }
                //��� ȥ����ĳ��������ͻ��possibleModule֮��� ̮����ʷ
                nowCollapseCube.possibleModules.Remove(randomModule);//һ��ȷ��һ�����൱��ʣ�µĶ����Ƴ���
                List<Module> removedModules = nowCollapseCube.possibleModules;
                nowCollapseCube.possibleModules = new List<Module>() { randomModule };
                history.Push(new Operation()
                {
                    opreatedCube = nowCollapseCube,
                    isCollapseOrPropagate = 'C',
                    removedModules = removedModules.ConvertAll(x=>x)//����Ƴ�Module����ʷ
                });               
            }
            //���봫�������С�
            propagateCubes.Push(nowCollapseCube);//��ʹ��ȫ���ҲҪ���룬ͨ���������ھ�
            //�Ƴ�̮������
            collapseCubes.Remove(nowCollapseCube);
            return true;
        }
        public void Propagate()
        {
            Cube nowPropagateCube = propagateCubes.Pop();
            List<Module> nowPropagateCubePossibleModules = nowPropagateCube.possibleModules;
            if (nowPropagateCubePossibleModules.Count == 0) return;//��Ϊ��������û�п��ܵ�ģ�飬���ﲻ���д�������̮����׽�����д���

            Dictionary<int, List<string>> allowedSockets = new Dictionary<int, List<string>>();//6������ÿ�������ϵ�sockets�������ж��Module��
            for(int i = 0; i < 6; i++) { allowedSockets.Add(i, new List<string>()); }
            foreach (Module module in nowPropagateCubePossibleModules) {     
                for (int i = 0; i < 6; i++)
                {
                    allowedSockets[i].AddRange(Module.socketMatchRules[module.sockets[i]]);//����һ�Զ�������
                }
            }

            if (nowPropagateCube.neighborCubes.Count == 0) nowPropagateCube.SetNeighborCubes();
            for(int i = 0; i < 6; i++)
            {
                Cube neighborCube = nowPropagateCube.neighborCubes[i]; 
                if (neighborCube == null) continue;//�п���ĳһ������û���ھ�


                List<Module> removedModules = new List<Module>();
                for(int moduleIndex=neighborCube.possibleModules.Count - 1; moduleIndex >=0; moduleIndex--)//���������ͬʱ���ܻ��Ƴ������Ե��ű���
                {
                    Module module = neighborCube.possibleModules[moduleIndex];
                    if (!allowedSockets[i].Contains(module.sockets[Module.socketCompareRules[i]]))
                    {                     
                        removedModules.Add(module);
                        neighborCube.possibleModules.Remove(module);
                    }
                }
                if (removedModules.Count > 0)//״̬�ı䣬Ҫ����������Ӱ��
                {
                    //���봫�������С�
                    propagateCubes.Push(neighborCube);
                    //�������̮��������,����̮������
                    if(!collapseCubes.Contains(neighborCube)) collapseCubes.Add(neighborCube);
                    //��Ӵ�����ʷ
                    history.Push(new Operation()
                    {
                        opreatedCube = neighborCube,
                        isCollapseOrPropagate = 'P',
                        removedModules = removedModules//����Ƴ�Module����ʷ
                    });
                }
            }
        }
        public void Backtrack()
        {
            //̮������֮ǰ����̮��cube������״̬������ ����̮����cube�����п��ܵ�module���������ͻʱ��������һ��̮������
            //��Ϊ�ڳ��Դ�̮����cube�����п��ܵ�module��������ͻ��module���Ѿ��Ƴ���
            Operation collapseFailedOperation = history.Pop();
            Operation beforeCollapseFailedOperation = history.Pop();
            collapseFailedOperation.opreatedCube.possibleModules = beforeCollapseFailedOperation.removedModules;

            while (history.Count !=0 && history.Peek().isCollapseOrPropagate != 'C') //�ҵ���һ��̮���ģ�����module�ٴγ���
            {
                Cube operatedCube = history.Peek().opreatedCube;
                List<Module> removedModules = history.Pop().removedModules;
                foreach (Module module in removedModules) {
                    operatedCube.possibleModules.Add(module);//���Ƴ���Module������ӻ�ȥ����ԭ״̬��
                }
            }
            
            if (history.Count != 0) {
                Cube collapseCube = history.Peek().opreatedCube;
                collapseCube.possibleModules.Clear();//ֻ��һ��������ʧ���ˣ��������Ľ��г���
                foreach (Module module in history.Pop().removedModules)
                {
                    collapseCube.possibleModules.Add(module);//���Ƴ���Module������ӻ�ȥ����ԭ״̬��
                }
                collapseCubes.Add(collapseCube);//��ӻ�ȥ���ٴγ�����������̮��
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
                cube.UpdateBit();//���»�ȡ����module

                if (cube.neighborCubes.Count == 0) { cube.SetNeighborCubes(); }
                foreach (Cube neighborCube in cube.neighborCubes.Values) { 
                    if(neighborCube!=null && !collapseCubes.Contains(neighborCube)) cubes.Add(neighborCube);
                }
            }
        }

        public void GetSmallRangeCollapseCubes(HashSet<Cube> goingToCollapseCubes)
        {
            HashSet<Cube> cubes = new HashSet<Cube>() {  };
            foreach(Cube cube in goingToCollapseCubes) { cubes.Add(cube); cube.module = null;}//��Щ���������/ɾ�����µ㣬��ģ�ͣ���Ҳ��Ҫ���� 

            while (cubes.Count > 0)
            {
                Cube cube = cubes.First();
                cubes.Remove(cube);
                cube.UpdateBit();//���»�ȡ����module
                collapseCubes.Add(cube);

                if (cube.module == null)//�Ѿ�ȷ��ģ�͵��ھӣ����ڼ����ж����ھ�
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
