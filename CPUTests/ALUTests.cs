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
        [InlineData( ALU.FLAG.Z | ALU.FLAG.E, ALU.FLAG.E, false )]
        [InlineData( ALU.FLAG.Z | ALU.FLAG.E, ALU.FLAG.E | ALU.FLAG.Z, false )]
        [InlineData( ALU.FLAG.Z | ALU.FLAG.E, ALU.FLAG.E | ALU.FLAG.Z, true )]
        public void ALU_evaluateFlags_correctly(ALU.FLAG flags, ALU.FLAG mask, bool exact){
            alu.setFlags(flags);
            Assert.True(
                alu.evaluateFlags(mask, exact),
                exact? 
                    ("Excpected to match " + alu.getFlags() + " but has "+ mask) :
                    ("Expected ANY from " + alu.getFlags() + " but recieved " + mask) 
            );
        }

        [Theory]
        [InlineData(200,200, (ALU.FLAG.E | ALU.FLAG.Z) )]
        [InlineData(100,101, (null) )]
        [InlineData(102,99 , (ALU.FLAG.A) )]
        public void ALU_CMP_modifiesFlagsCorrectly(ushort A, ushort B, ALU.FLAG expected_flags){
            alu.CMP(A,B);
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }
        
    }
}
