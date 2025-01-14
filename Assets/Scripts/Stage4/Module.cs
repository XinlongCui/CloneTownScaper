using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace TS
{
    public class Module
    {
        //一个mesh和6个socket
        public Mesh mesh;
        //6个方向，每个方向的socket
        //横为X轴，纵为Z轴（与unity一致）
        //      3-->0
        //          |
        //      2<--1
        //每个面的bit表示为
        //“11110000”
        //“00001111”

        //“11001100”
        //“01100110”

        //“00110011”
        //“10011001”
        public List<string> sockets = new List<string>() { "a", "a", "a", "a", "a", "a"};
        //哪个socket应该与哪个socket去匹配
        public static Dictionary<int,int> socketCompareRules = new Dictionary<int, int>
        {
            { 0, 1 },
            { 1, 0 },

            { 2, 5},
            { 3, 4},

            { 4, 3 },
            { 5, 2 },
        };
        //这种socket可以与哪些sockets匹配（允许1对多）
        public static Dictionary<string,List<string>> socketMatchRules = new Dictionary<string,List<string>>()
        {
            {"a",new List<string>{"a" } },
            {"b",new List<string>{"b" }},
            {"c",new List<string>{"c" }},
            {"z",new List<string>{"z" }},
            {"y",new List<string>{"y" }},
        };
    }
}
