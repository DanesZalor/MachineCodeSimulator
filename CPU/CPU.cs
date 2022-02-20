namespace CPU{
    
    private class Register{
        public byte value;
    }


    public class CPU{
        private ALU alu;
        private RAM ram;
        private Register ra, rb ,rc, rd, re, rf, rg; // General Purpose Registers
        private Register IR; // Instruction Register
        private Register IAR; // Instruction Address Register
        private Register SP; // Stack Pointer Register
        
        public CPU(){
            alu = new ALU();
            ram = new RAM();
        }

        public void InstructionCycleTick(){
            IR.value = ram.read(IAR.value); // Set Instruction Register 
            {// DO Instruction
                
            }
            IAR.value += 1;     // Increment Instruction Address Register
        }

        
    }
}

