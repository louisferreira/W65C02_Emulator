using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W6502C.CPU
{
    public class RAM : IDevice
    {

        private byte[] data;
        
        public byte[] Data { 
            get {
                return data;
            } 
            set
            {
                data = value;
            }
        }
        


        public RAM(byte kBsize)
        {
            data = new byte[1024 * kBsize];

            for (int index = 0; index < data.Length; index++)
            {
                data[index] = 0xEA;
            }

            
        }

        public byte Read(ushort address)
        {
            return data[address];
        }

       
        public void Write(ushort address, byte newData)
        {
            data[address] = newData;
        }

        public void SetupDummyProgram()
        {
            var x = 0x0200;

            data[x] = 0xa9; x++;
            data[x] = 0x0a; x++;
            data[x] = 0x85; x++;
            data[x] = 0x01; x++;
            data[x] = 0xa9; x++;
            data[x] = 0x0c; x++;
            data[x] = 0x85; x++;
            data[x] = 0x02; x++;
            data[x] = 0xa9; x++;
            data[x] = 0xaa; x++;
            data[x] = 0x85; x++;
            data[x] = 0x0a; x++;
            data[x] = 0xa9; x++;
            data[x] = 0xbb; x++;
            data[x] = 0x85; x++;
            data[x] = 0x0b; x++;
            data[x] = 0xa9; x++;
            data[x] = 0xcc; x++;
            data[x] = 0x85; x++;
            data[x] = 0x0c; x++;
            data[x] = 0xa9; x++;
            data[x] = 0xdd; x++;
            data[x] = 0x8d; x++;
            data[x] = 0x0a; x++;
            data[x] = 0x0c; x++;

            data[x] = 0xa9; x++;
            data[x] = 0xee; x++;
            data[x] = 0x8d; x++;
            data[x] = 0x0c; x++;
            data[x] = 0xea; x++;

            data[x] = 0xa9; x++;
            data[x] = 0x00; x++;
            data[x] = 0xa2; x++;
            data[x] = 0x01; x++;
            data[x] = 0xa5; x++;
            data[x] = 0x01; x++;
            data[x] = 0xb2; x++;
            data[x] = 0x01; x++;
            data[x] = 0xa1; x++;
            data[x] = 0x01; x++;
            data[x] = 0xea; x++;
            data[x] = 0x4c; x++;
            data[x] = 0x1b; x++;
            data[x] = 0x02;

            /*
            	.ORG $0200
	            LDA #$0A
	            STA $01

	            LDA #$0C
	            STA $02

	            LDA #$AA
	            STA $0A

	            LDA #$BB
	            STA $0B
	
	            LDA #$CC
	            STA $0C

	            LDA #$DD
	            STA $0C0A
	
	            LDA #$EE
	            STA $EA0C

	            LDA #0
	
            start:		
	            LDX #1
	            LDA $01
	            LDA ($01)
	            LDA ($01, X)
	            NOP
	            JMP start

            */

            data[0xFFFC] = 0x00;
            data[0xFFFD] = 0x02;
        }
    }
}
