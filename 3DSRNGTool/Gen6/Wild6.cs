﻿using PKHeX.Core;
using Pk3DSRNGTool.Core;

namespace Pk3DSRNGTool
{
    public class Wild6 : WildRNG
    {
        private static uint getrand => RNGPool.getrand;
        private static uint rand(uint n) => (uint)(getrand * (ulong)n >> 32);
        private static void Advance(int n) => RNGPool.Advance(n);

        private static uint getTinyRand => RNGPool.tinystatus.Nextuint();
        private static byte TinyRand(int n) => (byte)(getTinyRand * (ulong)n >> 32);
        private static void tiny_Advance(int n)
        {
            for (int i = n; i > 0; i--)
                RNGPool.AdvanceTiny();
        }

        private bool getSync => getTinyRand < 0x80000000;
        private void Prepare(ResultW6 rt)
        {
            if (RNGPool.tinystatus == null)
            {
                rt.Slot = slot = 1;
                return;
            }
            // Delay
            switch (Wildtype)
            {
                case EncounterType.RockSmash:
                    RNGPool.AdvanceMT(16);
                    tiny_Advance(3);
                    RNGPool.AdvanceMT(RNGPool.DelayTime - 228);
                    tiny_Advance(1);
                    RNGPool.AdvanceMT(212);
                    rt.IsPokemon = TinyRand(100) <= 30;
                    break;
                default:
                    RNGPool.AdvanceMT(RNGPool.DelayTime);
                    break;
            }
            // Sync
            rt.Synchronize = getSync;
            // Others
            switch (Wildtype)
            {
                case EncounterType.FriendSafari:
                    rt.IsPokemon = TinyRand(100) < 13;
                    rt.Slot = slot = (byte)(TinyRand(SlotNum) + 1);
                    rt.Item = TinyRand(100);
                    rt.ItemStr = getitemstr(rt.Item);
                    break;
                case EncounterType.TrashCan:
                    tiny_Advance(2);
                    rt.Slot = getslot(TinyRand(100));
                    break;
                case EncounterType.PokeRadar:
                    rt.Slot = IsShinyLocked ? slot = 1 : getslot(TinyRand(100));
                    break;
                case EncounterType.SingleSlot:
                    rt.Slot = slot = 1;
                    break;
                default:
                    rt.Slot = getslot(TinyRand(100));
                    break;
            }
        }

        public EncounterType Wildtype;
        public bool HA;
        public bool IsShinyLocked;
        public int _PIDroll_count;
        protected override int PIDroll_count => _PIDroll_count;
        public int _ivcnt = -1;
        protected override int PerfectIVCount => System.Math.Max(_ivcnt, IV3[slot] ? 3 : 0);
        public int BlankGenderRatio;
        public byte SlotNum;

        public byte[] SlotLevel;
        public bool CompoundEye;

        public override RNGResult Generate()
        {
            var rt = new ResultW6();
            Prepare(rt);
            Advance(60);
            Generate_Once(rt);
            return rt;
        }

        public ResultW6[] Generate_Horde()
        {
            var results = new ResultW6[5];
            for (int i = 0; i < 5; i++)
                results[i] = new ResultW6();
            bool Sync = false;
            if (RNGPool.tinystatus == null)
            {
                Sync = getSync;
                MarkHA(results);
            }
            Advance(60);
            for (int i = 0; i < 5; i++)
            {
                results[i].Synchronize = Sync;
                slot = results[i].Slot = (byte)(i + 1);
                Generate_Once(results[i]);
            }
            return results;
        }

        private void Generate_Once(ResultW6 rt)
        {
            //Level
            rt.Level = SlotLevel[slot];

            //Encryption Constant
            rt.EC = getrand;

            //PID
            for (int i = PIDroll_count; i > 0; i--)
            {
                rt.PID = getrand;
                if (rt.PSV == TSV)
                {
                    if (IsShinyLocked)
                        rt.PID ^= 0x10000000;
                    else
                        rt.Shiny = true;
                    break;
                }
            }

            //IV
            rt.IVs = new int[6];
            for (int i = PerfectIVCount; i > 0;)
            {
                uint tmp = rand(6);
                if (rt.IVs[tmp] == 0)
                {
                    i--; rt.IVs[tmp] = 31;
                }
            }
            for (int i = 0; i < 6; i++)
                if (rt.IVs[i] == 0)
                    rt.IVs[i] = (int)(getrand >> 27);

            //Ability
            rt.Ability = (byte)(rt.Ability < 3 ? (HA ? rand(3) : (getrand >> 31)) + 1 : 3);

            //Nature
            rt.Nature = (byte)(rt.Synchronize & Synchro_Stat < 25 ? Synchro_Stat : rand(25));

            //Gender
            rt.Gender = (byte)(RandomGender[slot] ? (rand(252) >= Gender[slot] ? 1 : 2) : Gender[slot]);
        }

        public override void Markslots()
        {
            IV3 = new bool[SpecForm.Length];
            RandomGender = new bool[SpecForm.Length];
            Gender = new byte[SpecForm.Length];
            for (int i = 0; i < SpecForm.Length; i++)
            {
                if (SpecForm[i] == 0)
                {
                    if (i == 0)
                        continue;
                    Gender[i] = FuncUtil.getGenderRatio(BlankGenderRatio);
                    RandomGender[i] = FuncUtil.IsRandomGender(BlankGenderRatio);
                    continue;
                }
                PersonalInfo info = PersonalTable.ORAS.getFormeEntry(SpecForm[i] & 0x7FF, SpecForm[i] >> 11);
                byte genderratio = (byte)info.Gender;
                IV3[i] = info.EggGroups[0] == 0xF;
                Gender[i] = FuncUtil.getGenderRatio(genderratio);
                RandomGender[i] = FuncUtil.IsRandomGender(genderratio);
            }
            _PIDroll_count += ShinyCharm && !IsShinyLocked ? 3 : 1;
        }

        private string getitemstr(int rand)
        {
            if (rand < (CompoundEye ? 60 : 50))
                return "50%";
            if (rand < (CompoundEye ? 80 : 55))
                return "5%";
            if (rand < (CompoundEye ? 85 : 56))
                return "1%";
            return "-";
        }

        private void MarkHA(ResultW6[] horde)
        {
            int idx = TinyRand(6);
            if (idx > 4)
                return;
            horde[idx].Ability = 3;
        }
    }
}
