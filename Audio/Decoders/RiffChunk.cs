using System;
using System.Text;

namespace BattleCity.Audio.Decoders
{
    /// <summary>
    /// Holds information about a RIFF file chunk
    /// </summary>
    public class RiffChunk
    {
        readonly int identifier;
        readonly int length;
        readonly long streamPosition;

        /// <summary>
        /// Creates a RiffChunk object
        /// </summary>
        public RiffChunk(int identifier, int length, long streamPosition)
        {
            this.identifier = identifier;
            this.length = length;
            this.streamPosition = streamPosition;
        }

        /// <summary>
        /// The chunk identifier
        /// </summary>
        public int Identifier
        {
            get
            {
                return identifier;
            }
        }

        /// <summary>
        /// The chunk identifier converted to a string
        /// </summary>
        public string IdentifierAsString
        {
            get
            {
                return Encoding.ASCII.GetString(BitConverter.GetBytes(identifier));
            }
        }

        /// <summary>
        /// The chunk length
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// The stream position this chunk is located at
        /// </summary>
        public long StreamPosition
        {
            get
            {
                return streamPosition;
            }
        }
    }
}
