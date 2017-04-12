using System;
using System.IO;
using System.Linq;

namespace Torrent.Client.Bencoding
{
    /// <summary>
    ///     Represents a bencoded node type.
    /// </summary>
    internal enum BencodedNodeType
    {
        String,
        Integer,
        List,
        Dictionary
    }

    /// <summary>
    ///     Parses bencoded data.
    /// </summary>
    public static class BencodingParser
    {
        private static BinaryReader _reader;
        private static Stream _stream;

        /// <summary>
        ///     Parses the bencoded string into a tree of bencoded elements.
        /// </summary>
        /// <remarks>If byte data is contained the the string, refrain from using this method and use Decode(byte[]) instead.</remarks>
        /// <param name="bencoded">The bencoded string to parse.</param>
        /// <returns></returns>
        public static IBencodedElement Decode(string bencoded)
        {
            return Decode(bencoded.Select(c => (byte) c).ToArray());
        }

        /// <summary>
        ///     Parses the bencoded bytestring into a tree of bencoded elements.
        /// </summary>
        /// <param name="bencoded">The bencoded bytestring to parse.</param>
        /// <returns>The <c>IBencodedElement</c> representing the top node of the returned tree.</returns>
        /// <exception cref="BencodingParserException"></exception>
        public static IBencodedElement Decode(byte[] bencoded)
        {
            try
            {
                using (_stream = new MemoryStream(bencoded))
                using (_reader = new BinaryReader(_stream))
                {
                    return ParseElement();
                }
            }
            catch (Exception e)
            {
                throw new BencodingParserException("Unable to parse stream.", e);
            }
        }

        private static IBencodedElement ParseElement()
        {
            switch (CurrentNodeType())
            {
                case BencodedNodeType.Integer:
                    return ParseInteger();
                case BencodedNodeType.String:
                    return ParseString();
                case BencodedNodeType.List:
                    return ParseList();
                case BencodedNodeType.Dictionary:
                    return ParseDictionary();
                default:
                    throw new BencodingParserException("Unrecognized node type.");
            }
        }

        private static BencodedDictionary ParseDictionary()
        {
            var endChar = 'e';
            var beginChar = 'd';
            var list = new BencodedDictionary();
            if (_reader.PeekChar() != beginChar) throw new BencodingParserException("Expected dictionary.");

            _reader.Read();
            while ((char) _reader.PeekChar() != endChar)
            {
                string key = ParseElement() as BencodedString;
                if (key == null) throw new BencodingParserException("Key is expected to be a string.");
                list.Add(key, ParseElement());
            }
            _reader.Read();
            return list;
        }

        private static BencodedList ParseList()
        {
            var endChar = 'e';
            var beginChar = 'l';
            var list = new BencodedList();
            if (_reader.PeekChar() != beginChar) throw new BencodingParserException("Expected list.");

            _reader.Read();
            while ((char) _reader.PeekChar() != endChar)
                list.Add(ParseElement());
            _reader.Read();
            return list;
        }

        private static BencodedString ParseString()
        {
            var lenEndChar = ':';
            if (!char.IsDigit((char) _reader.PeekChar()))
                throw new BencodingParserException("Expected to read string length.");
            var length = ReadIntegerValue(lenEndChar);
            if (length < 0) string.Format("String can not have a negative length of {0}.", length);
            int len;
            var byteResult = new byte[length];
            if ((len = _reader.Read(byteResult, 0, (int) length)) != length)
                throw new BencodingParserException(
                    string.Format("Did not read the expected amount of {0} bytes, {1} instead.", length, len));
            return new BencodedString(new string(byteResult.Select(b => (char) b).ToArray()));
        }

        private static BencodedInteger ParseInteger()
        {
            var endChar = 'e';
            var beginChar = 'i';
            if (_reader.PeekChar() != beginChar) throw new BencodingParserException("Expected integer.");
            _reader.Read();
            var result = ReadIntegerValue(endChar);
            return result;
        }

        private static long ReadIntegerValue(char endChar)
        {
            char c;
            long result = 0;
            var negative = 1;
            if ((char) _reader.PeekChar() == '-')
            {
                _reader.Read();
                negative = -1;
            }
            while ((c = (char) _reader.Read()) != endChar)
            {
                if (!char.IsDigit(c))
                    throw new BencodingParserException(string.Format("Expected a digit, got '{0}'.", c));
                result *= 10;
                result += (long) char.GetNumericValue(c);
            }
            return result * negative;
        }

        private static BencodedNodeType CurrentNodeType()
        {
            char c;
            switch (c = (char) _reader.PeekChar())
            {
                case 'l':
                    return BencodedNodeType.List;
                case 'd':
                    return BencodedNodeType.Dictionary;
                case 'i':
                    return BencodedNodeType.Integer;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return BencodedNodeType.String;
                default:
                    throw new BencodingParserException(string.Format("Node type not recognized: '{0}'.", c));
            }
        }
    }
}