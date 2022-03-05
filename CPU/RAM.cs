using System;

namespace CPU{
    public class RAM {
        private byte[] content;
        
        public RAM(short size = 256){
            content = new byte[size];
        }

        public int getSize(){
            return content.Length;
        }

        /// <summary> Get a single cell of the RAM </summary>
        public byte read(byte address){
            return content[address];
        }

        /// <summary> lazy overload for RAM.read(byte) </summary>
        public byte read(int address){
            return read((byte)address);
        }

        /// <summary> writes a byte to two consecutive cells </summary>
        public void write(byte address, byte data){
            content[address] = (byte)(data);
        }
    }
}
