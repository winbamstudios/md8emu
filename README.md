# md8emu
An emulator for a processor architecture I made up

This is a FOSS implementation of MingleDingle-8, a CPU architecture that is fully turing complete.

Here is the instruction set:
- 0 NOP (no operation)
- 1 ADD R1,R2,R3 (add two registers together to output register)
- 2 SUB R1,R2,R3 (subtract two registers to output register)
- 3 MOV R1,R2 (copy content of register to another register)
- 4 MOV R1,RAM (copy content of register to poInt32 in memory)
- 5 MOV RAM,R1 (load content of byte specified into register)
- 6 MOV INT,R1 (move integer into register)
- 7 PUSH R1 (pushes content of register into "stack")
- 8 POP R1 (pulls top of stack into register)
- 9 HLT (halts)
- 10 Not Implemented
- 11 JMP ADDR (jumps to address) (optional)
- 12 JZ ADDR (jumps to address if Register D zero)
- 13 JNZ ADDR (jumps to address if Register D nonzero)

made in about 2 hours because i was bored
