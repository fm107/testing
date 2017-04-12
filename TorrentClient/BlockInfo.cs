namespace Torrent.Client
{
    public struct BlockInfo
    {
        public static BlockInfo Empty = new BlockInfo(0, 0, 0);

        public int Index { get; }
        public int Offset { get; }
        public int Length { get; }

        public BlockInfo(int index, int offset, int length) : this()
        {
            Index = index;
            Offset = offset;
            Length = length;
        }

        #region Equality members

        public bool Equals(BlockInfo other)
        {
            return Index == other.Index && Offset == other.Offset && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlockInfo && Equals((BlockInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Index;
                hashCode = (hashCode * 397) ^ Offset;
                hashCode = (hashCode * 397) ^ Length;
                return hashCode;
            }
        }

        public static bool operator ==(BlockInfo left, BlockInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockInfo left, BlockInfo right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}