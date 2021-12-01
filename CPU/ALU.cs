
namespace CPU{
    public class ALU {
        [Flags]
        public enum FLAG : byte{
            OFF = 0b0000_0000,
            ON =  0b1111_1111,
            C = 0b1000,
            A = 0b0100,
            E = 0b0010,
            Z = 0b0001,
        };

        private FLAG flags = 0b0000;
        private void setFlagsIf(bool cond, FLAG mask){
            flags = ((cond) ? 
                (flags | mask ) :  // set to true
                (flags & ~mask ) // set to false
            );
        }

        /// <summary> evaluate the current flags if one of the mask matches the current flags </summary>
        /// <param name="mask"> the mask to be compared to the current flag bits </param>
        /// <param name="exact"> set to true to match ALL bits. Only used for testing (I think) </param>
        /// <returns> boolean true if the mask bits match one of the flags. if exact=true, returns true if all bits match </returns>
        public bool evaluateFlags(FLAG mask, bool exact=false){
            if(exact) return flags == mask;
            else return (((FLAG)flags) & mask)>0;
        }
        
        /// <remarks> debugging purposes only </remarks>
        public FLAG getFlags(){
            return flags;
        }
        /// <remarks> debugging purposes only </remarks>
        public void setFlags(FLAG f){
            flags = f;
        }
        
        /// <summary> compares A and B. Affected flags [A,E,Z] </summary> 
        public void CMP(ushort A, ushort B){
            setFlagsIf(A>B, FLAG.A);
            setFlagsIf(A==B, FLAG.E | FLAG.Z);
        }

        /// <summary same as CMP(A,B) but also does A xor B and put the result in A </summary>
        public void XOR(ref ushort A, ushort B){
            CMP(A,B);
            A = (ushort)(A ^ B);
        }

        /// <summary> Flips all the bits of A. Affected flags [Z] </summary>
        public void NOT(ref ushort A){
            A = (ushort)(~A);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into conjunction of A and B. Affected flags [Z] </summary>
        public void AND(ref ushort A, ushort B){
            A = (ushort)(A & B);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into disjunction of A and B. Affected flags [Z] </summary>
        public void OR(ref ushort A, ushort B){
            A = (ushort)(A | B);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into Left shift A by B times. Affected flags [C] </summary>
        public void SHL(ref ushort A, ushort B){
            // CARRY ON if A<<B which is an INT32 is greater than the ushort.MaxValue
            int res = A<<B;
            setFlagsIf((res) > ushort.MaxValue, FLAG.C);
            A = (ushort)res;
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into Right shift A by B times. Affected flags [C] </summary>
        public void SHR(ref ushort A, ushort B){
            // CARRY ON if A>>B is not equal to A/2^B
            int res = A>>B;
            setFlagsIf(res != A/(Math.Pow(2,B)), FLAG.C);
            A = (ushort)res;
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into A / B. Affected flags [Z] </summary>
        public void DIV(ref ushort A, ushort B){
            if(B==0) return; // division by zero error
            
            A = (ushort)(A/B);
            setFlagsIf(A==0, FLAG.Z);
        }
        /// <summary> changes A into A * B. Affected flags [C,Z] </summary>
        public void MUL(ref ushort A, ushort B){
            int res = A*B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res>ushort.MaxValue, FLAG.C);
            A = (ushort)res;
        }
        /// <summary> changes A into A - B. Affected flags [C,Z] </summary>
        public void SUB(ref ushort A, ushort B){
            int res = A-B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res<0, FLAG.C);
            A = (ushort)res;
        }
        /// <summary> changes A into A + B. Affected flags [C,Z] </summary>
        public void ADD(ref ushort A, ushort B){
            int res = A+B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res>ushort.MaxValue, FLAG.C);
            A = (ushort)res;
        }

    }
}