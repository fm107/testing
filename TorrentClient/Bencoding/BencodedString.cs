using System.Collections;
using System.Collections.Generic;

namespace Torrent.Client.Bencoding
{
    /// <summary>
    ///     Provides a class for representing Bencoded strings.
    /// </summary>
    public class BencodedString : IBencodedElement, IEnumerable<char>
    {
        private readonly string _innerString;

        /// <summary>
        ///     Initializes a new instance of the Torrent.Client.Bencoding.BencodedString class via a string.
        /// </summary>
        /// <param name="value">A string containing the Bencoded data.</param>
        public BencodedString(string value)
        {
            _innerString = value;
        }

        /// <summary>
        ///     Returns the length of the Bencoded string.
        /// </summary>
        public int Length
        {
            get { return _innerString.Length; }
        }

        #region IBencodedElement Members

        /// <summary>
        ///     Returns the Bencoded string in Bencoded format.
        /// </summary>
        /// <returns></returns>
        public string ToBencodedString()
        {
            return string.Format("{0}:{1}", _innerString.Length, _innerString);
        }

        #endregion

        /// <summary>
        ///     Returns the Bencoded string as a normal string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _innerString;
        }

        public static implicit operator string(BencodedString value)
        {
            return value._innerString;
        }

        public static implicit operator BencodedString(string value)
        {
            return new BencodedString(value);
        }

        #region IEnumerable<char> Members

        public IEnumerator<char> GetEnumerator()
        {
            foreach (var c in _innerString)
                yield return c;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var c in _innerString)
                yield return c;
        }

        #endregion
    }
}