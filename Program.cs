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
16 MUL R1,R2,R3 (multiplies two registers to output register)
17 DIV R1,R2,R3 (divides R1 by R2 and outputs quotient to R3)
18 MOD R1,R2,R3 (divides R1 by R2 and outputs remainder to R3)
19 JEQ R1,R2,ID (jumps to label if R1 equals R2)
20 JLT R1,R2,ID (jumps to label if R1 is less than R2)
21 JGT R1,R2,ID (jumps to label if R1 is greater than R2)
22 INB R1,ID (takes byte from Bus A (0) or Bus B (1) and copies to R1)
23 OUTB R1,ID  (takes R1 and copies it to Bus A (0) or Bus B (1))

MSGs:
00 Print Bus A to console
01 Print Bus B to console
02 Accept key press from Bus A
03 Accept key press from Bus B
04 Unused
05 Reset processor
06 Unused
07 Jump to specific instruction specified in Register A
08 Switch to 8-bit addressing
09 Switch to 16-bit addressing

instructions are 4 bytes each
*/

namespace MingusDingus8
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
        public static byte[] Ram = new byte[16384]; // 16kb ram
        public static byte[] Stack = new byte[256]; // 256byte stack
        public static byte BusA = 0; // serial bus a
        public static byte BusB = 0; // serial bus b
        public static byte[] Rom; // program rom
        public static byte[] CMem = new byte[65536]; // currently banked memory
        public static byte RegisterA; // A/251
        public static byte RegisterB; // B/252
        public static byte RegisterC; // C/253
        public static byte RegisterD; // D/254
        public static byte StackPointer; // SP/255
        public static ushort ProgramCounter; // PC
        public static bool ZeroFlag = true; // Z
        public static bool Addr16Bit = true; // 16-bit addressing toggle
        public static byte CurrentBank = 0; // CB
        public static byte MemPointerA = 0; // MP - used in 8-bit and 16-bit mode
        public static byte MemPointerB = 0; // MP - used in 16-bit mode
        /*
        16-bit address map in hexadecimal
        0000-3FFF: 16KB RAM
        4000-BFFF: 32KB of ROM space
        C000-FFFF: Free space
        */
    }
    // cpu is entirely uncommented have fun hehe :3
    public static class Cpu
    {
        public static void Exec(byte opcode, byte input1, byte input2, byte input3)
        {
            Array.Copy(Memory.CMem, 0, Memory.Ram, 0, 16384); // syncs ram with cmem
            Memory.Ram.CopyTo(Memory.CMem, 0); // copies ram to cmem
            Memory.Rom.CopyTo(Memory.CMem, 16383); // copies rom to cmem (even though the rom cannot be changed)
            // 16383-49151 is ROM, cannot be written to
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
                if (BitConverter.ToUInt16(new byte[2] {input3, input2}, 0) > 16383 ||  BitConverter.ToUInt16(new byte[2] {input3, input2}, 0) < 65537)
                {
                    Console.WriteLine("This area in memory is not writable!");
                    Hlt();
                }
                else
                {
                    status = MovReg2Mem(input1, input2, input3);
                }
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 5)
            {
                Int32 status = MovMem2Reg(input1, input2, input3);
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
            else if ((Int32)opcode == 16)
            {
                Int32 status = Mul(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 17)
            {
                Int32 status = Div(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 18)
            {
                Int32 status = Mod(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 19)
            {
                Int32 status = Jeq(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 20)
            {
                Int32 status = Jlt(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 21)
            {
                Int32 status = Jgt(input1, input2, input3);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 22)
            {
                Int32 status = Inb(input1, input2);
                if (status == 1)
                {
                    Hlt();
                }
            }
            else if ((Int32)opcode == 23)
            {
                Int32 status = Outb(input1, input2);
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
                else if ((Int32)input2 == 255)
                {
                    Memory.StackPointer = Memory.RegisterA;
                    return 0;
                }
                else if ((Int32)input2 == 250)
                {
                    Memory.MemPointerA = Memory.RegisterA;
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
                else if ((Int32)input2 == 255)
                {
                    Memory.StackPointer = Memory.RegisterB;
                    return 0;
                }
                else if ((Int32)input2 == 250)
                {
                    Memory.MemPointerA = Memory.RegisterB;
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
                else if ((Int32)input2 == 255)
                {
                    Memory.StackPointer = Memory.RegisterC;
                    return 0;
                }
                else if ((Int32)input2 == 250)
                {
                    Memory.MemPointerA = Memory.RegisterC;
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
                else if ((Int32)input2 == 255)
                {
                    Memory.StackPointer = Memory.RegisterD;
                    return 0;
                }
                else if ((Int32)input2 == 250)
                {
                    Memory.MemPointerA = Memory.RegisterD;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }
        static Int32 MovReg2Mem(byte input1, byte rampos, byte rampos16)
        {
            // 16383-49151 is ROM, cannot be written to
            if ((Int32)input1 == 251)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.CMem[ramposbutshorter] = Memory.RegisterA;
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.CMem[ramposbutshorter] = Memory.RegisterB;
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.CMem[ramposbutshorter] = Memory.RegisterC;
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.CMem[ramposbutshorter] = Memory.RegisterD;
                return 0;
            }
            else if ((Int32)input1 == 255)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.CMem[ramposbutshorter] = Memory.StackPointer;
                return 0;
            }
            return 0;
        }
        static Int32 MovMem2Reg(byte rampos, byte rampos16, byte input1)
        {
            if ((Int32)input1 == 251)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.RegisterA = Memory.CMem[ramposbutshorter];
                return 0;
            }
            else if ((Int32)input1 == 252)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.RegisterB = Memory.CMem[ramposbutshorter];
                return 0;
            }
            else if ((Int32)input1 == 253)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.RegisterC = Memory.CMem[ramposbutshorter];
                return 0;
            }
            else if ((Int32)input1 == 254)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.RegisterD = Memory.CMem[ramposbutshorter];
                return 0;
            }
            else if ((Int32)input1 == 255)
            {
                ushort ramposbutshorter = 0;
                if (rampos == 250 && rampos16 == 0)
                {
                    ramposbutshorter = (ushort)Memory.MemPointerA;
                }
                else
                {
                    if (Memory.Addr16Bit)
                    {
                        BitConverter.ToUInt16(new byte[2] {rampos16, rampos}, 0);
                    }
                    else
                    {
                        ramposbutshorter = (ushort)rampos;
                    }
                }
                Memory.StackPointer = Memory.CMem[ramposbutshorter];
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
            else if ((Int32)input1 == 255)
            {
                Int32 ramposbutinInt32thistime = (Int32)integer;
                Memory.StackPointer = integer;
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
            Console.WriteLine("\nCPU halted.");
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
                        Memory.ProgramCounter = (ushort)i;
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
                            Memory.ProgramCounter = (ushort)i;
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
                            Memory.ProgramCounter = (ushort)i;
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
            04 Unused
            05 Reset processor
            06 Unused
            07 Jump to specific instruction specified in Register A
            */
            if (input1 == 0)
            {
                if (Memory.BusA == 13)
                {
                    Console.WriteLine();
                }
                else if (Memory.BusA == 127)
                {
                    try
                    {
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.CursorLeft--;
                    }
                    catch
                    {
                        // nothing
                    }
                }
                else
                {
                    Console.Write((char)Memory.BusA);
                }
                return 0;
            }
            else if (input1 == 1)
            {
                if (Memory.BusB == 15)
                {
                    Console.WriteLine();
                }
                else if (Memory.BusB == 127)
                {
                    try
                    {
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.CursorLeft--;
                    }
                    catch
                    {
                        // nothing
                    }
                }
                else
                {
                    Console.Write((char)Memory.BusB);
                }
                return 0;
            }
            else if (input1 == 2)
            {
                Memory.BusA = Convert.ToByte(Console.ReadKey().KeyChar);
                Console.CursorLeft--;
                Console.Write(" ");
                Console.CursorLeft--;
                return 0;
            }
            else if (input1 == 3)
            {
                Memory.BusB = Convert.ToByte(Console.ReadKey().KeyChar);
                Console.CursorLeft--;
                Console.Write(" ");
                Console.CursorLeft--;
                return 0;
            }
            else if (input1 == 4)
            {
                Console.WriteLine("MSG 04 has been deprecated in the 16-bit addressing update. Please update your software.");
                return 0;
            }
            else if (input1 == 5)
            {
                Memory.ProgramCounter = 0;
                return 0;
            }
            else if (input1 == 6)
            {
                Console.WriteLine("MSG 06 has been deprecated in the 16-bit addressing update. Please update your software.");
                return 0;
            }
            else if (input1 == 7)
            {
                Memory.ProgramCounter = Memory.RegisterA;
                return 0;
            }
            else if (input1 == 8)
            {
                Memory.Addr16Bit = false;
                return 0;
            }
            else if (input1 == 9)
            {
                Memory.Addr16Bit = true;
                return 0;
            }
            else
            {
                Console.WriteLine("MSG ID invalid");
                return 1;
            }
        }
        static Int32 Mul(byte input1, byte input2, byte input3)
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
                Int32 math = (Int32)value1 * (Int32)value2;
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
                Int32 math = (Int32)value1 * (Int32)value2;
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
                Int32 math = (Int32)value1 * (Int32)value2;
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
                Int32 math = (Int32)value1 * (Int32)value2;
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
        static Int32 Div(byte input1, byte input2, byte input3)
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
                Int32 math = (Int32)value1 / (Int32)value2;
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
                Int32 math = (Int32)value1 / (Int32)value2;
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
                Int32 math = (Int32)value1 / (Int32)value2;
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
                Int32 math = (Int32)value1 / (Int32)value2;
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
        static Int32 Mod(byte input1, byte input2, byte input3)
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
                Int32 math = (Int32)value1 % (Int32)value2;
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
                Int32 math = (Int32)value1 % (Int32)value2;
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
                Int32 math = (Int32)value1 % (Int32)value2;
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
                Int32 math = (Int32)value1 % (Int32)value2;
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
        static int Jeq(byte input1, byte input2, byte input3)
        {
            if (input1 == input2)
            {
                for (int i = 0; i < Memory.Rom.Length; i += 4) 
                {
                    if ((int)Memory.Rom[i] == 10)
                    {
                        if ((int)Memory.Rom[i + 1] == input3)
                        {
                            Memory.ProgramCounter = (ushort)i;
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        static int Jlt(byte input1, byte input2, byte input3)
        {
            if (input1 < input2)
            {
                for (int i = 0; i < Memory.Rom.Length; i += 4) 
                {
                    if ((int)Memory.Rom[i] == 10)
                    {
                        if ((int)Memory.Rom[i + 1] == input3)
                        {
                            Memory.ProgramCounter = (ushort)i;
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        static int Jgt(byte input1, byte input2, byte input3)
        {
            if (input1 > input2)
            {
                for (int i = 0; i < Memory.Rom.Length; i += 4) 
                {
                    if ((int)Memory.Rom[i] == 10)
                    {
                        if ((int)Memory.Rom[i + 1] == input3)
                        {
                            Memory.ProgramCounter = (ushort)i;
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        static int Inb(byte input1, byte input2)
        {
            if (input2 == 0)
            {
                if (input1 == 251)
                {
                    Memory.RegisterA = Memory.BusA;
                }
                else if (input1 == 252)
                {
                    Memory.RegisterB = Memory.BusA;
                }
                else if (input1 == 253)
                {
                    Memory.RegisterC = Memory.BusA;
                }
                else if (input1 == 254)
                {
                    Memory.RegisterD = Memory.BusA;
                }
                else
                {
                    Console.WriteLine("Not a register!");
                    Hlt();
                }
            }
            else if (input2 == 1)
            {
                if (input1 == 251)
                {
                    Memory.RegisterA = Memory.BusB;
                }
                else if (input1 == 252)
                {
                    Memory.RegisterB = Memory.BusB;
                }
                else if (input1 == 253)
                {
                    Memory.RegisterC = Memory.BusB;
                }
                else if (input1 == 254)
                {
                    Memory.RegisterD = Memory.BusB;
                }
                else
                {
                    Console.WriteLine("Not a register!");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Invalid bus ID");
                return 1;
            }
            return 0;
        }
        static int Outb(byte input1, byte input2)
        {
            if (input2 == 0)
            {
                if (input1 == 251)
                {
                    Memory.BusA = Memory.RegisterA;
                }
                else if (input1 == 252)
                {
                    Memory.BusA = Memory.RegisterB;
                }
                else if (input1 == 253)
                {
                    Memory.BusA = Memory.RegisterC;
                }
                else if (input1 == 254)
                {
                    Memory.BusA = Memory.RegisterD;
                }
                else
                {
                    Console.WriteLine("Not a register!");
                    Hlt();
                }
            }
            else if (input2 == 1)
            {
                if (input1 == 251)
                {
                    Memory.BusB = Memory.RegisterA;
                }
                else if (input1 == 252)
                {
                    Memory.BusB = Memory.RegisterB;
                }
                else if (input1 == 253)
                {
                    Memory.BusB = Memory.RegisterC;
                }
                else if (input1 == 254)
                {
                    Memory.BusB = Memory.RegisterD;
                }
                else
                {
                    Console.WriteLine("Not a register!");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Invalid bus ID");
                return 1;
            }
            return 0;
        }
        /*
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
        */
    }
}