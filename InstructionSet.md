# Instruction Set
## Syntax
The syntax is similar as most assemblers are using. Every instruction must be on their own line. Labels are optional and must either start with a letter or a dot (.) and end with a colon.
```
Label: instruction operands   

; Comment
```

examples:
```
starts:         ; starts is a label and it  will start here (top to bottom)
	MOV a, 1    ; <-- example of a comment
	MOV b, 2
compare:
	CMP a, b
	JBE addz
	hlt
```
## MOV instructions
Copies a value from source to destination (e.g. var b = a). The MOV instruction is the only one able to directly modify the memory.
```
MOV Reg, Const
MOV Reg, Reg
```
examples: 
```
MOV a,5    ; the value of a is 5 because of the constant [MOV Reg, Const]
MOV b,a    ; the value of b is the value of a, it just copies [Mov Reg, Reg]
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
examples:
```
Note: The offset value is set to -16 to +15
MOV b,[a+2] ; the value of b is the value of a with an offset of 2.
MOV b,[5]   ; the value of b is  the value of the 5th address of the RAM
```

## JUMP Instructions
Lets the instruction pointer do a "Jump / Go to" to a defined address or register. There are two types of jump, unconditional and conditional jump.

<br /> Unconditional jump - it allows the instruction pointer to jump with no restriction nor conditions <br />
```
JMP Reg
JMP Const
```
example of unconditional JMP
```
mov a,1  
mov b,2  
jmp 10   ; <-- Jumps to 10 which is HLT which ends the instruction
mov a,5  ; this is ignored because of JMP
mov b,3  ; this is ignored because of JMP
HLT      ; <-- jumps here
```
<br /> Conditional jump - it allows the instruction pointer to jump if it satisfies the specific condition(s), the condition is the CAZ. <br />
```
C = C is true if there is carry
A = A is true if A > B after CMP (compare) instruction
Z = Z is true if zero 

example:
mov a,1   
mov b,1   
cmp a,b   
jz 9     ; <-- conditional jump, if z flag is turned on it will jump to inc 
hlt      ; ....in this case it  turns on because compare 1 and 1 is 0
inc a    ; <-- increments a (a + 1)
jmp 4    ; <-- unconditional jump to jump back to cmp a,b
```
There is a table below showing the aliases and derivation of conditional jumps and also the conditional jump's meaning/condition. **Note that aliases will be derived during compilation**
|   Alias   |   Derivation   |  Meaning |
|--|--|--|
|   JNC | JAZ | `011` 
|   JNA | JCZ | `!(A>B)`→`(A<=B)`
|   JNZ | JCA |  `output!=0`
|   JE  | JZ  | `(A xor B)==0` → **(A==B)**|
|   JNE | JCA |  JE → JZ so...
|   JB  | JC    | `!((A>B) or (A==B))` → `(A<=B) and (A!=B)` → **(A<B)** |
|   JNB | JAZ | `!(A<B)`→`(A>=B)` |
|   JAE | JAZ   | `JA or JE which is JZ` → **(A>=B)** |
|   JNAE| JC  | JNAE → JB → **JC**
|   JBE | JCZ | `!JA or JE`→ `JCZ or JZ` → **JCZ**
|   JNBE| JA  | `!(A<=B)` → **A>B**

## PUSH AND POP


## CALL


## ALU
