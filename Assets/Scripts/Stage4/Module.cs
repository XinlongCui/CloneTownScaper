using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace TS
{
    public class Module
    {
        //һ��mesh��6��socket
        public Mesh mesh;
        //6������ÿ�������socket
        //��ΪX�ᣬ��ΪZ�ᣨ��unityһ�£�
        //      3-->0
        //          |
        //      2<--1
        //ÿ�����bit��ʾΪ
        //��11110000��
        //��00001111��

        //��11001100��
        //��01100110��

        //��00110011��
        //��10011001��
        public List<string> sockets = new List<string>() { "a", "a", "a", "a", "a", "a"};
        //�ĸ�socketӦ�����ĸ�socketȥƥ��
        public static Dictionary<int,int> socketCompareRules = new Dictionary<int, int>
        {
            { 0, 1 },
            { 1, 0 },

            { 2, 5},
            { 3, 4},

            { 4, 3 },
            { 5, 2 },
        };
        //����socket��������Щsocketsƥ�䣨����1�Զࣩ
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
