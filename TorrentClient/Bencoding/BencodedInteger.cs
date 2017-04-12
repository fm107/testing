﻿namespace Torrent.Client.Bencoding
{
    /// <summary>
    ///     Provides a class for representing Bencoded integers.
    /// </summary>
    public class BencodedInteger : IBencodedElement
    {
        private readonly long _innerInteger;

        /// <summary>
        ///     Initializes a new instance of the Torrent.Client.Bencoding.BencodedInteger class via a long value.
        /// </summary>
        /// <param name="value">A long value containing the Bencoded data.</param>
        public BencodedInteger(long value)
        {
            _innerInteger = value;
        }

        #region IBencodedElement Members

        /// <summary>
        ///     Returns a Bencoded string that represents the content of the Bencoded integer.
        /// </summary>
        /// <returns></returns>
        public string ToBencodedString()
        {
            return string.Format("i{0}e", _innerInteger);
        }

        #endregion

        /// <summary>
        ///     Returns a string that represents the content of the Bencoded integer.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _innerInteger.ToString();
        }

        public static implicit operator long(BencodedInteger value)
        {
            return value._innerInteger;
        }

        public static implicit operator int(BencodedInteger value)
        {
            return (int) value._innerInteger;
        }

        public static implicit operator BencodedInteger(long value)
        {
            return new BencodedInteger(value);
        }

        public static implicit operator BencodedInteger(int value)
        {
            return new BencodedInteger(value);
        }
    }
}