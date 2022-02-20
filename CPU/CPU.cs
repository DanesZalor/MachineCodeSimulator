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
        private Register IR; // Instruction Register
        private Register IAR; // Instruction Address Register
        private Register SP; // Stack Pointer Register
        
        public CPU(byte[] program=null){
            for(int i = 0; i<8; i++) GP[i] = new CPU.Register();
            IR = new CPU.Register();
            IAR = new CPU.Register();
            SP = new CPU.Register(255);
            alu = new ALU();
            ram = new RAM();

            for(byte i=0; i<program.Length; i++) // load program into ram
                ram.write(i, program[i]);    
        }

        /// <summary> gets a string readable state of the CPU <br> 
        /// This is for debugging purposes only </summary>
        public string getState(){
            return String.Format(
                "GP = [{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} ]\n",
                GP[0],GP[1],GP[2],GP[3],GP[4],GP[5],GP[6],GP[7]
            );
        }

        public void InstructionCycleTick(){
            IR.value = ram.read(IAR.value); // Set Instruction Register 
            byte incrementInstruction = 1;
            {// DO Instruction
                
                if(IR.value<0b1000){ // MOV_R_R [0000_0AAA,0000_0BBB]

                    // RA.value = RB.value
                    GP[IR.value].value = GP[ ram.read(IAR.value+1) ].value;
                    incrementInstruction = 2;
                }
                else if(IR.value>0b111 && IR.value<0b1000){ // MOV_R_C [0000_1AAA, <8:Const>]

                    byte ra = (byte)(IR.value | 0b111);
                    // RA.value = Const
                    GP[ra].value = ram.read(IAR.value+1);
                    incrementInstruction = 2;
                }
            
            }
            IAR.value += incrementInstruction;     // Increment Instruction Address Register
        }

        
    }
}

