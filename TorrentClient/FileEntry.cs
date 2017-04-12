namespace Torrent.Client
{
    /// <summary>
    ///     Represents a file exchanged on the BitTorrent protocol.
    /// </summary>
    public class FileEntry
    {
        /// <summary>
        ///     Initializes a new instance of the Torrent.Client.FileEntry class.
        /// </summary>
        /// <param name="name">The name of the file. (full path)</param>
        /// <param name="length">The length of the file in bytes.</param>
        public FileEntry(string name, long length)
        {
            Name = name;
            Length = length;
        }

        /// <summary>
        ///     The name of the file enter. (in BitTorrent, includes full path)
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The length of the file in bytes.
        /// </summary>
        public long Length { get; }

        /// <summary>
        ///     Returns a string that represents the content of the FileEntry object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} | {1}", Name, Length);
        }
    }
}