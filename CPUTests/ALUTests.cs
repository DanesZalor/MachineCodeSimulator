using Xunit;
using CPU;
using System;

namespace CPUTests{
    public class ALUTests{
        ALU alu;
        
        public ALUTests(){
            alu = new ALU();
        }

        /// <summary> compares alu flags and expected flags. displays a detailed error message </summary>
        private void AssertFlags(ALU alu, ALU.FLAG expected_flags ){
            Assert.True(
                alu.evaluateFlags(expected_flags,true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
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
            AssertFlags(alu,expected_flags);
        }
        
        [Theory]
        [InlineData(0b1010, 0b0110, 0b1100,ALU.FLAG.A)]
        [InlineData(0b0011, 0b0011, 0b0000,ALU.FLAG.E | ALU.FLAG.Z)]
        [InlineData(0b0011, 0b1100, 0b1111)]
        public void ALU_XOR_modifiesFlagsCorrectlyAndChangesA(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF){
            alu.XOR(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu,expected_flags);
        }

        [Theory]
        [InlineData(0b11, 0b1111_1111_1111_1100)]
        [InlineData(0b1111_1111_1111_1111, 0b0, ALU.FLAG.Z)]
        [InlineData(0b0,0b1111_1111_1111_1111)]
        public void ALU_NOT_flipsAllBitsCorrectly_andModifiesFlagZCorrectly(ushort A, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.NOT(ref A);
            Assert.Equal(A,expected_A);
            AssertFlags(alu, expected_flags);
        }
        
        [Theory]
        [InlineData(0b1110, 0b0111, 0b0110)]
        [InlineData(0b1100, 0b0011, 0b0, ALU.FLAG.Z)]
        [InlineData(0b1011, 0b1010, 0b1010)]
        public void ALU_AND_changesACorrectly_andOnlyAffectzZFlag(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.AND(ref A, B);
            Assert.Equal(A,expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b0, 0b0111, 0b0111)]
        [InlineData(0b0, 0b00, 0b0, ALU.FLAG.Z)]
        public void ALU_OR_changesACorrectly_andOnlyAffectzZFlag(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.OR(ref A, B);
            Assert.Equal(A,expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b0011, 2, 0b1100)]
        [InlineData(0b0011_0000_0000_0000, 3, 0b1000_0000_0000_0000, ALU.FLAG.C)]
        [InlineData(0b0011_0000_0000_0000, 4, 0b0000_0000_0000_0000, ALU.FLAG.C | ALU.FLAG.Z)]
        public void ALU_SHL_shiftsCorrectly_andAffectsFlagCZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.SHL(ref A,B);
            Assert.Equal(A,expected_A);
            AssertFlags(alu,expected_flags);
        }

        [Theory]
        [InlineData(0b1100, 2, 0b11)]
        [InlineData(0b0, 1, 0b0, ALU.FLAG.Z)]
        [InlineData(0b0100_1100, 3, 0b0000_1001, ALU.FLAG.C)]
        [InlineData(0b0000_1001, 4, 0b0, ALU.FLAG.C | ALU.FLAG.Z)]
        public void ALU_SHR_shiftsCorrectly_andAffectsFlagCZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.SHR(ref A, B);
            Assert.Equal(A,expected_A);
            AssertFlags(alu,expected_flags);
        }

        [Theory]
        [InlineData(300,4,75)]
        [InlineData(420,6,70)]
        [InlineData(0,17013,0, ALU.FLAG.Z)]
        [InlineData(2,0,2)] //if divided by 0, doesn't change anything
        public void ALU_DIV_changesAcorrectly_andAffectsFlagZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.DIV(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu,expected_flags);
        }

        [Theory]
        [InlineData(120,3,360)]
        [InlineData(33000,2,464, ALU.FLAG.C)] // 330000 * 2 = 660000 >> 464 because of ushort conversion
        [InlineData(0,2,0, ALU.FLAG.Z)]
        [InlineData(2,0,0, ALU.FLAG.Z)]
        public void ALU_MUL_changesAcorrectly_andAffectsFlagCZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.MUL(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu,expected_flags);
        }

        [Theory]
        [InlineData(255,55,200)]
        [InlineData(55,55,0, ALU.FLAG.Z )]
        [InlineData(55,56,65535, ALU.FLAG.C )] // -1 turns into 65535
        public void ALU_SUB_changesAcorrectly_andAffectsFlagCZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.SUB(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu,expected_flags);
        }
        
        [Theory]
        [InlineData(255,45,300)]
        [InlineData(65535,1,0, ALU.FLAG.C | ALU.FLAG.Z)]
        [InlineData(65535,3,2, ALU.FLAG.C)]
        [InlineData(0,0,0, ALU.FLAG.Z)]
        public void ALU_ADD_changesAcorrectly_andAffectsFlagCZ(ushort A, ushort B, ushort expected_A, ALU.FLAG expected_flags=ALU.FLAG.OFF){
            alu.ADD(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu,expected_flags);
        }
    }
}
