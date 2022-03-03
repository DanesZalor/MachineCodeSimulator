using Xunit;
using CPU;
using System;

namespace CPUTests
{
    public class ALUTests
    {
        ALU alu;

        public ALUTests()
        {
            alu = new ALU();
        }

        /// <summary> compares alu flags and expected flags. displays a detailed error message </summary>
        private void AssertFlags(ALU alu, ALU.FLAG expected_flags)
        {
            Assert.True(
                alu.evaluateFlags(expected_flags, true),
                "Expected: " + expected_flags + "\tRecieved: " + alu.getFlags()
            );
        }
        [Theory]
        [InlineData(ALU.FLAG.Z, ALU.FLAG.Z, false)]
        [InlineData(ALU.FLAG.Z | ALU.FLAG.A, ALU.FLAG.A | ALU.FLAG.Z, false)]
        [InlineData(ALU.FLAG.Z | ALU.FLAG.A, ALU.FLAG.A | ALU.FLAG.Z, true)]
        public void ALU_evaluateFlags_correctly(ALU.FLAG flags, ALU.FLAG mask, bool exact)
        {
            alu.setFlags(flags);
            Assert.True(
                alu.evaluateFlags(mask, exact),
                exact ?
                    ("Excpected to match " + alu.getFlags() + " but has " + mask) :
                    ("Expected ANY from " + alu.getFlags() + " but recieved " + mask)
            );
        }

        [Theory]
        [InlineData(200, 200, (ALU.FLAG.Z))]
        [InlineData(100, 101, (ALU.FLAG.C))]
        [InlineData(102, 99, (ALU.FLAG.A))]
        public void CMP_modifiesFlagsCorrectly(byte A, byte B, ALU.FLAG expected_flags)
        {
            alu.CMP(A, B);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b1010, 0b0110, 0b1100, ALU.FLAG.A)]
        [InlineData(0b0011, 0b0011, 0b0000, ALU.FLAG.Z)]
        [InlineData(0b0011, 0b1100, 0b1111, ALU.FLAG.C)]
        public void XOR_modifiesFlagsCorrectlyAndChangesA(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.XOR(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b11, 0b1111_1100)]
        [InlineData(0b1111_1111, 0b0, ALU.FLAG.Z)]
        [InlineData(0b0, 0b1111_1111)]
        public void NOT_flipsAllBitsCorrectly_andModifiesFlagZCorrectly(byte A, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.NOT(ref A);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b1110, 0b0111, 0b0110)]
        [InlineData(0b1100, 0b0011, 0b0, ALU.FLAG.Z)]
        [InlineData(0b1011, 0b1010, 0b1010)]
        public void AND_changesACorrectly_andOnlyAffectzZFlag(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.AND(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b0, 0b0111, 0b0111)]
        [InlineData(0b0, 0b00, 0b0, ALU.FLAG.Z)]
        public void OR_changesACorrectly_andOnlyAffectzZFlag(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.OR(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b0011, 2, 0b1100)]
        [InlineData(0b0011_0000, 3, 0b1000_0000, ALU.FLAG.C)]
        [InlineData(0b0011_0000, 4, 0b0, ALU.FLAG.C | ALU.FLAG.Z)]
        public void SHL_shiftsCorrectly_andAffectsFlagCZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.SHL(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(0b1100, 2, 0b11)]
        [InlineData(0b0, 1, 0b0, ALU.FLAG.Z)]
        [InlineData(0b0100_1100, 3, 0b0000_1001, ALU.FLAG.C)]
        [InlineData(0b0000_1001, 4, 0b0, ALU.FLAG.C | ALU.FLAG.Z)]
        public void SHR_shiftsCorrectly_andAffectsFlagCZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.SHR(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(150, 5, 30)]
        [InlineData(255, 5, 51)]
        [InlineData(0, 242, 0, ALU.FLAG.Z)]
        [InlineData(2, 0, 2)] //if divided by 0, doesn't change anything
        public void DIV_changesAcorrectly_andAffectsFlagZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.DIV(ref A, B);
            Assert.Equal(expected_A, A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(12, 12, 144)]
        [InlineData(150, 2, 44, ALU.FLAG.C)] // 150 * 2 = 300 >> 45 because of byte conversion
        [InlineData(0, 2, 0, ALU.FLAG.Z)]
        [InlineData(2, 0, 0, ALU.FLAG.Z)]
        public void MUL_changesAcorrectly_andAffectsFlagCZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.MUL(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(255, 55, 200)]
        [InlineData(55, 55, 0, ALU.FLAG.Z)]
        [InlineData(55, 56, 255, ALU.FLAG.C)] // -1 turns into 65535
        public void SUB_changesAcorrectly_andAffectsFlagCZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.SUB(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Theory]
        [InlineData(255, 45, 44, ALU.FLAG.C)]
        [InlineData(255, 1, 0, ALU.FLAG.C | ALU.FLAG.Z)]
        [InlineData(255, 3, 2, ALU.FLAG.C)]
        [InlineData(0, 0, 0, ALU.FLAG.Z)]
        public void ADD_changesAcorrectly_andAffectsFlagCZ(byte A, byte B, byte expected_A, ALU.FLAG expected_flags = ALU.FLAG.OFF)
        {
            alu.ADD(ref A, B);
            Assert.Equal(A, expected_A);
            AssertFlags(alu, expected_flags);
        }

        [Fact]
        public void ALU_appendsFlagsCorrectly()
        {
            alu.CMP(255, 254);
            byte A = 255, B = 1;
            alu.ADD(ref A, B);

            // expect flag to be A|C|Z because 
            //  CMP set A on
            //  ADD set C and Z on
            AssertFlags(alu, ALU.FLAG.A | ALU.FLAG.C | ALU.FLAG.Z);
        }
    }
}
