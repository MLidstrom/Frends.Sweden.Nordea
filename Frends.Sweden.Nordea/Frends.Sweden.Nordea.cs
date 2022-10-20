﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

#pragma warning disable 1591

namespace Frends.Sweden.Nordea
{
    public static class NordeaSweden
    {
        /// <summary>
        /// Calculates a HMAC value for the secret key and input file content according to Nordeas Sweden requirements. The input file must be
        /// encoded in ISO-8859-1 and only contain linefeeds CRLF. A new file is generated with added transmission header %001, transmission 
        /// trailer %022, file header %020 and file trailer %022. The file trailer %022 contains the calculated HMAC values for the secret key 
        /// and for the source file content. Some transmission/file property fields in this task are not customizable and are autogenerated with 
        /// a value. 
        /// See Nordea specification https://www.nordea.se/Images/39-16211/technical-specification-HMAC.pdf for more info.
        /// </summary>
        /// <returns>{Object}</returns>
        public static object FileProtectionHmac([PropertyTab] NordeaHmacInputGeneral General, [PropertyTab] NordeaHmacInputTransmissionHeader TransmissionHeader, [PropertyTab] NordeaHmacInputFileHeader FileHeader, [PropertyTab] NordeaHmacInputFileTrailer FileTrailer, [PropertyTab] NordeaHmacInputTransmissionTrailer TransmissionTrailer, CancellationToken cancellationToken)
        {

            string encoding = "iso-8859-1";
            FileInfo targetFilePath = new FileInfo(General.TargetFilePath);
            FileInfo sourceFilepath = new FileInfo(General.SourceFilePath);

            // If target file exists, then throw error since we do not want to overwrite it
            if (targetFilePath.Exists)
            {
                throw new Exception("Target file " + targetFilePath + " already exists!");
            }

            // Get tempDirPath
            string tempDirPath = GetTempDir(General.UseTempFileDir, General.TempDirPath, sourceFilepath);

            // Secret key to byte
            byte[] keyByte = HexStringToByteArray(General.SecretKey, cancellationToken);

            // Get calculated Key Verification Value (KVV) HMAC
            string kvvHmac = GetKeyVerificationValueHmac(keyByte, cancellationToken);

            // Get calculated HMAC based on file content
            string fileContentHmac = GetCalculatedHmac(keyByte, sourceFilepath, tempDirPath, encoding, cancellationToken);

            // Create transmission header
            TransmissionHeader tTransmHead = CreateTransmissionHeader(
                TransmissionHeader.NodeId_Pos_5_To_14,
                TransmissionHeader.Password_Pos_15_To_20,
                TransmissionHeader.FileType_Pos_22_To_24,
                TransmissionHeader.ExternalReference_Pos_25_To_30,
                TransmissionHeader.FreeField_Pos_31,
                TransmissionHeader.Reserve_Pos_33_To_80);

            // Create file header
            FileHeader tFileHead = CreateFileHeader(
                FileHeader.DestinationNode_Pos_5_To_14,
                FileHeader.SourceNode_Pos_15_To_24,
                FileHeader.ExternalReference_1_Pos_25_To_31,
                FileHeader.NumberOfItems_Pos_32_To_38,
                FileHeader.ExternalReference_2_Pos_39_To_48,
                FileHeader.Reserve_Pos_49_To_80);

            // Create file trailer
            FileTrailer tFileTrail = CreateFileTrailer(
                FileTrailer.NumberOfRecords_Pos_5_To_11,
                kvvHmac,
                fileContentHmac,
                FileTrailer.Reserve_Pos_76_To_80);

            // Create transmission trailer
            TransmissionTrailer tTransmTrail = CreateTransmissionTrailer(TransmissionTrailer.Reserve_Pos_5_To_80);

            // Write out file with transmission and file posts
            WriteOutFile(General.SourceFilePath,
                General.TargetFilePath,
                tTransmHead.TransmissionHeaderLine,
                tFileHead.FileHeaderLine,
                tFileTrail.FileTrailerLine,
                tTransmTrail.TransmissionTrailerLine,
                encoding,
                cancellationToken);

            // Create result output
            Result root = new Result
            {
                SourceFilePath = General.SourceFilePath,
                TargetFilePath = General.TargetFilePath,
                TempDirPath = General.TempDirPath,
                TransmissionHeader = tTransmHead,
                FileHeader = tFileHead,
                FileTrailer = tFileTrail,
                TransmissionTrailer = tTransmTrail
            };

