using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Convex.Net.Model.Services {
    public class Handshake {
        #region MEMBERS

        private const int KEY_SIZE = 512;
        private const int BLOCK_SIZE = 256;
        private const int BASE_SIZE = 4;

        public bool IsInitialised { get; }

        private RandomNumberGenerator Random { get; }
        public byte[] Base { get; }
        public int BaseInt32 => BitConverter.ToInt32(Base, 0);

        public byte[] PublicKey {
            get => publicKey;
            private set {
                if (value.Length != KEY_SIZE)
                    throw new ArgumentException($"Length of new byte array must be {KEY_SIZE} bytes");

                publicKey = value;
                PublicKeyInt512 = new BigInteger(publicKey);
                AsymmetricKey = GetAsymmetricKey();
            }
        }

        private byte[] PrivateKey {
            get => privateKey;
            set {
                if (value.Length != KEY_SIZE)
                    throw new ArgumentException($"Length of new byte array must be {KEY_SIZE} bytes");

                privateKey = value;
                PrivateKeyInt512 = new BigInteger(privateKey);
                AsymmetricKey = GetAsymmetricKey();
            }
        }

        private byte[] AsymmetricKey {
            get => asymmetricKey;
            set {
                asymmetricKey = value;
                AsymmetricKeyInt = new BigInteger(asymmetricKey);
            }
        }

        public BigInteger PublicKeyInt512 { get; set; }
        private BigInteger PrivateKeyInt512 { get; set; }
        private BigInteger AsymmetricKeyInt { get; set; }


        private byte[] asymmetricKey;
        private byte[] privateKey;
        private byte[] publicKey;

        #endregion


        public Handshake() {
            //Random = RandomNumberGenerator.Create();
            //Base = new byte[BASE_SIZE];
            //PublicKey = new byte[KEY_SIZE];
            //PrivateKey = new byte[KEY_SIZE];

            //InitialiseKeys();

            //IsInitialised = true;

            // todo separate PublicKey and PrivateKey into field/Property relationship
            // todo set up BigIntegers in propertyies that update when field is changed
        }

        #region UTIL

        private static bool IsPrime(int number) {
            if (number == 1)
                return false;
            if (number == 2)
                return true;
            if (number % 2 == 0)
                return false;

            int boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        #endregion

        #region INIT

        private void InitialiseKeys() {
            PublicKeyBaseToRandomPrime();
            Random.GetBytes(PublicKey);
            Random.GetBytes(PrivateKey);

            PublicKeyInt512 = new BigInteger(PublicKey);
            PrivateKeyInt512 = new BigInteger(PrivateKey);

            EnsurePrivateKeySize();
        }

        private void PublicKeyBaseToRandomPrime() {
            do {
                Random.GetBytes(Base);
            } while (!IsPrime(BitConverter.ToInt32(PublicKey, 0)));
        }

        private void EnsurePrivateKeySize() {
            while (PrivateKeyInt512 > PublicKeyInt512)
                Random.GetBytes(PrivateKey);
        }

        #endregion

        #region CRYPTO

        public byte[] Encrypt(byte[] externalAsymmetricKey, byte[] data) {
            byte[] symmetricKey = GetSymmetricKey(externalAsymmetricKey);

            if (externalAsymmetricKey == null || externalAsymmetricKey.Length != KEY_SIZE)
                throw new ArgumentException($"External Key need to be {KEY_SIZE} bytes.");
            if (data == null)
                throw new NullReferenceException("Data payload cannot be null.");

            byte[] cipherText;
            byte[] iv;

            using (AesManaged aes = new AesManaged {KeySize = KEY_SIZE * 8, BlockSize = BLOCK_SIZE * 8, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7}) {
                aes.GenerateIV();
                iv = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(symmetricKey, iv)) {
                    using (MemoryStream cipherStream = new MemoryStream()) {
                        using (CryptoStream cryptoStream = new CryptoStream(cipherStream, encryptor, CryptoStreamMode.Write)) {
                            using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream)) {
                                binaryWriter.Write(data);
                            }

                            cipherText = cipherStream.ToArray();
                        }
                    }
                }
            }

            using (HMACSHA512 hmac = new HMACSHA512(symmetricKey)) {
                using (MemoryStream encryptedStream = new MemoryStream()) {
                    using (BinaryWriter binaryWriter = new BinaryWriter(encryptedStream)) {
                        binaryWriter.Write(iv);
                        binaryWriter.Write(cipherText);
                        binaryWriter.Flush();

                        byte[] tag = hmac.ComputeHash(encryptedStream.ToArray());

                        binaryWriter.Write(tag);
                    }

                    return encryptedStream.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] externalAsymmetricKey, byte[] data) {
            byte[] symmetricKey = GetSymmetricKey(externalAsymmetricKey);

            if (externalAsymmetricKey == null || externalAsymmetricKey.Length != KEY_SIZE)
                throw new ArgumentException($"External Key need to be {KEY_SIZE} bytes.");
            if (data == null)
                throw new NullReferenceException("Data payload cannot be null.");

            using (HMACSHA512 hmac = new HMACSHA512(externalAsymmetricKey)) {
                byte[] sentTag = new byte[hmac.HashSize / 8];

                byte[] calcTag = hmac.ComputeHash(data, 0, data.Length - sentTag.Length);
                const int ivLength = BLOCK_SIZE / 8;

                if (data.Length < sentTag.Length + ivLength) {
                    return null;
                }

                Array.Copy(data, data.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                int compare = 0;
                for (int i = 0; i < sentTag.Length; i++) {
                    compare |= sentTag[i] ^ calcTag[i];
                }

                if (compare != 0)
                    return null;

                using (AesManaged aes = new AesManaged() {KeySize = KEY_SIZE * 8, BlockSize = BLOCK_SIZE * 8, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7}) {

                    byte[] iv = new byte[ivLength];
                    Array.Copy(data, 0, iv, 0, iv.Length);


                }

                return null;

            }
        }

        private byte[] GetAsymmetricKey() {
            return (BaseInt32 ^ (PrivateKeyInt512 % PublicKeyInt512)).ToByteArray();
        }

        private byte[] GetSymmetricKey(byte[] externalAsymmetricKey) {
            return (AsymmetricKeyInt ^ (new BigInteger(externalAsymmetricKey) % PublicKeyInt512)).ToByteArray();
        }

        #endregion
    }
}
