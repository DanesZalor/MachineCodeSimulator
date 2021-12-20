# Assembler
the class-lib for the Assembler component of **MachineCodeSimulator** project. The Assembly Language Compilation process consists of 3 phases:
### 1. Initial phase
    mov b, 0    ; this is a comment
    mov d, 10

    label1:     inc B
                cmp b,d
                jc label1   ;jump if b<d
    
    
The initial file undergoes syntax checking. For that, a special Lexicon is derived out of the file. The **Base Lexicon** contains the grammar for Registers, numeric constants, address, etc. To take the custom made labels in mind, a **New Lexicon** is derived. This lexicon considers existing labels to be a constant. 
#### Evaluation Sub-phases
##### 1. Scanning Labels Phase
During this step, the file is scanned line-by-line for label declarations, to dynamically create a RegularExpression for the NEW_LEXICON._EXISTING_LABELS_ grammar. After this phase, the **New Lexicon** is completely derived.
##### 2. Grammar Evaluation Phase
The file is once again scanned line-by-line but this time, for the grammar of each instruction. The grammar basis is from the derived **New Lexicon** from the previous phase. However, in a line of `label1: inc b ;comment`, the label declaration and the comments are completely ignored. Besides checking whether the line is grammatically correct or not, a detailed feedback is also provided. For example:
> `mov b, [257]`  "'257' not an 8-bit constant" <br>
> `mov [2], 22`  "invalid mov operands"

the way a line is checked is, first it checks if it matches the grammar of the *NEW_LEXICON*, then if it does, it means its in correct grammar. But if it doesn't, it checks if the line matches the grammar of the *VAGUE_LEXICON*, if it does, scan each arguement for errors. but if it doesn't, its an invalid statement.

#### JumpIf flags
| flags | meaning |
|-|-|
| C | Carry |
| A | A>B |
| Z | ZERO | 

|   Alias   |   Derivation   |  Meaning |
|--|--|--|
|   JE  | JZ  | (A xor B)==0 |
|   JB  | JC    | `!((A>B) or (A==B))` → `(A<=B) and (A!=B)` → **(A<B)** |
|   JAE | JAZ   | `JA or JE which is JZ` → **(A>=B)** |
|   JBE | 



```
C - Carry flag
A - A>B flag
Z - Zero flag

Basic Instruction
JC, JA, JZ, 
JCA, JAZ, JCZ,
JCAZ

Alias               Derivation      Meaning
JE                  JZ              if A==B
JB                  JC              ~((A>B) || (A==B)) 
                                    --> ((A<=B) && (A!=B)) --> (A<B)
JAE                 JAZ             if (A>B) ||
JBE                 JCZ             if 

```