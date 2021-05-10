﻿using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.ROM
{
    public class ROM32K : BaseIODevice
    {
        public ROM32K(Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(bus, startAddress, endAddress, mode)
        {
            bus.Subscribe<RomLoadArgs>(Load);
        }

        public void Load(RomLoadArgs arg)
        {
            if (arg.Data.Length > memory.Length)
            {
                throw new InvalidOperationException($"Attempting to load ROM with incorrect data size. ROM size is {memory.Length} bytes, and data load size is {arg.Data.Length} bytes.");
            }

            for (int index = 0; index < memory.Length; index++)
            {
                memory[index] = arg.Data[index];
            }
        }


    }
}