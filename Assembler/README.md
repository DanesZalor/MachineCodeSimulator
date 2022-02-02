# Assembler
the class-lib for the Assembler component of **MachineCodeSimulator** project. The Assembly Language Compilation process consists of 4 phases:

## Instruction Set
#### Conditional Jump flags
standard `JCAZ <arguement>` is allowed where you can have `JC`, `JA`, `JZ`, `JCZ` or any mix of both. But for easier usability, we provide aliases as well that basically has a `JC?A?Z?` derivation.

| flags | meaning |
|-|-|
| C | Carry |
| A | A>B |
| Z | ZERO | 

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
#### ALU operations
some Arithmetic & Logic operations can operate with 1 arguement, 2 arguements, or both.

|  | Syntax | Operations Available |
|-|-|-|
| **Nomadic Operation** | `OP <reg>` | not,shl,shr,inc,dec |  
| **Dyadic Operation**  | `OP <reg>, <any>` |  all except inc, dec, and not |

for `SHL` and `SHR`, when used in a nomadic ALU instruction, is basically shifted by 1 position.

## 1. Initial phase
    mov b, 0    ; this is a comment
    mov d, 10

    label1:     inc B
                cmp b,d
                jc label1   ;jump if b<d

a sample Assembly code.
    
## 2. Evaluation     
The initial file undergoes syntax checking. For that, a special Lexicon is derived out of the file. The **Base Lexicon** contains the grammar for Registers, numeric constants, address, etc. To take the custom made labels in mind, a **New Lexicon** is derived. This lexicon considers existing labels to be a constant. 
#### Evaluation Sub-phases
##### 1. Scanning Labels Phase
During this step, the file is scanned line-by-line for label declarations, to dynamically create a RegularExpression for the NEW_LEXICON._EXISTING_LABELS_ grammar. After this phase, the **New Lexicon** is completely derived.
##### 2. Grammar Evaluation Phase
The file is once again scanned line-by-line but this time, for the grammar of each instruction. The grammar basis is from the derived **New Lexicon** from the previous phase. However, in a line of `label1: inc b ;comment`, the label declaration and the comments are completely ignored. Besides checking whether the line is grammatically correct or not, a detailed feedback is also provided. For example:
> `mov b, [257]`  "'257' not an 8-bit constant" <br>
> `mov [2], 22`  "invalid mov operands"

the way a line is checked is, first it checks if it matches the grammar of the *NEW_LEXICON*, then if it does, it means its in correct grammar. But if it doesn't, it checks if the line matches the grammar of the *VAGUE_LEXICON*, if it does, scan each arguement for errors. but if it doesn't, its an invalid statement.

## 3. Preprocessor Directives
*During this phase, it is assumed that the code is already syntactically correct*

The assembly code will be reduced to smaller bits for optimization
1. The comments will be removed
2. the JXXX instructions are translated to their corresponding alias derivations 

3. Some instructions can be optimized and replaced with an instruction that takes up less memory upon compilation.

    | Before | After |
    |--|--|
    | `ADD b, 1` | `INC b` |
    | `SUB b, 1` | `DEC b` |
    | `SHL b, 1` | `SHL b` |
    | `SHR b, 1` | `SHR b` |
    | `MUL b, 1` | ` ` |
    | `DIV b, 1` | ` ` |
    | `ADD b, 0` | ` ` |
    | `SUB b, 0` | ` ` | 
    | `MOV Rx, Rx` (same registers) | ` ` |

4. There are also aliases for constants. Binary constants in the form of `0b[0-1]{1,8}` and Hexademicals in the form of `0x([0-9]|[a-f]){2}`. These can be translated into Decimals for easier compilation.
5. The labels will be replaced with their corresponding constants
6. The spacing will be replaced by a uniform spacing for easier compilation
 

Example: From 
```
start:  mov a, 0x0a
        mov b, 0b1
        db 0x22
        db 0b1001
        db 25

loopstart:  cmp a, b
            jnbe loopend    ;end loop if a<=b
            sub a, 1
            jmp loopstart
loopend:    hlt
```
To
```
mov a,10
mov b,1
db 34
db 9
db 25
cmp a,b
ja 15
dec a
jmp 7
hlt
```

## 4. Compilation
*During this phase, it is assumed that the code has all aliases and preprocessor directives replaced and all spacing is uniform.*

Lastly, the actual translation to machine code.
![Instruction Set](Assembler/InstructionSet.png?raw=true "Title")