using System;

namespace CPU{
    public class RAM {
        private byte[] content;
        
        public RAM(ushort size){
            content = new byte[size];
        }

        /// <summary> for manual testing purposes only. to be removed in production </summary> ///
        public void print(){
            for(int i = 0; i<content.Length; i++) Console.Write( Convert.ToString(content[i]) + " " );
        }

        public int getSize(){
            return content.Length;
        }

        /// <summary> Get a single cell of the RAM. Might be deprecated </summary>
        public byte read1(ushort address){
            return content[address];
        }

        /// <summary> get 2 consecutive cells of the RAM and return as a ushort </summary>
        public ushort read(ushort address){
            return (ushort)((content[address]<<8) + (content[address+1]));
        }

        /// <summary> writes a ushort to two consecutive cells </summary>
        public void write(ushort address, ushort data){
            content[address] = (byte)(data>>8);
            content[address+1] = (byte)data;
        }
    }
}
