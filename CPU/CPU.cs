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
        
        public CPU(){
            for(int i = 0; i<8; i++) GP[i] = new CPU.Register();
            IR = new CPU.Register();
            IAR = new CPU.Register();
            SP = new CPU.Register(255);

            alu = new ALU();
            ram = new RAM();
        }


        public void InstructionCycleTick(){
            IR.value = ram.read(IAR.value); // Set Instruction Register 
            {// DO Instruction
                if(IR.value<0b1000){ // MOV_R_R
                    byte ra = IR.value;
                    byte rb = ram.read(IAR.value+1);
                    GP[ra].value = GP[rb].value;
                }
            }
            IAR.value += 1;     // Increment Instruction Address Register
        }

        
    }
}

