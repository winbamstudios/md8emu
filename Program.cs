using System;

/*
ID, Name, Description
00 NOP (no operation)
01 ADD R1,R2,R3 (add two registers together to output register)
02 SUB R1,R2,R3 (subtract two registers to output register)
03 MOV R1,R2 (copy content of register to another register)
04 MOV R1,MEM (copy content of register to point in memory)
05 MOV MEM,R1 (load content of byte specified into register)
06 MOV INT,R1 (move integer into register)
07 PUSH R1 (pushes content of register into "stack")
08 POP R1 (pulls top of stack into register)
09 HLT (halts)
10 LBL ID (function)
11 JMP ID (jumps to lbl)
12 JZ ID (jumps to lbl if zeroflag is zero)
13 JNZ ID (jumps to lbl if zeroflag is nonzero)
14 SWB INT (switches bank from RAM/0, Bus A/1, Bus B/2, or ROM/3-6)
15 MSG ID (interrupt-type thing but not really)

MSGs:
00 Print Bus A to console
01 Print Bus B to console
02 Accept key press from Bus A (Writes key to location in Bus A specified by Register A)
03 Accept key press from Bus B (Writes key to location in Bus B specified by Register A)
04 Clear currently selected bank
05 Reset processor
06 Copy current bank to bank ID in Register A
07 Jump to specific instruction specified in Register A

instructions are 4 bytes each
*/

// THIS IS MINGLEDINGLE-8 COMPLIANT (minus LBL)

