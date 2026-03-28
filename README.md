# md8emu
An emulator for a processor architecture I made up

This is a FOSS implementation of MingusDingus-8, a CPU architecture that is fully turing complete.

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
- 16 MUL R1,R2,R3 (multiplies two registers to output register)
- 17 DIV R1,R2,R3 (divides R1 by R2 and outputs quotient to R3)
- 18 MOD R1,R2,R3 (divides R1 by R2 and outputs remainder to R3)
- 19 JEQ R1,R2,ID (jumps to label if R1 equals R2)
- 20 JLT R1,R2,ID (jumps to label if R1 is less than R2)
- 21 JGT R1,R2,ID (jumps to label if R1 is greater than R2)
- 22 INB R1,ID (takes byte from Bus A (0) or Bus B (1) and copies to R1)
- 23 OUTB R1,ID  (takes R1 and copies it to Bus A (0) or Bus B (1))

Here are the implemented MSGs:

- 00 Print Bus A to console
- 01 Print Bus B to console
- 02 Accept key press from Bus A
- 03 Accept key press from Bus B
- 04 Unused
- 05 Reset processor
- 06 Unused
- 07 Jump to specific instruction specified in Register A
- 08 Switch to 8-bit addressing
- 09 Switch to 16-bit addressing

To-dos:
- [x] MOV to stack pointer
- [x] 16 bit addressing
- [x] Refactor MD8 assembly language to be more similar to Intel syntax
  - [x] Make RAM addresses in MD8 assembly hexadecimal instead of decimal
  - [x] Change syntax from "MOV,6,A,0" to "mov 6,a"
- [ ] Add memory pointer (MP) and ability to MOV to it
- [ ] Add integer operations on SP and MP
- [x] Update Bus A and B to act as single bytes that transfer 1 bit at a time similarly to actual serial
- [ ] Improve documentation for the assembly language

made over an extended period of time
