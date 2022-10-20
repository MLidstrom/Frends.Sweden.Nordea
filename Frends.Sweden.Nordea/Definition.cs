#pragma warning disable 1591

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Sweden.Nordea
{
    /// <summary>
    /// Input class for task SplitXmlFileToFileParts
    /// </summary>
    public class NordeaHmacInputGeneral
    {
        /// <summary>
        /// Nordea assigned secret key in 32 char hex
        /// </summary>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("1234567890ABCDEF1234567890ABCDEF")]
        public string SecretKey { get; set; }

        /// <summary>
        /// Source file path (must be encoded in ISO-8859-1)
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(@"C:\InFolder\FileUsedforHmacCalculation.txt")]
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Target file path (will be encoded in ISO-8859-1)
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(@"C:\OutFolder\SignedFileWithAddedPosts.txt")]
        public string TargetFilePath { get; set; }

        /// <summary>
        /// Use temp directory? A temporary directory where a temp file used for 
        /// HMAC calculation will be added and deleted after calculation is done.
        /// If set to No (default), then source file dir will be used instead.
        /// The temp file will allways be named &lt;source file name&gt;_tmp.
        /// </summary>
        public bool UseTempFileDir { get; set; }

        /// <summary>
        /// Temp directory
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(UseTempFileDir), "", true)]
        [DefaultValue(@"C:\InFolder\Temp")]
        public string TempDirPath { get; set; }
    }

    public class NordeaHmacInputTransmissionHeader
    {
        /// <summary>
        /// Node Id - Max chars 10 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        //[DefaultValue("1234567890ABCDEF1234567890ABCDEF")]
        public string NodeId_Pos_5_To_14 { get; set; }

        /// <summary>
        /// Password - Max chars 6 (leave empty if not used).
        /// </summary>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Password_Pos_15_To_20 { get; set; }

        /// <summary>
        /// File type - Max chars 3 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string FileType_Pos_22_To_24 { get; set; }

        /// <summary>
        /// External reference - Max chars 6. Dates must be specified as yyMMdd. If left empty, then todays date will be added.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string ExternalReference_Pos_25_To_30 { get; set; }

        /// <summary>
        /// Free field - Max chars 1 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string FreeField_Pos_31 { get; set; }

        /// <summary>
        /// Reserve - Max chars 48 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Reserve_Pos_33_To_80 { get; set; }
    }

    public class NordeaHmacInputFileHeader
    {
        /// <summary>
        /// Destination node - Max chars 10 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string DestinationNode_Pos_5_To_14 { get; set; }

        /// <summary>
        /// Source node - Max chars 10 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string SourceNode_Pos_15_To_24 { get; set; }

        /// <summary>
        /// External reference 1 - Max chars 7. Dates must be specified as yyMMdd. If left empty, then todays date will be added.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string ExternalReference_1_Pos_25_To_31 { get; set; }

        /// <summary>
        /// Number of items - Max chars 7 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string NumberOfItems_Pos_32_To_38 { get; set; }

        /// <summary>
        /// External reference 2 - Max chars 10 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string ExternalReference_2_Pos_39_To_48 { get; set; }

        /// <summary>
        /// Reserve - Max chars 32 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Reserve_Pos_49_To_80 { get; set; }
    }

    public class NordeaHmacInputFileTrailer
    {
        /// <summary>
        /// Number of records - Max chars 7. Will be right padded with "0" (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string NumberOfRecords_Pos_5_To_11 { get; set; }

        /// <summary>
        /// Reserve - Max chars 5 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Reserve_Pos_76_To_80 { get; set; }
    }

    public class NordeaHmacInputTransmissionTrailer
    {
        /// <summary>
        /// Reserve - Max chars 76 (leave empty if not used).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Reserve_Pos_5_To_80 { get; set; }
    }

    public class Result
    {
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        public string TempDirPath { get; set; }
        public TransmissionHeader TransmissionHeader { get; set; }
        public FileHeader FileHeader { get; set; }
        public FileTrailer FileTrailer { get; set; }
        public TransmissionTrailer TransmissionTrailer { get; set; }
    }

    public class TransmissionHeader
    {
        public string _001Pos1To4 { get; set; }
        public string NodeIdPos5To14 { get; set; }
        public string PasswordPos15To20 { get; set; }
        public string DeliveryPos21 { get; set; }
        public string FileTypePos22To24 { get; set; }
        public string ExternalReferencePos25To30 { get; set; }
        public string FreeFieldPos31 { get; set; }
        public string ZeroPos32 { get; set; }
        public string ReservePos33To80 { get; set; }
        public string TransmissionHeaderLine { get; set; }
    }

    public class FileHeader
    {
        public string _020Pos1To4 { get; set; }
        public string DestinationNodePos5To14 { get; set; }
        public string SourceNodePos15To24 { get; set; }
        public string ExternalReference1Pos25To31 { get; set; }
        public string NumberOfItemsPos32To38 { get; set; }
        public string ExternalReference2Pos39To48 { get; set; }
        public string ReservePos49To80 { get; set; }
        public string FileHeaderLine { get; set; }
    }

    public class FileTrailer
    {
        public string _002Pos1To4 { get; set; }
        public string NumberOfRecords_Pos_5_To_11 { get; set; }
        public string KeyVerificationValueHmac_Pos_12_43 { get; set; }
        public string FileContentHmac_Pos_44_75 { get; set; }
        public string ReservePos76To80 { get; set; }
        public string FileTrailerLine { get; set; }
    }

    public class TransmissionTrailer
    {
        public string _002Pos1To4 { get; set; }
        public string ReservePos5To80 { get; set; }
        public string TransmissionTrailerLine { get; set; }
    }
}
