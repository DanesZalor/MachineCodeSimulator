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

            byte doMOV(){
                // MOV R,R // [0000_0AAA,0000_0BBB] 
                if(IR.value <= 0b111){ 
                    
                    // RA.value = RB.value
                    GP[
                        IR.value // AAA
                    ].value = GP[ 
                        ram.read(IAR.value+1) // BBB 
                    ].value; 
                }
                // MOV R,C // [0000_1AAA, <8:Const>] 
                else if(IR.value <= 0b1111){
                    
                    // RA.value = Const
                    GP[ 
                        (byte)(IR.value & 0b111) // AAA
                    ].value = ram.read(IAR.value+1); // Const
                }
                // MOV R,[RO] // [0001_0AAA <5:Offset>BBB]
                else if(IR.value <= 0b1_0111){ 

                    // adjacent ram cell to the pointed cell (instruction)
                    byte i2 = ram.read(IAR.value+1);
                    
                    // get BBB
                    byte bbbCode = (byte)(i2 & 0b111);
                    
                    // get <5:offset>
                    byte offsetCode = (byte)((i2 & 0b0111_1000) >> 3);
                    sbyte offset = (sbyte)( ((i2&0b1000_0000)>0)?( (offsetCode+1)*-1 ):(offsetCode));
                    
                    // RA.value = RAM[ RB.value + Offset ]
                    GP[
                        (byte)(IR.value & 0b111) // AAA
                    ].value = ram.read(
                        GP[bbbCode].value // BBB 
                        + offset          // Offset
                    );
                }

                //  MOV R,[C] // [0001_1AAA <8:Const>]
                else if(IR.value <= 0b1_1111){
                    
                    GP[
                        (byte)(IR.value & 0b111) // AAA
                    ].value = ram.read(
                        ram.read(IAR.value+1) // Const
                    );
                }

                //  MOV [RO],R // [0010_0BBB <5:Offset>AAA]
                else if(IR.value <= 0b10_0111){ 
                    
                    byte i2 = ram.read(IAR.value+1);

                    // get <5:offset>
                    byte offsetCode = (byte)((i2 & 0b0111_1000) >> 3);
                    sbyte offset = (sbyte)( ((i2&0b1000_0000)>0)?( (offsetCode+1)*-1 ):(offsetCode));

                    // RAM[ RA.value + offset ] = RB.value
                    ram.write(
                        (byte)( GP[ i2 & 0b111 ].value + offset ), // AAA + Offset
                        GP[IR.value & 0b111].value
                    );
                }

                //  MOV [C],R // [0010_1AAA <8:Const>]
                else if(IR.value <= 0b10_1111){

                    ram.write(
                        ram.read(IAR.value+1), // Const
                        GP[ IR.value & 0b111 ].value // AAA
                    );
                }
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
            
            {// DO Instruction
                IR.value = ram.read(IAR.value); // Set Instruction Register 
                byte increment = 0;
                if(IR.value <= 0b10_1111) // MOVs instructions
                    increment = doMOV();
                else if(IR.value <= 0b11_1000) // JMP instructions
                    increment = doJMP();
                else if(IR.value <= 0b100_1111) // JCAZ instructions
                    increment = doJCAZ();
                
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

