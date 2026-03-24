# md8emu
An emulator for a processor architecture I made up

This is a FOSS implementation of MingleDingle-8, a CPU architecture that is fully turing complete.

Here is the instruction set:

- 00 NOP (no operation)
- 01 ADD R1,R2,R3 (add two registers together to output register)
- 02 SUB R1,R2,R3 (subtract two registers to output register)
- 03 MOV R1,R2 (copy content of register to another register)
- 04 MOV R1,MEM (copy content of register to point in memory)
- 05 MOV MEM,R1 (load content of byte specified into register)
- 06 MOV INT,R1 (move integer into register)
- 07 PUSH R1 (pushes content of register into "stack")
- 08 POP R1 (pulls top of stack into register)
- 09 HLT (halts)
- 10 LBL ID (function)
- 11 JMP ID (jumps to lbl)
- 12 JZ ID (jumps to lbl if zeroflag is zero)
- 13 JNZ ID (jumps to lbl if zeroflag is nonzero)
- 14 SWB INT (switches bank from RAM/0, Bus A/1, Bus B/2, or ROM/3-6)
- 15 MSG ID (interrupt-type thing but not really)

made over an extended period of time
