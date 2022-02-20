using System;

namespace CPU{
    public class RAM {
        private byte[] content;
        
        public RAM(byte size = 255){
            content = new byte[size];
        }

        /// <summary> for manual testing purposes only. to be removed in production </summary> ///
        public string getState(){
            string r = "[";
            for(int i = 0; i<content.Length; i++) 
                r = string.Concat(r, 
                    Convert.ToString(content[i]) + (i<content.Length-1?", ":"") 
                ); 
            return string.Concat(r, "]");
        }

        public int getSize(){
            return content.Length;
        }

        /// <summary> Get a single cell of the RAM. Might be deprecated </summary>
        public byte read(byte address){
            return content[address];
        }

        /// <summary> Only exists for int overload so i dont have to typecast the arguement to byte every fucking time
        public byte read(int address){
            return read((byte)address);
        }

        /// <summary> writes a byte to two consecutive cells </summary>
        public void write(byte address, byte data){
            content[address] = (byte)(data);
        }
    }
}
