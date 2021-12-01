using Xunit;
using CPU;
using System;

namespace CPUTests{
    public class ALUTests{
        ALU alu;
        
        public ALUTests(){
            alu = new ALU();
        }

        [Theory]
        [InlineData(200,200, (ALU.FLAG.E | ALU.FLAG.Z) )]
        [InlineData(100,101, (null) )]
        [InlineData(102,99 , (ALU.FLAG.A) )]
        public void CMP_modifiesFlagsCorrectly(ushort A, ushort B, ALU.FLAG expected_flags){
            alu.CMP(A,B);
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }
        
    }
}
