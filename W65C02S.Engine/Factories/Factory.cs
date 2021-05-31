using System;
using System.IO;
using System.Linq;
using System.Reflection;
using W65C02.API.Enums;
using W65C02.API.Interfaces;
using W65C02.API.Models;
using W65C02S.Engine.Devices;

namespace W65C02S.Engine.Factories
{
    public static class Factory
    {
        //public static IEmulator CreateEmulator(IBus bus)
        //{
        //    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    var sss = Assembly.LoadFile($"{path}\\W65C02S.Engine.dll");
        //    if (sss == null)
        //        return null;
        //    var type = sss.GetTypes().FirstOrDefault(x => x.Name == "Emulator");
        //    if (type == null)
        //        return null;


        //    return (IEmulator)Activator.CreateInstance(type, bus);
        //}

        public static IBus CreateBus()
        {
            return new Bus();
        }

        public static IROM CreateROM(IBus bus, ushort startAddress)
        {
            return new ROM(bus, startAddress);
        }

        public static IMemoryMappedDevice CreateRAM(IBus bus, ushort startAddress)
        {
            return new RAM(bus, startAddress);
        }


    }
}
