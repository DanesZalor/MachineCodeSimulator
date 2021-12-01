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
        [InlineData(100,101, (ALU.FLAG.OFF) )]
        [InlineData(102,99 , (ALU.FLAG.A) )]
        public void ALU_CMP_modifiesFlagsCorrectly(ushort A, ushort B, ALU.FLAG expected_flags){
            alu.CMP(A,B);
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }
        
        [Theory]
        [InlineData(0b1010, 0b0110, ALU.FLAG.A, 0b1100)]
        [InlineData(0b0011, 0b0011, ALU.FLAG.E | ALU.FLAG.Z, 0b0000)]
        [InlineData(0b0011, 0b1100, ALU.FLAG.OFF, 0b1111)]
        public void ALU_XOR_modifiesFlagsCorrectlyAndChangesA(ushort A, ushort B, ALU.FLAG expected_flags, ushort expected_A){
            alu.XOR(ref A, ref B);
            Assert.Equal(A, expected_A);
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }

        [Theory]
        [InlineData(0b11, 0b1111_1111_1111_1100, ALU.FLAG.OFF)]
        [InlineData(0b1111_1111_1111_1111, 0b0, ALU.FLAG.Z)]
        [InlineData(0b0,0b1111_1111_1111_1111, ALU.FLAG.OFF)]
        public void ALU_NOT_flipsAllBitsCorrectly_andModifiesFlagZCorrectly(ushort A, ushort expected_A, ALU.FLAG expected_flags){
            alu.NOT(ref A);
            Assert.Equal(A,expected_A);
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }
        
    }
}