            return root;
        }

        private static string GetKeyVerificationValueHmac(byte[] keyByte, CancellationToken cancellationToken)
        {
            // Calculate HMAC for Key Verification Value (KVV)
            byte[] ba = Encoding.UTF8.GetBytes("00000000");
            string msgHexKey = BitConverter.ToString(ba).Replace("-", "");

            return GetSealMAC(keyByte, msgHexKey, cancellationToken);
        }

        private static string GetTempDir(bool useTempFileDir, string tempDirPath, FileInfo sourceFilePath)
        {
            if (useTempFileDir)
            {
                // Create temp directory if not exists
                if (!Directory.Exists(tempDirPath))
                {
                    Directory.CreateDirectory(tempDirPath);
                }

                return tempDirPath;
            }
            else
            {
                return sourceFilePath.DirectoryName;
            }
        }

        private static string GetCalculatedHmac(byte[] keyByte, FileInfo inFilePath, string tempDirPath, string encoding, CancellationToken cancellationToken)
        {
            // Get dictionary used as lookup to get correct hex value for HMAC calculation based on Nordea requirement 
            Dictionary<string, string> d = GetConvertionDictionary();

            // Create a temp file with everything converted to hex without line feeds
            string tmpFile = Path.Combine(tempDirPath, inFilePath.Name + "_tmp");
            StreamWriter tmpOutput = new StreamWriter(tmpFile);

            foreach (string line in File.ReadLines(inFilePath.FullName, Encoding.GetEncoding(encoding)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string lineHex = "";

                foreach (char c in line)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lineHex += d.TryGetValue(c.ToString(), out string value) ? value : "C3";
                }
                tmpOutput.Write(lineHex);
            }

            tmpOutput.Close();
            tmpOutput.Dispose();

            // Get temp file data and generate HMAC
            string msgHexFileContent = File.ReadLines(tmpFile, Encoding.GetEncoding(encoding)).First();

            // Delete temp file
            File.Delete(tmpFile);

            return GetSealMAC(keyByte, msgHexFileContent, cancellationToken);
        }

        private static TransmissionHeader CreateTransmissionHeader(string nodeId_Pos_5_To_14, string password_Pos_15_To_20, string fileType_Pos_22_To_24, string externalReference_Pos_25_To_30, string freeField_Pos_31, string reserve_Pos_33_To_80)
        {
            externalReference_Pos_25_To_30 = String.IsNullOrEmpty(externalReference_Pos_25_To_30) ? DateTime.Now.ToString("yyMMdd") : externalReference_Pos_25_To_30;

            TransmissionHeader th = new TransmissionHeader
            {
                _001Pos1To4 = "%001",
                NodeIdPos5To14 = ValidateCharLengthAndPad(nodeId_Pos_5_To_14, "Node id pos 5 to 14", 10),
                PasswordPos15To20 = ValidateCharLengthAndPad(password_Pos_15_To_20, "Password pos 15 to 20", 6),
                DeliveryPos21 = "0",
                FileTypePos22To24 = ValidateCharLengthAndPad(fileType_Pos_22_To_24, "File type pos 22 to 24", 3),
                ExternalReferencePos25To30 = ValidateCharLengthAndPad(externalReference_Pos_25_To_30, "External reference pos 25 to 30", 6),
                FreeFieldPos31 = ValidateCharLengthAndPad(freeField_Pos_31, "Free field pos 31", 1),
                ZeroPos32 = "0",
                ReservePos33To80 = ValidateCharLengthAndPad(reserve_Pos_33_To_80, "Reserve pos 33 to 80", 48)
            };

            th.TransmissionHeaderLine = th._001Pos1To4 + th.NodeIdPos5To14 + th.PasswordPos15To20 + th.DeliveryPos21 + th.FileTypePos22To24 + th.ExternalReferencePos25To30 + th.FreeFieldPos31 + th.ZeroPos32 + th.ReservePos33To80;

            return (th);
        }

