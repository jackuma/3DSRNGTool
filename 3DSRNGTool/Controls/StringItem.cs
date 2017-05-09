﻿using System.Linq;
using Pk3DSRNGTool.Controls;

namespace Pk3DSRNGTool
{
    class StringItem
    {
        public static string[] naturestr = new bool[25].Select(i => "").ToArray();
        public static string[] hpstr = new bool[18].Select(i => "").ToArray();
        public static string[] genderratio = new bool[7].Select(i => "").ToArray();

        public static string[] genderstr = { "-", "♂", "♀" };
        public static string[] abilitystr = { "-", "1", "2", "H" };
        public static string[] eventabilitystr = { "1/2", "1/2/H" };

        public static string[] gen7wildtypestr = { "-", "UB", "QR" };

        public static string[] species;
        public static string[] smlocation, gen6location;
        public static string[] items;

        public static ComboItem[] NatureList
            => naturestr.Select((str, i) => new ComboItem(str, i)).ToArray();

        public static ComboItem[] HiddenPowerList
            => hpstr.Skip(1).Take(16).Select((str, i) => new ComboItem(str, i)).ToArray();

        private static byte[] genderratiodata = new byte[] { 0xFF, 0x7F, 0x1F, 0x3F, 0xBF, 0x00, 0xFE };

        public static ComboItem[] GenderRatioList
            => genderratio.Select((str, i) => new ComboItem(str, genderratiodata[i])).ToArray();

        public static string getlocationstr(int locationidx, int ver)
        {
            switch (ver)
            {
                case 2:
                case 3:
                    return gen6location[locationidx & 0x1FF] + LocationTable6.Table_ORAS.FirstOrDefault(t => t.Locationidx == locationidx).mark;
                case 4:
                case 5:
                    return smlocation[locationidx & 0xFF] + LocationTable7.Table.FirstOrDefault(t => t.Locationidx == locationidx).mark;
                default:
                    return "";
            }
        }

        private static string[][] Translation =
        {
            new [] { "Legendary", "定点传说" },
            new [] { "Starters", "御三家" },
            new [] { "In-Game Gift", "礼物" },
            new [] { "Pokemon Link", "宝可梦连接" },
            new [] { "Normal Stationary","普通定点" },
            new [] { "Fossils", "化石" },
            new [] { "Hoenn Legendary", "丰缘传说" },
            new [] { "Johto Legendary", "城都传说" },
            new [] { "Sinnoh Legendary", "神奥传说" },
            new [] { "Unova Legendary", "合众传说" },
            new [] { "Island Scan", "岛屿搜索" },
            new [] { "Normal Wild", "普通野外" },
            new [] { "Johto Starters", "城都御三家" },
            new [] { "Legendary Titans", "三神柱" },
            new [] { "Not Impled", "功能未开放" },
        };

        public static string Translate(string input, int language)
        {
            if (0 >= language || language >= 2)
                return input;
            foreach (string[] a in Translation)
                if (input == a[0])
                    return a[language];
            return input;
        }
    }
}