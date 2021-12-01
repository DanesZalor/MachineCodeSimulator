
namespace CPU{
    public class ALU {
        [Flags]
        public enum FLAG : ushort{
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

        public bool evaluateFlags(FLAG mask){
            return (((FLAG)flags) & mask)>0;
        }

        public void printFlags(){
            Console.WriteLine((FLAG)flags);
        }
        
        /// <summary> compares A and B. Affected flags [A,E,Z] </summary> 
        public void CMP(ushort A, ushort B){
            setFlagsIf(A>B, FLAG.A);
            setFlagsIf(A==B, FLAG.E | FLAG.Z);
        }

        /// <summary same as CMP(A,B) but also does A xor B and put the result in A </summary>
        public void XOR(ref ushort A, ref ushort B){
            CMP(A,B);
            A = (ushort)(A ^ B);
        }

        /// <summary> Flips all the bits of A. Affected flags [Z] </summary>
        public void NOT(ref ushort A){
            A = (ushort)(~A);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into conjunction of A and B. Affected flags [Z] </summary>
        public void AND(ref ushort A, ref ushort B){
            A = (ushort)(A & B);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into disjunction of A and B. Affected flags [Z] </summary>
        public void OR(ref ushort A, ref ushort B){
            A = (ushort)(A | B);
            setFlagsIf(A==0, FLAG.Z);
        }

        /// <summary> changes A into Left shift A by B times. Affected flags [C] </summary>
        public void SHL(ref ushort A, ref ushort B){
            // CARRY ON if A<<B which is an INT32 is greater than the ushort.MaxValue
            int res = A<<B;
            setFlagsIf((res) > ushort.MaxValue, FLAG.C);
            A = (ushort)res;
        }

        /// <summary> changes A into Right shift A by B times. Affected flags [C] </summary>
        public void SHR(ref ushort A, ref ushort B){
            // CARRY ON if A>>B is not equal to A/2^B
            int res = A>>B;
            setFlagsIf(res != A/(Math.Pow(2,B)), FLAG.C);
            A = (ushort)res;
        }

        /// <summary> changes A into A / B. Affected flags [Z] </summary>
        public void DIV(ref ushort A, ref ushort B){
            if(B==0) return; // division by zero error
            
            A = (ushort)(A/B);
            setFlagsIf(A==0, FLAG.Z);
        }
        /// <summary> changes A into A * B. Affected flags [C,Z] </summary>
        public void MUL(ref ushort A, ref ushort B){
            int res = A*B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res>ushort.MaxValue, FLAG.C);
            A = (ushort)res;
        }
        /// <summary> changes A into A - B. Affected flags [C,Z] </summary>
        public void SUB(ref ushort A, ref ushort B){
            int res = A-B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res<0, FLAG.C);
            A = (ushort)res;
        }
        /// <summary> changes A into A + B. Affected flags [C,Z] </summary>
        public void ADD(ref ushort A, ref ushort B){
            int res = A+B;
            setFlagsIf(res==0, FLAG.Z);
            setFlagsIf(res>ushort.MaxValue, FLAG.C);
            A = (ushort)res;
        }

    }
}