using Dlid.MiHome.Exceptions;
using Dlid.MiHome.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dlid.MiHome.Tests
{
    [TestClass]
    public class MiHomeTokenTests
    {

        /// <summary>
        /// Make sure a token containing errous values throws an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MiTokenException), "Token must only contain HEX characters")]
        public void MiToken_NonHexValues_Exception()
        {
            var x = new MiDevice("127.0.0.1", "hello there");
        }

        /// <summary>
        /// Make sure an empty token throws an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MiTokenException), "No token was provided")]
        public void MiToken_EmptyToken_Exception()
        {
            var x = new MiDevice("127.0.0.1", "");
        }

        /// <summary>
        /// Make sure the initialization vector becomes correct.
        /// This is used later for encrypting and decrypting data
        /// </summary>
        [TestMethod]
        public void MiToken_InitializationVector()
        {
            var t = new MiHomeToken("00112233445566778899aabbccddeeff");

            var expectedInitializationVector = new byte[] { 0x6f, 0x43, 0x4f, 0xa9, 0xac, 0xd7, 0x5d, 0xa7, 0x3e, 0x5f, 0xb9, 0x99, 0xf6, 0x41, 0xcd, 0xa2 };
            for( var i=0; i < t.InitializationVector.Length; i++)
            {
                Assert.AreEqual(expectedInitializationVector[i], t.InitializationVector[i], $"Byte {i} value was unexpected");
            }
        }

        /// <summary>
        /// Make sure the key becomes correct.
        /// This is used later for encrypting and decrypting data
        /// </summary>
        [TestMethod]
        public void MiToken_Key()
        {
            var t = new MiHomeToken("00112233445566778899aabbccddeeff");

            var expectedKey = new byte[] { 0x6e, 0x83, 0x11, 0x16, 0x8e, 0xe1, 0x6d, 0x6a, 0xa1, 0xaa, 0x48, 0xc6, 0x41, 0x45, 0x00, 0x3c };
            for (var i = 0; i < t.Key.Length; i++)
            {
                Assert.AreEqual(expectedKey[i], t.Key[i], $"Byte {i} value was unexpected");
            }
        }

        /// <summary>
        /// Make sure the parsing of the token into bytes are correct
        /// </summary>
        [TestMethod]
        public void MiToken_Token()
        {
            var t = new MiHomeToken("00112233445566778899aabbccddeeff");

            var expectedToken = new byte[] {0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff};
            for (var i = 0; i < t.Token.Length; i++)
            {
                Assert.AreEqual(expectedToken[i], t.Token[i], $"Byte {i} value was unexpected");
            }
        }
    }
}
