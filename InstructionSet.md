# Instruction Set
## Syntax
The syntax is similar as most assemblers are using. Every instruction must be on their own line. Labels are optional and must either start with a letter or a dot (.) and end with a colon.
```
Label: instruction operands   

; Comment
```

## MOV instructions
Copies a value from source to destination (e.g. var b = a). The MOV instruction is the only one able to directly modify the memory.
```
MOV Reg, Reg
MOV Reg, Const
```
Furthermore, MOV instruction could also store or load data from the register.
<br /> **Load** <br />
```
MOV Reg, [Reg + Offset]
MOV Reg, [Const]
```
**Store**
```
MOV [Reg + Offset], Reg
MOV [Const], Reg
```

## JUMP Instructions
Lets the instruction pointer do a "Jump / Go to" to a defined address or register. There are two types of jump, unconditional and conditional jump.

<br /> Unconditional jump - it allows the instruction pointer to jump with no restriction nor conditions <br />
```
JMP Reg
JMP Const
```
<br /> Conditional jump - it allows the instruction pointer to jump if it satisfies the specific condition(s), these conditions are  <br />
```
C
```