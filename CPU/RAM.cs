using System;

namespace CPU{
    public class RAM {
        private byte[] content;
        
        public RAM(byte size = 255){
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
        public byte read(byte address){
            return content[address];
        }

        /// <summary> writes a byte to two consecutive cells </summary>
        public void write(byte address, byte data){
            content[address] = (byte)(data);
        }
    }
}
