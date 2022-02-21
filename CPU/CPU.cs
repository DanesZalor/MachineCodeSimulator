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
            IR.value = ram.read(IAR.value); // Set Instruction Register 
            byte incrementInstruction = 1;
            {// DO Instruction

                if(IR.value <= 0b111){ // MOV R,R // [0000_0AAA,0000_0BBB]

                    // RA.value = RB.value
                    GP[IR.value].value = GP[ ram.read(IAR.value+1) ].value;
                    incrementInstruction = 2;
                }
                else if(IR.value <= 0b1111){ // MOV R,C // [0000_1AAA, <8:Const>]

                    // RA.value = Const
                    GP[
                        (byte)(IR.value & 0b111) // AAA
                    ].value = ram.read(IAR.value+1);
                    incrementInstruction = 2;
                }
                else if(IR.value <= 0b1_0111){ // MOV R,[RO] // [0001_0AAA <5:Offset>BBB]

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
                    incrementInstruction = 2;
                }
                else if(IR.value <= 0b1_1111){ //  MOV R,[C] // [0001_1AAA <8:Const>]
                    
                    byte i2 = ram.read(IAR.value+1);
                    
                    GP[
                        (byte)(IR.value & 0b111) // AAA
                    ].value = ram.read(i2);
                    incrementInstruction = 2;
                }
                else if(IR.value <= 0b10_0111){ //  MOV [RO],R // [0010_0BBB <5:Offset>AAA]
                    
                    byte i2 = ram.read(IAR.value+1);

                    // get <5:offset>
                    byte offsetCode = (byte)((i2 & 0b0111_1000) >> 3);
                    sbyte offset = (sbyte)( ((i2&0b1000_0000)>0)?( (offsetCode+1)*-1 ):(offsetCode));

                    // RAM[ RA.value + offset ] = RB.value
                    ram.write(
                        (byte)( GP[ i2 & 0b111 ].value + offset ), // AAA + Offset
                        GP[IR.value & 0b111].value
                    );
                    incrementInstruction = 2;
                }
                else if(IR.value <= 0b10_1111){ //  MOV [C],R // [0010_1AAA <8:Const>]

                    ram.write(
                        ram.read(IAR.value+1), // Const
                        GP[ IR.value & 0b111 ].value // AAA
                    );
                    incrementInstruction = 2;
                }
            }
            
            IAR.value += incrementInstruction;     // Increment Instruction Address Register
        }


        /*************   FOR TESTING ONLY   **************/
        // REQUIRED for automated testing. DO NOT REMOVE
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

        public byte[] getRAMState(){
            return ram.getState();
        }
        public Dictionary<string,byte> getState(){
            Dictionary<string,byte> r = new Dictionary<string,byte>();
            
            string[] gpr = {"ra", "rb", "rc", "rd", "re", "rf", "rg", "sp"};
            for(int i = 0; i<8; i++)
                r.Add(gpr[i],GP[i].value);
            
            r.Add("ir", IR.value);
            r.Add("iar", IAR.value);

            return r;
        }
    }
}

