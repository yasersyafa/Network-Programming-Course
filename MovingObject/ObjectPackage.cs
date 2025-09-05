using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingObject
{
    [Serializable]
    class ObjectPackage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ObjectPackage(int x, int y)
        {
            X = x;
            Y = y;
        }

        public ObjectPackage(byte[] data)
        {
            X = BitConverter.ToInt32(data, 0);
            Y = BitConverter.ToInt32(data, 4);
        }

        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(X));
            buffer.AddRange(BitConverter.GetBytes(Y));

            return buffer.ToArray();
        }

    }
}
