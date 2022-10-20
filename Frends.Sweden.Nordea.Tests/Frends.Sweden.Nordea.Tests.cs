using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace Frends.Sweden.Nordea.Tests
{
    [TestFixture]
    class TestClass
    {
        /// <summary>
        /// You need to run Frends.Sweden.Nordea.SetPaswordsEnv.ps1 before running unit test, or some other way set environment variables e.g. with GitHub Secrets.
        /// </summary>

        private readonly string _secretKey = "1234567890ABCDEF1234567890ABCDEF";
        private readonly string _testFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        private string _tempTestFolder;
        private Result _result;
        private string[] _lines;

        [SetUp]
        public void TestSetup()
        {
            // Create tmp directory if not exists
            _tempTestFolder = Path.Combine(Path.GetTempPath(), "NordeaSwedenTest");
            if (!Directory.Exists(_tempTestFolder)) Directory.CreateDirectory(_tempTestFolder);

            // Create test properties and run
            var nordeaHmacInputGeneral = new NordeaHmacInputGeneral
            {
                SecretKey = _secretKey,
                SourceFilePath = _testFilePath + @"\testfile.txt",
                TargetFilePath = _tempTestFolder + @"\generated.txt",
                UseTempFileDir = true,
                TempDirPath = _testFilePath + @"\temp",
            };

            var nordeaHmacInputTransmissionHeader = new NordeaHmacInputTransmissionHeader
            {
                NodeId_Pos_5_To_14 = "AAAAAAAAAA",
                Password_Pos_15_To_20 = "BBBBBB",
                FileType_Pos_22_To_24 = "CCC",
                FreeField_Pos_31 = "E",
                Reserve_Pos_33_To_80 = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
            };

            var nordeaHmacInputFileHeader = new NordeaHmacInputFileHeader
            {
                DestinationNode_Pos_5_To_14 = "AAAAAAAAAA",
                SourceNode_Pos_15_To_24 = "BBBBBBBBBB",
                ExternalReference_1_Pos_25_To_31 = "CCCCCCC",
                NumberOfItems_Pos_32_To_38 = "DDDDDDD",
                ExternalReference_2_Pos_39_To_48 = "EEEEEEEEEE",
                Reserve_Pos_49_To_80 = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
            };

            var nordeaHmacInputFileTrailer = new NordeaHmacInputFileTrailer
            {
                NumberOfRecords_Pos_5_To_11 = "AAAAAAA",
                Reserve_Pos_76_To_80 = "BBBBB"
            };

            var nordeaHmacInputTransmissionTrailer = new NordeaHmacInputTransmissionTrailer
            {
                Reserve_Pos_5_To_80 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            };

            var result = Nordea.NordeaSweden.FileProtectionHmac(nordeaHmacInputGeneral, nordeaHmacInputTransmissionHeader, nordeaHmacInputFileHeader, nordeaHmacInputFileTrailer, nordeaHmacInputTransmissionTrailer, new CancellationToken());

            _result = (Result)result;

            // Get all lines from generated test file
            _lines = File.ReadAllLines(_tempTestFolder + @"\generated.txt");
        }

        [TearDown]
        public void TestTearDown()
        {
            Directory.Delete(_tempTestFolder, true);
        }

        [Test]
        public void KeyVerificationValueHmacTest()
        {
            // Get KVV HMAC from generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%022"));
            var kvv = t[0].Substring(11, 32);

            // KVV HMAC should be same as as result
            Assert.AreEqual(_result.FileTrailer.KeyVerificationValueHmac_Pos_12_43, kvv);
        }

        [Test]
        public void FileContentHmacTest()
        {
            // Get file content Hmac from generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%022"));
            var fc = t[0].Substring(43, 32);

            // File content HMAC should be same as as result output
            Assert.AreEqual(_result.FileTrailer.FileContentHmac_Pos_44_75, fc);
        }

        [Test]
        public void TransmissionHeaderTest()
        {
            // Get transmission header line in generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%001"));

            // Transmission header line should be 80 chars long
            Assert.AreEqual(80, t[0].Length);

            // Transmission header line should be same as result output
            Assert.AreEqual(_result.TransmissionHeader.TransmissionHeaderLine, t[0]);
        }

        [Test]
        public void FileHeaderTest()
        {
            // Get file header line in generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%020"));

            // File header line should be 80 chars long
            Assert.AreEqual(80, t[0].Length);

            // File header line should be same as result JObject
            Assert.AreEqual(_result.FileHeader.FileHeaderLine, t[0]);
        }

        [Test]
        public void FileTrailerTest()
        {
            // Get file trailer line in generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%022"));

            // File trailer line should be 80 chars long
            Assert.AreEqual(80, t[0].Length);

            // File trailer line should be same as result JObject
            Assert.AreEqual(_result.FileTrailer.FileTrailerLine, t[0]);
        }

        [Test]
        public void TransmissionTrailerTest()
        {
            // Get transmission trailer line in generated target file
            var t = Array.FindAll(_lines, s => s.StartsWith("%002"));

            // Transmission trailer line should be 80 chars long
            Assert.AreEqual(80, t[0].Length);

            // Transmission trailer line should be same as result JObject
            Assert.AreEqual(_result.TransmissionTrailer.TransmissionTrailerLine, t[0]);
        }
    }
}