namespace MingleDingle8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MD8 Emulator Mar 2026 Release");
            Console.WriteLine("© 2026 winbamstudios");
            try
            {
                Memory.Rom = File.ReadAllBytes(args[0]); // reads the bytes from the rom file and stuff
            }
            catch
            {
                Console.WriteLine("No file input or file does not exist."); // do i really need to explain this
                Environment.Exit(1);
            }
            for (Memory.ProgramCounter = 0; Memory.ProgramCounter < Memory.Rom.Length; Memory.ProgramCounter += 4) 
            {
                // executes instruction 1
                Cpu.Exec(Memory.Rom[Memory.ProgramCounter], Memory.Rom[Memory.ProgramCounter + 1], Memory.Rom[Memory.ProgramCounter + 2], Memory.Rom[Memory.ProgramCounter + 3]);
                // prints serial console
                /*
                if (Memory.SerialBus == 1)
                {
                    //Console.Clear();
                    Console.WriteLine(System.Text.Encoding.Default.GetString(Memory.BusA));
                }
                else if (Memory.SerialBus == 2)
                {
                    //Console.Clear();
                    Console.WriteLine(System.Text.Encoding.Default.GetString(Memory.BusB));
                }
                */
                // prints cycle number
                //Console.WriteLine(Memory.ProgramCounter/4);
            }
        }
    }
    public static class Memory
    {
        public static byte[] Ram = new byte[256]; // 256byte ram (bank 0)
        public static byte[] Stack = new byte[256]; // 256byte stack
        public static byte[] BusA = new byte[256]; // expanision bus a (bank 1)
        public static byte[] BusB = new byte[256]; // expansion bus b (bank 2)
        public static byte[] Rom; // 1kb program rom (banks 3, 4, 5, and 6)
        public static byte[] CMem = new byte[256]; // currently banked memory
        public static byte RegisterA; // A/251
        public static byte RegisterB; // B/252
        public static byte RegisterC; // C/253
        public static byte RegisterD; // D/254
        public static byte StackPointer; // SP
        public static Int32 ProgramCounter; // PC
        public static bool ZeroFlag = true; // Z
        public static byte CurrentBank = 0; // CB
        public static byte SerialBus = 1; // 0 is disabled, 1 is Bus A, 2 is Bus B
    }
    // cpu is entirely uncommented have fun hehe :3
    public static class Cpu
    {
        public static void Exec(byte opcode, byte input1, byte input2, byte input3)
        {
            if (Memory.CurrentBank == 0)
            {
                Memory.Ram = Memory.CMem;
                Memory.CMem = Memory.Ram;
            }
            else if (Memory.CurrentBank == 1)
            {
                Memory.BusA = Memory.CMem;
                Memory.CMem = Memory.BusA;
            }
            else if (Memory.CurrentBank == 2)
            {
                Memory.BusB = Memory.CMem;
                Memory.CMem = Memory.BusB;
            }
            else if (Memory.CurrentBank == 3)
            {
                for (int i = 0; i < 256; i++)
                {
                    Memory.CMem[i] = Memory.Rom[i];
                }
            }
            else if (Memory.CurrentBank == 4)
            {
                for (int i = 255; i < 512; i++)
                {
                    Memory.CMem[i] = Memory.Rom[i];
                }
            }
            else if (Memory.CurrentBank == 5)
            {
                for (int i = 511; i < 768; i++)
                {
                    Memory.CMem[i] = Memory.Rom[i];
                }
            }
            else if (Memory.CurrentBank == 6)
            {
                for (int i = 767; i < 1024; i++)
                {
                    Memory.CMem[i] = Memory.Rom[i];
                }
            }
            else
            {
                Console.WriteLine("Brochacho that ain't a memory bank");
                Hlt();
            }
            if ((Int32)opcode == 0)
            {
                Int32 status = Nop();
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 1)
            {
                Int32 status = Add(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 2)
            {
                Int32 status = Sub(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 3)
            {
                Int32 status = MovReg2Reg(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 4)
            {
                Int32 status = 0;
                if (Memory.CurrentBank < 3)
                {
                    status = MovReg2Mem(input1, input2);
                }
                else
                {
                    Console.WriteLine("Bank is set to " + Memory.CurrentBank.ToString() + ", which cannot be written to, as it is in ROM.");
                    Hlt();
                }
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 5)
            {
                Int32 status = MovMem2Reg(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 6)
            {
                Int32 status = MovInt2Reg(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 7)
            {
                Int32 status = Push(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 8)
            {
                Int32 status = Pop(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 9)
            {
                Hlt();
            }
            else if ((Int32)opcode == 10)
            {
                // do nothing because labels don't work like that
            }
            else if ((Int32)opcode == 11)
            {
                Int32 status = Jmp(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 12)
            {
                Int32 status = Jz(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 13)
            {
                Int32 status = Jnz(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 14)
            {
                Int32 status = Swb(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 15)
            {
                Int32 status = Msg(input1);
                if (status == 1)
                {
                    Hlt();
                }
            }
            // Instructions MBA and MBB have been deprecated in favor of memory banking.
            /*
            else if ((Int32)opcode == 14)
            {
                Int32 status = MovReg2Eba(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 15)
            {
                Int32 status = MovReg2Ebb(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 16)
            {
                Int32 status = MovEba2Reg(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 17)
            {
                Int32 status = MovEbb2Reg(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            */
            else
            {
                Console.WriteLine("Opcode " + opcode.ToString() + " does not exist.");
            }
        }
        static Int32 Nop()
        {
            return 0;
        }
        static Int32 Add(byte input1, byte input2, byte input3)
        {
            byte value1;
            byte value2;
            byte value3;
            if ((Int32)input1 == 251)
            {
                value1 = Memory.RegisterA;
            }
            else if ((Int32)input1 == 252)
            {
                value1 = Memory.RegisterB;
            }
            else if ((Int32)input1 == 253)
            {
                value1 = Memory.RegisterC;
            }
            else if ((Int32)input1 == 254)
            {
                value1 = Memory.RegisterD;
            }
            else
            {
                return 1;
            }
            if ((Int32)input2 == 251)
            {
                value2 = Memory.RegisterA;
            }
            else if ((Int32)input2 == 252)
            {
                value2 = Memory.RegisterB;
            }
            else if ((Int32)input2 == 253)
            {
                value2 = Memory.RegisterC;
            }
            else if ((Int32)input2 == 254)
            {
                value2 = Memory.RegisterD;
            }
            else
            {
                return 1;
            }
            if ((Int32)input3 == 251)
            {
                Int32 math = (Int32)value1 + (Int32)value2;
                if (math == 0)
                {
                    Memory.ZeroFlag = true;
                }
                else
                {
                    Memory.ZeroFlag = false;
                }
                Memory.RegisterA = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 252)
            {
                Int32 math = (Int32)value1 + (Int32)value2;
                if (math == 0)
                {
                    Memory.ZeroFlag = true;
                }
                else
                {
                    Memory.ZeroFlag = false;
                }
                Memory.RegisterB = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 253)
            {
                Int32 math = (Int32)value1 + (Int32)value2;
                if (math == 0)
                {
                    Memory.ZeroFlag = true;
                }
                else
                {
                    Memory.ZeroFlag = false;
                }
                Memory.RegisterC = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 254)
            {
                Int32 math = (Int32)value1 + (Int32)value2;
                if (math == 0)
                {
                    Memory.ZeroFlag = true;
                }
                else
                {
                    Memory.ZeroFlag = false;
                }
                Memory.RegisterD = Convert.ToByte(math);
                return 0;
            }
            else
            {
                return 1;
            }
            return 1;
        }
        static Int32 Sub(byte input1, byte input2, byte input3)
        {
            byte value1;
            byte value2;
            byte value3;
            if ((Int32)input1 == 251)
            {
                value1 = Memory.RegisterA;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                value1 = Memory.RegisterB;
            }
            else if ((Int32)input1 == 253)
            {
                value1 = Memory.RegisterC;
            }
            else if ((Int32)input1 == 254)
            {
                value1 = Memory.RegisterD;
            }
            else
            {
                return 1;
            }
            if ((Int32)input2 == 251)
            {
                value2 = Memory.RegisterA;
            }
            else if ((Int32)input2 == 252)
            {
                value2 = Memory.RegisterB;
            }
            else if ((Int32)input2 == 253)
            {
                value2 = Memory.RegisterC;
            }
            else if ((Int32)input2 == 254)
            {
                value2 = Memory.RegisterD;
            }
            else
            {
                return 1;
            }
            if ((Int32)input3 == 251)
            {
                Int32 math = (Int32)value1 - (Int32)value2;
                Memory.RegisterA = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 252)
            {
                Int32 math = (Int32)value1 - (Int32)value2;
                Memory.RegisterB = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 253)
            {
                Int32 math = (Int32)value1 - (Int32)value2;
                Memory.RegisterC = Convert.ToByte(math);
                return 0;
            }
            else if ((Int32)input3 == 254)
            {
                Int32 math = (Int32)value1 - (Int32)value2;
                Memory.RegisterD = Convert.ToByte(math);
                return 0;
            }
            else
            {
                return 1;
            }
            return 1;
        }
        static Int32 MovReg2Reg(byte input1, byte input2)
        {
            if ((Int32)input1 == 251)
            {
                if ((Int32)input2 == 251)
                {
                    Memory.RegisterA = Memory.RegisterA;
                    return 0;
                }
                else if ((Int32)input2 == 252)
                {
                    Memory.RegisterB = Memory.RegisterA;
                    return 0;
                }
                else if ((Int32)input2 == 253)
                {
                    Memory.RegisterC = Memory.RegisterA;
                    return 0;
                }
                else if ((Int32)input2 == 254)
                {
                    Memory.RegisterD = Memory.RegisterA;
                    return 0;
                }
                else
                {
                    return 1;
                }
                
            }
            else if ((Int32)input1 == 252)
            {
                if ((Int32)input2 == 251)
                {
                    Memory.RegisterA = Memory.RegisterB;
                    return 0;
                }
                else if ((Int32)input2 == 252)
                {
                    Memory.RegisterB = Memory.RegisterB;
                    return 0;
                }
                else if ((Int32)input2 == 253)
                {
                    Memory.RegisterC = Memory.RegisterB;
                    return 0;
                }
                else if ((Int32)input2 == 254)
                {
                    Memory.RegisterD = Memory.RegisterB;
                    return 0;
                }
                else
                {
                    return 1;
                }
                
            }
            else if ((Int32)input1 == 253)
            {
                if ((Int32)input2 == 251)
                {
                    Memory.RegisterA = Memory.RegisterC;
                    return 0;
                }
                else if ((Int32)input2 == 252)
                {
                    Memory.RegisterB = Memory.RegisterC;
                    return 0;
                }
                else if ((Int32)input2 == 253)
                {
                    Memory.RegisterC = Memory.RegisterC;
                    return 0;
                }
                else if ((Int32)input2 == 254)
                {
                    Memory.RegisterD = Memory.RegisterC;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else if ((Int32)input1 == 254)
            {
                if ((Int32)input2 == 251)
                {
                    Memory.RegisterA = Memory.RegisterD;
                    return 0;
                }
                else if ((Int32)input2 == 252)
                {
                    Memory.RegisterB = Memory.RegisterD;
                    return 0;
                }
                else if ((Int32)input2 == 253)
                {
                    Memory.RegisterC = Memory.RegisterD;
                    return 0;
                }
                else if ((Int32)input2 == 254)
                {
                    Memory.RegisterD = Memory.RegisterD;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }
        static Int32 MovReg2Mem(byte input1, byte rampos)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.CMem[ramposbutinInt32thistime] = Memory.RegisterA;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.CMem[ramposbutinInt32thistime] = Memory.RegisterB;
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.CMem[ramposbutinInt32thistime] = Memory.RegisterC;
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.CMem[ramposbutinInt32thistime] = Memory.RegisterD;
                return 0;
            }
            return 0;
        }
        static Int32 MovMem2Reg(byte rampos, byte input1)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterA = Memory.CMem[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterB = Memory.CMem[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterC = Memory.CMem[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterD = Memory.CMem[ramposbutinInt32thistime];
                return 0;
            }
            return 0;
        }
        static Int32 MovInt2Reg(byte integer, byte input1)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)integer;
                Memory.RegisterA = integer;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)integer;
                Memory.RegisterB = integer;
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)integer;
                Memory.RegisterC = integer;
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)integer;
                Memory.RegisterD = integer;
                return 0;
            }
            return 0;
        }
        static Int32 Push(byte input1)
        {
            Memory.StackPointer++;
            if ((Int32)input1 == 251)
            {
                Memory.Stack[Memory.StackPointer] = Memory.RegisterA;
            }
            else if ((Int32)input1 == 252)
            {
                Memory.Stack[Memory.StackPointer] = Memory.RegisterB;
            }
            else if ((Int32)input1 == 253)
            {
                Memory.Stack[Memory.StackPointer] = Memory.RegisterC;
            }
            else if ((Int32)input1 == 254)
            {
                Memory.Stack[Memory.StackPointer] = Memory.RegisterD;
            }
            return 0;
        }
        static Int32 Pop(byte input1)
        {
            if ((Int32)input1 == 251)
            {
                Memory.RegisterA = Memory.Stack[Memory.StackPointer];
            }
            else if ((Int32)input1 == 252)
            {
                Memory.RegisterB = Memory.Stack[Memory.StackPointer];
            }
            else if ((Int32)input1 == 253)
            {
                Memory.RegisterC = Memory.Stack[Memory.StackPointer];
            }
            else if ((Int32)input1 == 254)
            {
                Memory.RegisterD = Memory.Stack[Memory.StackPointer];
            }
            Memory.Stack[Memory.StackPointer] = (byte)0;
            Memory.StackPointer--;
            return 0;
        }
        static void Hlt()
        {
            Console.WriteLine("CPU halted.");
            System.Environment.Exit(1);
        }
        static int Jmp(byte input1)
        {
            //Memory.ProgramCounter = (int)input1;
            for (int i = 0; i < Memory.Rom.Length; i += 4) 
            {
                if ((int)Memory.Rom[i] == 10)
                {
                    if ((int)Memory.Rom[i + 1] == input1)
                    {
                        Memory.ProgramCounter = i;
                        return 0;
                    }
                }
            }
            return 0;
        }
        static int Jz(byte input1)
        {
            if (Memory.ZeroFlag)
            {
                for (int i = 0; i < Memory.Rom.Length; i += 4) 
                {
                    if ((int)Memory.Rom[i] == 10)
                    {
                        if ((int)Memory.Rom[i + 1] == input1)
                        {
                            Memory.ProgramCounter = i;
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        static int Jnz(byte input1)
        {
            if (!Memory.ZeroFlag)
            {
                for (int i = 0; i < Memory.Rom.Length; i += 4) 
                {
                    if ((int)Memory.Rom[i] == 10)
                    {
                        if ((int)Memory.Rom[i + 1] == input1)
                        {
                            Memory.ProgramCounter = i;
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        static int Swb(byte input1)
        {
            if (input1 < 7)
            {
                Memory.CurrentBank = input1;
                return 0;
            }
            else
            {
                return 1;
            }
        }
        static int Msg(byte input1)
        {
            /*
            MSGs:
            00 Print Bus A to console
            01 Print Bus B to console
            02 Accept key press from Bus A (Writes key to location in Bus A specified by Register A)
            03 Accept key press from Bus B (Writes key to location in Bus B specified by Register A)
            04 Clear currently selected bank
            05 Reset processor
            06 Copy current bank to bank ID in Register A
            07 Jump to specific instruction specified in Register A
            */
            if (input1 == 0)
            {
                foreach (byte b in Memory.BusA)
                {
                    Console.Write(b.ToString());
                }
                return 0;
            }
            else if (input1 == 1)
            {
                foreach (byte b in Memory.BusB)
                {
                    Console.Write(b.ToString());
                }
                return 0;
            }
            else if (input1 == 2)
            {
                Memory.BusA[Memory.RegisterA] = Convert.ToByte(Console.ReadKey().KeyChar);
                return 0;
            }
            else if (input1 == 3)
            {
                Memory.BusB[Memory.RegisterA] = Convert.ToByte(Console.ReadKey().KeyChar);
                return 0;
            }
            else if (input1 == 4)
            {
                if (Memory.CurrentBank < 3)
                {
                    Memory.CMem = new byte[255];
                    return 0;
                }
                else
                {
                    Console.WriteLine("ROM cannot be cleared.");
                    return 1;
                }
            }
            else if (input1 == 5)
            {
                Memory.ProgramCounter = 0;
                return 0;
            }
            else if (input1 == 6)
            {
                if (Memory.RegisterA < 3)
                {
                    if (Memory.RegisterA == 0)
                    {
                        Memory.Ram = Memory.CMem;
                    }
                    else if (Memory.RegisterA == 1)
                    {
                        Memory.BusA = Memory.CMem;
                    }
                    else if (Memory.RegisterA == 2)
                    {
                        Memory.BusB = Memory.CMem;
                    }
                    return 0;
                }
                else
                {
                    Console.WriteLine("Cannot write to ROM!");
                    return 1;
                }
            }
            else if (input1 == 7)
            {
                Memory.ProgramCounter = Memory.RegisterA;
                return 0;
            }
            else
            {
                Console.WriteLine("MSG ID invalid");
                return 1;
            }
        }
        static Int32 MovReg2Eba(byte input1, byte rampos)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusA[ramposbutinInt32thistime] = Memory.RegisterA;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusA[ramposbutinInt32thistime] = Memory.RegisterB;
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusA[ramposbutinInt32thistime] = Memory.RegisterC;
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusA[ramposbutinInt32thistime] = Memory.RegisterD;
                return 0;
            }
            return 0;
        }
        static Int32 MovReg2Ebb(byte input1, byte rampos)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusB[ramposbutinInt32thistime] = Memory.RegisterA;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusB[ramposbutinInt32thistime] = Memory.RegisterB;
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusB[ramposbutinInt32thistime] = Memory.RegisterC;
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.BusB[ramposbutinInt32thistime] = Memory.RegisterD;
                return 0;
            }
            return 0;
        }
        static Int32 MovEba2Reg(byte input1, byte rampos)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterA = Memory.BusA[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterB = Memory.BusA[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterC = Memory.BusA[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterD = Memory.BusA[ramposbutinInt32thistime];
                return 0;
            }
            return 0;
        }
        static Int32 MovEbb2Reg(byte input1, byte rampos)
        {
            if ((Int32)input1 == 251)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterA = Memory.BusB[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterB = Memory.BusB[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterC = Memory.BusB[ramposbutinInt32thistime];
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                Int32 ramposbutinInt32thistime = (Int32)rampos;
                Memory.RegisterD = Memory.BusB[ramposbutinInt32thistime];
                return 0;
            }
            return 0;
        }
    }
}