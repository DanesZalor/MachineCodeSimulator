namespace CPU{
    public class CPU{
        private class Register{ // for readability
            public byte value = 0;
            public Register(byte v = 0){
                value = v;
            }
        }
        private ALU alu;
        private RAM ram;
        private Register[] GP = new CPU.Register[8]; // general purpose registers
        private Register SP { // Stack Pointer (or GP[7])
            get => GP[7];
            set => GP[7] = value;
        }
        private Register IR; // Instruction Register
        private Register IAR; // Instruction Address Register
        
        
        public CPU(byte[] program){
            for(int i = 0; i<8; i++) GP[i] = new CPU.Register();
            SP.value = 255;
            IR = new CPU.Register();
            IAR = new CPU.Register();
            alu = new ALU();
            ram = new RAM();

            for(byte i=0; i<program.Length; i++) // load program into ram
                ram.write(i, program[i]);    
        }

        

        public void InstructionCycleTick(){

            byte getOffsetByteFromInstruction(int instructionAddress){
                byte instruction = ram.read(instructionAddress);
                byte offsetCode = (byte)((instruction & 0b0111_1000) >> 3);
                
                return (byte)(
                    GP[instruction & 0b111].value + 
                    ( ((instruction&0b1000_0000)>0)?( (offsetCode+1)*-1 ):(offsetCode) )
                );
            }

            byte doMOV(){

                // MOV R,R // [0000_0AAA,0000_0BBB] // RA.value = RB.value 
                if(IR.value <= 0b111)
                    GP[ IR.value ].value = GP[ ram.read(IAR.value+1) ].value; 
                
                // MOV R,C // [0000_1AAA, <8:Const>] // RA.value = Const 
                else if(IR.value <= 0b1111)
                    GP[ (byte)(IR.value & 0b111) ].value = ram.read(IAR.value+1); 
                
                // MOV R,[RO] // [0001_0AAA <5:Offset>BBB] // RA.value = RAM[ RB.value + Offset ]
                else if(IR.value <= 0b1_0111) 
                    GP[ (byte)(IR.value & 0b111) ].value = ram.read( getOffsetByteFromInstruction(IAR.value+1) );
                
                //  MOV R,[C] // [0001_1AAA <8:Const>] // RA.value = RAM[Const]
                else if(IR.value <= 0b1_1111)    
                    GP[ (byte)(IR.value & 0b111) ].value = ram.read( ram.read(IAR.value+1) );
                
                //  MOV [RO],R // [0010_0BBB <5:Offset>AAA] // RAM[ RA.value + offset ] = RB.value
                else if(IR.value <= 0b10_0111)

                    ram.write(
                        getOffsetByteFromInstruction(IAR.value+1), // AAA + Offset
                        GP[IR.value & 0b111].value
                    );
                
                //  MOV [C],R // [0010_1AAA <8:Const>]
                else if(IR.value <= 0b10_1111)

                    ram.write(
                        ram.read(IAR.value+1), // Const
                        GP[ IR.value & 0b111 ].value // AAA
                    );
                
                return 2;
            }

            byte doJMP(){

                // JMP R // [0011_0AAA]
                if(IR.value <= 0b11_0111)
                    IAR.value = GP[ IR.value & 0b111 ].value;
                
                // JMP C // [0011_1000 <8:Const>]
                else //if(IR.value == 0b11_1000) 
                    IAR.value = ram.read(IAR.value+1);

                return 0;
            }

            byte doJCAZ(){
                
                // evaluate IR (the jump flags)
                if(alu.evaluateFlags( (ALU.FLAG)(IR.value & 0b111) )){

                     if(IR.value <= 0b100_0111)
                        // next byte in ram will be read as the REG to jump towards
                        IAR.value = GP[ ram.read(IAR.value+1) & 0b111 ].value;

                    // evaluate IR (the jump flags)
                    else if(alu.evaluateFlags( (ALU.FLAG)(IR.value & 0b111) ))
                        // next byte in ram will be read as the CONST to jump towards
                        IAR.value = ram.read(IAR.value+1);

                    return 0;
                }
                else return 2;
            }
            
            byte doPUSH(){
                // PUSH R // [0101_0AAA]
                if(IR.value <= 0b101_0111){
                    ram.write( SP.value--, GP[IR.value & 0b111].value );
                    return 1;
                }
                
                // PUSH [RO] // [0101_1000 <5:Offset>AAA]
                else if(IR.value == 0b101_1000)
                    ram.write(
                        SP.value--,
                        ram.read(getOffsetByteFromInstruction(IAR.value+1))
                    );
                
                // PUSH [C] // [0101_1001 <8:Const>]
                else if(IR.value == 0b101_1001)
                    ram.write(
                        SP.value--,
                        ram.read(ram.read(IAR.value+1))
                    );
                
                // PUSH C // [0101_1010 <8:Const>]
                else if(IR.value == 0b101_1010)
                    ram.write(
                        SP.value--,
                        ram.read(IAR.value+1)
                    );

                return 2;
            }

            byte doPOP(){
                
                // POP R // [0110_0AAA]
                if(IR.value <= 0b110_0111){
                    GP[ IR.value & 0b111 ].value = ram.read(++SP.value);
                    return 1;
                }

                // POP [RO] // [0110_1000 <5:Offset>AAA]
                else if(IR.value <= 0b110_1000)
                    ram.write(
                        getOffsetByteFromInstruction(IAR.value+1),
                        ram.read(++SP.value)
                    );
                
                else if(IR.value <= 0b110_1001)
                    ram.write(
                        ram.read(IAR.value+1),
                        ram.read(++SP.value)
                    );

                return 2;
            }

            byte doCALL(){

                // CALL R // [0111_0AAA]
                if(IR.value <= 0b111_0111){

                    // PUSH the address of the next instruction to the stack
                    ram.write( SP.value-- , (byte)(IAR.value+1) ); 

                    IAR.value = GP[ IR.value & 0b111 ].value; // jump to R
                    return 0;
                }

                // CALL Const // [0111_1000 <8:Const>] 
                else{ //if(IR.value <= 0b111_1000)
                    
                    // PUSH the address of the next instruction to the stack
                    ram.write( SP.value-- , (byte)(IAR.value+2) ); 
                    
                    IAR.value = ram.read(IAR.value+1);
                    return 0;
                }
            }

            byte doALU1(){
                byte opcode = (byte)( (IR.value & 0b0011_1000)>>3 );
                byte reg = (byte)(IR.value & 0b111);

                switch(opcode){
                    case 0b000: alu.NOT(ref GP[reg].value); break;
                    case 0b001: alu.ADD(ref GP[reg].value, 1); break;
                    case 0b010: alu.SUB(ref GP[reg].value, 1); break;
                    case 0b011: alu.SHL(ref GP[reg].value, 1); break;
                    case 0b100: alu.SHR(ref GP[reg].value, 1); break;
                }
                return 1;
            }

            byte doALU2(){
                byte opcode = (byte)(IR.value & 0b1111);
                byte regA = (byte)((ram.read(IAR.value+1) & 0b0111_0000) >> 4);
                byte i2 = (byte)(ram.read(IAR.value+1) & 0b1111); // only get first 4 bits
                
                
                byte arg2 = 0; /* 2nd arguement*/{
                    if(i2 <= 0b0111) // regB
                        arg2 = GP[ i2 & 0b111 ].value; 
                    else if(i2 == 0b1000) // [regB+Offset]
                        arg2 = ram.read( getOffsetByteFromInstruction( IAR.value+2 ) );
                    else if(i2 == 0b1001) // [const]
                        arg2 = ram.read( ram.read(IAR.value+2) );
                    else if(i2 == 0b1010) // const
                        arg2 = ram.read(IAR.value+2);
                }

                //Console.WriteLine(String.Format("op:{0}  arg1:{1}  arg2:{2}",
                //            opcode, GP[regA].value,arg2));
                switch(opcode){
                    case 0b0000: alu.CMP( GP[regA].value, arg2 ); break;
                    case 0b0001: alu.XOR( ref GP[regA].value, arg2 ); break;
                    case 0b0010: alu.AND( ref GP[regA].value, arg2 ); break;
                    case 0b0011: alu.OR( ref GP[regA].value, arg2 ); break;
                    case 0b0100: alu.SHL( ref GP[regA].value, arg2 ); break;
                    case 0b0101: alu.SHR( ref GP[regA].value, arg2 ); break;
                    case 0b0110: alu.MUL( ref GP[regA].value, arg2 ); break;
                    case 0b0111: alu.DIV( ref GP[regA].value, arg2 ); break;
                    case 0b1000: alu.ADD( ref GP[regA].value, arg2 ); break;
                    case 0b1001: alu.SUB( ref GP[regA].value, arg2 ); break;
                }

                return (byte)((i2 <= 0b0111)?2:3);
            }

            byte doETC(){
                
                switch(IR.value){
                    case 0b1101_0000:   
                        alu.clearFlags();   
                        return 1;
                    case 0b1101_0001:
                        IAR.value = ram.read(++SP.value); // JUMP to the POP'd value
                        return 0;
                    case 0b1101_0010:
                        // HLT
                        break;
                }
                return 0;
            }

            {// DO Instruction
                IR.value = ram.read(IAR.value); // Set Instruction Register 
                byte increment = 0;
                if(IR.value <= 0b10_1111) // MOVs instructions
                    increment = doMOV();
                else if(IR.value <= 0b11_1000) // JMP instructions
                    increment = doJMP();
                else if(IR.value <= 0b100_1111) // JCAZ instructions
                    increment = doJCAZ();
                else if(IR.value <= 0b101_1010) // PUSH instructions
                    increment = doPUSH();
                else if(IR.value <= 0b0110_1001) // POP instructions
                    increment = doPOP();
                else if(IR.value <= 0b0111_1000) // CALL instructions
                    increment = doCALL();
                else if(IR.value <= 0b10100_111) // ALU nomadic instructions
                    increment = doALU1();
                else if(IR.value <= 0b1100_1001) // ALU dyadic instructions
                    increment = doALU2();
                else if(IR.value <= 0b1101_0010) // ETC instructions
                    increment = doETC();

                IAR.value += increment;
            }
        }


        /*************   FOR TESTING ONLY   **************/
        // REQUIRED for automated testing. DO NOT REMOVE
        // can remove this when putting it in the game.

        /// <summary> TESTING PURPOSE ONLY <br> DO NOT USE UNLESS FOR AUTOMATED TESTING </summary>
        public void printState(){
            
            string get_GP_State(){
                return String.Format(
                    "[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}]",
                    GP[0].value, GP[1].value, GP[2].value,
                    GP[3].value, GP[4].value, GP[5].value,
                    GP[6].value, GP[7].value
                );
            }

            string prt = String.Format(
                "GP = {0}\nIR = {1}\tIAR = {2}\tSP = {3}",
                get_GP_State(), IR.value, IAR.value, SP.value
            );
            Console.WriteLine(prt);
        }

        /// <summary> TESTING PURPOSE ONLY <br> DO NOT USE UNLESS FOR AUTOMATED TESTING </summary>
        public byte[] getRAMState(){
            return ram.getState();
        }

        /// <summary> TESTING PURPOSE ONLY <br> DO NOT USE UNLESS FOR AUTOMATED TESTING </summary>
        public Dictionary<string,byte> getState(){
            Dictionary<string,byte> r = new Dictionary<string,byte>();
            
            string[] gpr = {"ra", "rb", "rc", "rd", "re", "rf", "rg", "sp"};
            for(int i = 0; i<8; i++)
                r.Add(gpr[i],GP[i].value);
            
            r.Add("ir", IR.value);
            r.Add("iar", IAR.value);

            return r;
        }

        /// <summary> TESTING PURPOSE ONLY <br> DO NOT USE UNLESS FOR AUTOMATED TESTING </summary>
        public ALU.FLAG getALUFlags(){
            return alu.getFlags();
        }

        /// <summary> TESTING PURPOSE ONLY <br> DO NOT USE UNLESS FOR AUTOMATED TESTING 
        /// <br> All short arguements yes, but make sure to use byte values (0-255) </summary>
        public void setState(
            short ra=-1, short rb=-1, short rc=-1, short rd=-1,short re=-1, 
            short rf=-1, short rg=-1, short sp=-1, short ir=-1,short iar=-1, ALU.FLAG aluflags=ALU.FLAG.NONE){
            
            Register[] r = {GP[0], GP[1], GP[2], GP[3], GP[4], GP[5], GP[6], SP, IR, IAR};
            short[] args = {ra,rb,rc,rd,re,rf,rg,sp,ir,iar};

            for(int i=0; i< args.Length; i++){
                if(args[i]>=0 && args[i]<256)
                    r[i].value = (byte)args[i];
            }

            if(aluflags != ALU.FLAG.NONE )
                alu.setFlags(aluflags);
        }
    }
}

