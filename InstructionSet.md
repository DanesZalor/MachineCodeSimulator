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
There is a table below showing the aliases and derivation of conditional jumps and also the conditional jump's meaning/condition. <ins>**Note that aliases will be derived during compilation**<ins>
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
Will add soon

## CALL
Will add soon

## ALU
some Arithmetic & Logic Unit (ALU) operations can operate with 1 arguement, 2 arguements, or both. ALU has two types of Ops, Nomadic and Dynamic Ops. These operations can <ins>**modify the C, A, Z flags.**<ins/>

<br> Note: It is possible to **trigger multiple flags** on a single ALU instruction <br/>
| Instruction | Meaning |
|--|--|
| `NOT`| **Not/False** <br> This op may trigger the Z flag, for example when you do the instruction NOT 1111, the result will be 0000 therefore triggering the Z  flag <br/>| 
| `INC`| **Increment** <br> This op may trigger the C and Z flags, for example INC 255, it would trigger the C value because it is over the 255 byte limit. Additionally it also would trigger the Z flag since the result of INC 255 is 0.   <br/> |
| `DEC`| **Decrement** <br> This op may trigger  the C and Z flags, for example DEC 0 would trigger the C flag and DEC 1 would trigger the Z flag since the result will be 0. <br/>|		
| `SHL`| **Shift Left (once)** <br> This op may trigger the C & Z flags if for example shift left once with the value 1 (1) will become 0.  <br/>|
| `SHR`| **Shift Right (once)** <br> This op may trigger the C & Z flags if for example shift right once with the binary value of 1 (1) will become 0.  <br/>|
|--|--|
| `CMP`| **Compare** <br> This op may trigger the C, A, and Z flags. For example, when A > B it will trigger the C flags, A < B it will trigger A flag, and A == B it will trigger the Z flag. <br/> |
| `XOR`| **XOR** <br> This op may trigger the Z flag for example when you XOR 6 and 6 result being a 0 and will trigger the flag. <br/> |
| `AND`| **AND** <br> This op may trigger the Z flag, for example XOR of the binary value of 1 AND 0 will result to 0 and will trigger the Z flag.<br/> |
| `OR`| **OR** <br> This op  may  trigger the Z flag, for example when the two binary values are 0 and using the OR instruction will result to 0 and triggering the Z flag.<br/>  |
| `SHL`| **Shift Left (can shift  more than once)** <br> This op may trigger the C & Z flag if for example shift left 4 times with the binary value 1001 (9) will become 0. <br/>|
| `SHR`| **Shift Right (can shift  more than once)** <br> This op may trigger the C & Z flag if for example shift right 4 the value 1010 (10) will become 0. <br/>|  
| `MUL`| **Multiply** <br> This op may trigger the C & Z flags, if for example when you multiply two values and it exceeds the byte limit (255) it would trigger the C flag. Furthermore, if it exceeds and has a product 0 or multiplied with a 0 it will trigger the Z flag. <br/>|
| `DIV`| **Divide** <br> This op may trigger the Z flag, it occurs when the first value or register has the value of 0 and therefore triggering the Z flag. <br/>  |	
| `ADD`| **Add** <br> This op may trigger the C and Z flags, for example ADD two values and it exceeds the byte limit therefore triggering the C flag. Additionally, if that result is 0 then it would also trigger the Z flag. <br/> |
| `SUB`| **Subtract** <br> This op may trigger the C and Z flag, for example when subtracting below 0 would trigger the C flag. Furthermore, if the result would be 0 it would trigger the Z Flag. <br/> |

examples:
```
MOV a, 1
INC a      ; <-- increments a = 1 to a = 2

MOV a, 2 
MOV b, 2 
XOR a,b    ; <-- XOR of a=2 & b=2 == 0 therefore Z flag is triggered
JZ 10      ; <-- will jump because XOR of a and b will trigger the Z flag
INC a      ; <-- ignored/skipped
HLT        ; <-- JZ jumps here
```

## ETC
Other instructions that are used but is on a different category of other instructions.
```
CLF - Clear flags, clears the C, A, and Z flags
RET - Exits a subroutines by popping the return address previously pushed by the CALL instruction.
HLT - Halts/ends the program
```