        private static FileHeader CreateFileHeader(string destinationNode_Pos_5_To_14, string sourceNode_Pos_15_To_24, string externalReference_1_Pos_25_To_31, string numberOfItems_Pos_32_To_38, string externalReference_2_Pos_39_To_48, string reserve_Pos_49_To_80)
        {
            externalReference_1_Pos_25_To_31 = String.IsNullOrEmpty(externalReference_1_Pos_25_To_31) ? DateTime.Now.ToString("yyMMdd") : externalReference_1_Pos_25_To_31;

            FileHeader fh = new FileHeader
            {
                _020Pos1To4 = "%020",
                DestinationNodePos5To14 = ValidateCharLengthAndPad(sourceNode_Pos_15_To_24, "Source node pos 15 to 24", 10),
                SourceNodePos15To24 = ValidateCharLengthAndPad(sourceNode_Pos_15_To_24, "Source node pos 15 to 24", 10),
                ExternalReference1Pos25To31 = ValidateCharLengthAndPad(externalReference_1_Pos_25_To_31, "External reference 1 pos 25 to 31", 7),
                NumberOfItemsPos32To38 = ValidateCharLengthAndPad(numberOfItems_Pos_32_To_38, "Number of items pos 32 to 38", 7),
                ExternalReference2Pos39To48 = ValidateCharLengthAndPad(externalReference_2_Pos_39_To_48, "External reference 2 pos 39 to 48", 10),
                ReservePos49To80 = ValidateCharLengthAndPad(reserve_Pos_49_To_80, "Reserve pos 49 to 80", 32)

            };

            fh.FileHeaderLine = fh._020Pos1To4 + fh.DestinationNodePos5To14 + fh.SourceNodePos15To24 + fh.ExternalReference1Pos25To31 + fh.NumberOfItemsPos32To38 + fh.ExternalReference2Pos39To48 + fh.ReservePos49To80;

            return (fh);
        }

        private static FileTrailer CreateFileTrailer(string numberOfRecords_Pos_5_To_11, string kvvHmac, string fileContentHmac, string reserve_Pos_76_To_80)
        {
            FileTrailer ft = new FileTrailer
            {
                _002Pos1To4 = "%022",
                NumberOfRecords_Pos_5_To_11 = ValidateCharLengthAndPad(numberOfRecords_Pos_5_To_11, "Number of Records pos 5 to 11", 7, "zero"),
                KeyVerificationValueHmac_Pos_12_43 = kvvHmac,
                FileContentHmac_Pos_44_75 = fileContentHmac,
                ReservePos76To80 = ValidateCharLengthAndPad(reserve_Pos_76_To_80, "Reserve pos 76 to 80", 5)
            };

            ft.FileTrailerLine = ft._002Pos1To4 + ft.NumberOfRecords_Pos_5_To_11 + ft.KeyVerificationValueHmac_Pos_12_43 + ft.FileContentHmac_Pos_44_75 + ft.ReservePos76To80;

            return ft;
        }

        private static TransmissionTrailer CreateTransmissionTrailer(string reserve_Pos_5_To_80)
        {
            TransmissionTrailer tt = new TransmissionTrailer
            {
                _002Pos1To4 = "%002",
                ReservePos5To80 = ValidateCharLengthAndPad(reserve_Pos_5_To_80, "Reserve pos 5 to 80", 76)
            };

            tt.TransmissionTrailerLine = tt._002Pos1To4 + tt.ReservePos5To80;

            return tt;
        }

        private static void WriteOutFile(string inFilePath, string outFilePath, string transmHead, string fileHeader, string fileTrail, string transmTrail, string encoding, CancellationToken cancellationToken)
        {
            FileStream inStream = new FileStream(inFilePath, FileMode.Open);
            FileStream outStream = File.OpenWrite(outFilePath);

            // Write Transmission Header to outStream
            transmHead += "\r\n";
            outStream.Write(Encoding.GetEncoding(encoding).GetBytes(transmHead), 0, transmHead.Length);

            // Write File Header to outStream
            fileHeader += "\r\n";
            outStream.Write(Encoding.GetEncoding(encoding).GetBytes(fileHeader), 0, fileHeader.Length);

            // Reset inStream to the beginning of the file.
            inStream.Position = 0;

            // Write inStream to outStream
            int bytesRead;
            byte[] buffer = new byte[1024];
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                bytesRead = inStream.Read(buffer, 0, 1024);
                outStream.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            // Write File Trailer to outStream
            fileTrail = "\r\n" + fileTrail;
            outStream.Write(Encoding.GetEncoding(encoding).GetBytes(fileTrail), 0, fileTrail.Length);

            // Write Transmission Trailer to outStream
            transmTrail = "\r\n" + transmTrail;
            outStream.Write(Encoding.GetEncoding(encoding).GetBytes(transmTrail), 0, transmTrail.Length);

            inStream.Close();
            outStream.Close();
            inStream.Dispose();
            outStream.Dispose();
        }

