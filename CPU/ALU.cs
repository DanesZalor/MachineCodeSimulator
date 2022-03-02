
namespace CPU
{
    public class ALU
    {
        [Flags]
        public enum FLAG : byte
        {
            NONE = 0b1111_1000,
            OFF = 0b0000_0000,
            ON = 0b1111_1111,
            C = 0b100,
            A = 0b010,
            Z = 0b001,
        };

        public enum OP_CODE : byte
        {

        }

        private FLAG flags = 0b0000;
        private void setFlagsIf(bool cond, FLAG mask)
        {
            flags = ((cond) ?
                (flags | mask) :  // set to true
                (flags & ~mask) // set to false
            );
        }

        /// <summary> evaluate the current flags if one of the mask matches the current flags </summary>
        /// <param name="mask"> the mask to be compared to the current flag bits </param>
        /// <param name="exact"> set to true to match ALL bits. Only used for testing (I think) </param>
        /// <returns> boolean true if the mask bits match one of the flags. if exact=true, returns true if all bits match </returns>
        public bool evaluateFlags(FLAG mask, bool exact = false)
        {
            if (exact) return flags == mask;
            else return (((FLAG)flags) & mask) > 0;
        }

        /// <summary> sets flags to turn all bits off </summary>
        public void clearFlags()
        {
            flags = FLAG.OFF;
        }

        /// <remarks> debugging purposes only </remarks>
        public FLAG getFlags()
        {
            return flags;
        }
        /// <remarks> debugging purposes only </remarks>
        public void setFlags(FLAG f)
        {
            flags = f;
        }

        /// <summary> compares A and B. Affected flags [A,Z] </summary> 
        public void CMP(byte A, byte B)
        {
            setFlagsIf(A > B, FLAG.A);
            setFlagsIf(A == B, FLAG.Z);
            setFlagsIf(A < B, FLAG.C);
        }

        /// <summary same as CMP(A,B) but also does A xor B and put the result in A </summary>
        public void XOR(ref byte A, byte B)
        {
            CMP(A, B);
            A = (byte)(A ^ B);
        }

        /// <summary> Flips all the bits of A. Affected flags [Z] </summary>
        public void NOT(ref byte A)
        {
            A = (byte)(~A);
            setFlagsIf(A == 0, FLAG.Z);
        }

        /// <summary> changes A into conjunction of A and B. Affected flags [Z] </summary>
        public void AND(ref byte A, byte B)
        {
            A = (byte)(A & B);
            setFlagsIf(A == 0, FLAG.Z);
        }

        /// <summary> changes A into disjunction of A and B. Affected flags [Z] </summary>
        public void OR(ref byte A, byte B)
        {
            A = (byte)(A | B);
            setFlagsIf(A == 0, FLAG.Z);
        }

        /// <summary> changes A into Left shift A by B times. Affected flags [C] </summary>
        public void SHL(ref byte A, byte B)
        {
            // CARRY ON if A<<B which is an INT32 is greater than the byte.MaxValue
            int res = A << B;
            setFlagsIf((res) > byte.MaxValue, FLAG.C);
            A = (byte)res;
            setFlagsIf(A == 0, FLAG.Z);
        }

        /// <summary> changes A into Right shift A by B times. Affected flags [C] </summary>
        public void SHR(ref byte A, byte B)
        {
            // CARRY ON if A>>B is not equal to A/2^B
            int res = A >> B;
            setFlagsIf(res != A / (Math.Pow(2, B)), FLAG.C);
            A = (byte)res;
            setFlagsIf(A == 0, FLAG.Z);
        }

        /// <summary> changes A into A / B. Affected flags [Z] </summary>
        public void DIV(ref byte A, byte B)
        {
            if (B == 0) return; // division by zero error

            A = (byte)(A / B);
            setFlagsIf(A == 0, FLAG.Z);
        }
        /// <summary> changes A into A * B. Affected flags [C,Z] </summary>
        public void MUL(ref byte A, byte B)
        {
            int res = A * B;
            setFlagsIf(res > byte.MaxValue, FLAG.C);
            A = (byte)res;
            setFlagsIf(A == 0, FLAG.Z);
        }
        /// <summary> changes A into A - B. Affected flags [C,Z] </summary>
        public void SUB(ref byte A, byte B)
        {
            int res = A - B;
            setFlagsIf(res < 0, FLAG.C);
            A = (byte)res;
            setFlagsIf(A == 0, FLAG.Z);
        }
        /// <summary> changes A into A + B. Affected flags [C,Z] </summary>
        public void ADD(ref byte A, byte B)
        {
            int res = A + B;
            setFlagsIf(res > byte.MaxValue, FLAG.C);
            A = (byte)res;
            setFlagsIf(A == 0, FLAG.Z);
        }

    }
}