        private static string ValidateCharLengthAndPad(string value, string name, int maxChar, string pad = "ws")
        {
            if (value.Length > maxChar)
            {
                throw new Exception("Maximum characters allowed for '" + name + "' are " + maxChar.ToString() + ", found " + value.Length + "!");
            }

            if (pad == "ws")
            {
                return value.PadRight(maxChar, ' ');
            }
            else
            {
                return value.PadRight(maxChar, '0');
            }
        }

        private static byte[] HexStringToByteArray(string hex, CancellationToken cancellationToken)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static string GetSealMAC(byte[] keyByte, string msgHex, CancellationToken cancellationToken)
        {
            byte[] byteArray = HexStringToByteArray(msgHex, cancellationToken);

            HMACSHA256 hmacsha256 = new HMACSHA256(keyByte);
            byte[] messageHash = hmacsha256.ComputeHash(byteArray);
            byte[] messageArray = new byte[16];
            Array.Copy(messageHash, messageArray, messageArray.Length);

            return BitConverter.ToString(messageArray).Replace("-", "");
        }

        private static Dictionary<string, string> GetConvertionDictionary()
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                { " ", "20" },
                { "!", "21" },
                { "\"", "22" },
                { "#", "23" },
                { "$", "24" },
                { "%", "25" },
                { "&", "26" },
                { "'", "27" },
                { "(", "28" },
                { ")", "29" },
                { "*", "2A" },
                { "+", "2B" },
                { ",", "2C" },
                { "-", "2D" },
                { ".", "2E" },
                { "/", "2F" },
                { "0", "30" },
                { "1", "31" },
                { "2", "32" },
                { "3", "33" },
                { "4", "34" },
                { "5", "35" },
                { "6", "36" },
                { "7", "37" },
                { "8", "38" },
                { "9", "39" },
                { ":", "3A" },
                { ";", "3B" },
                { "<", "3C" },
                { "=", "3D" },
                { ">", "3E" },
                { "?", "3F" },
                { "@", "40" },
                { "É", "40" },
                { "A", "41" },
                { "B", "42" },
                { "C", "43" },
                { "D", "44" },
                { "E", "45" },
                { "F", "46" },
                { "G", "47" },
                { "H", "48" },
                { "I", "49" },
                { "J", "4A" },
                { "K", "4B" },
                { "L", "4C" },
                { "M", "4D" },
                { "N", "4E" },
                { "O", "4F" },
                { "P", "50" },
                { "Q", "51" },
                { "R", "52" },
                { "S", "53" },
                { "T", "54" },
                { "U", "55" },
                { "V", "56" },
                { "W", "57" },
                { "X", "58" },
                { "Y", "59" },
                { "Z", "5A" },
                { "[", "5B" },
                { "Ä", "5B" },
                { "\\", "5C" },
                { "Ö", "5C" },
                { "]", "5D" },
                { "Å", "5D" },
                { "^", "5E" },
                { "Ü", "5E" },
                { "_", "5F" },
                { "`", "60" },
                { "é", "60" },
                { "a", "61" },
                { "b", "62" },
                { "c", "63" },
                { "d", "64" },
                { "e", "65" },
                { "f", "66" },
                { "g", "67" },
                { "h", "68" },
                { "i", "69" },
                { "j", "6A" },
                { "k", "6B" },
                { "l", "6C" },
                { "m", "6D" },
                { "n", "6E" },
                { "o", "6F" },
                { "p", "70" },
                { "q", "71" },
                { "r", "72" },
                { "s", "73" },
                { "t", "74" },
                { "u", "75" },
                { "v", "76" },
                { "w", "77" },
                { "x", "78" },
                { "y", "79" },
                { "z", "7A" },
                { "{", "7B" },
                { "ä", "7B" },
                { "|", "7C" },
                { "ö", "7C" },
                { "}", "7D" },
                { "å", "7D" },
                { "~", "7E" },
                { "ü", "7E" }
            };

            return d;
        }
    }
}
