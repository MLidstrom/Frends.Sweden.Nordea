# Frends.Sweden.Nordea

Calculates a HMAC value for a secret key and input file content. It then adds transmission header, file header, file trailer and transmission trailer to
the output file according to Nordeas Sweden requirements. The file trailer contains the calculated HMAC values for the secret key and for the source file 
content.

The input file must be encoded in ISO-8859-1.

See Nordea Sweden specification https://www.nordea.se/Images/39-16211/technical-specification-HMAC.pdf for more info.

This task do not support sub-files as following Nordea statement:

*"Sending format S2 also makes it possible to run several sub-files of the same type in a single 
sending. Each sub-file is to be surrounded by file items (%020 and %022) and the entire 
sending is to be surrounded by sending items (%001 and %002)."*

***

- [Installing](#installing)
- [Tasks](#tasks)
	- [FileProtectionHmac](#fileProtectionHmac)
		- [Task Parameters](#task-Parameters)
			- [General](#general)
			- [Transmission header](#transmission-header)
			- [File header](#file-header)
			- [File trailer](#file-trailer)
			- [Transmission trailer](#transmission-trailer)
			- [Returns](#returns)
		- [Test](#test)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Sweden.Nordea

# Tasks

## FileProtectionHmac

### Task Parameters

#### General

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Secret key | `string` | Nordea assigned secret key - 32 chars long in HEX value. | `1234567890ABCDEF1234567890ABCDEF` |
| Source file path | `string` | Source file path (must be encoded in ISO-8859-1). | `C:\InFolder\FileUsedforHmacCalculation.txt` |
| Target file path | `string` | Target file path (will be encoded in ISO-8859-1). | `C:\OutFolder\SignedFileWithAddedPosts.txt` |
| Use temp file dir | `bool` | A temporary directory where a temp file used for HMAC calculation will be added and deleted after calculation is done. If set to No (default), then `Source file path` will be used instead. The temp file will allways be named &lt;source file name&gt;_tmp. | `Yes` |
| Target file path | `string` | Temp directory. Only used if property Use temp file dir is set to true| `C:\InFolder\Temp` |

#### Transmission header
Settings for transmission header. Some settings are not customizable and will be automatically added. All values will be right padded with white space if nothing else is described.

| Property | Type | Description | Customizable |
| -------- | -------- | -------- | -------- |
| 001 pos 1 to 4 | `string` | Automatically sets value: %001 | No |
| NodeId pos 5 To 14 | `string` | Max chars 10 (leave empty if not used). | Yes |
| Password pos 15 to 20 | `string` | Max chars 6 (leave empty if not used). | Yes |
| Delivery pos 21 | `string` | Automatically sets value: 0 | No |
| File type pos 22 to 24 | `string` | Max chars 3 (leave empty if not used). | Yes |
| External reference pos 25 to 30 | `string` | Max chars 6. Dates must be specified as yyMMdd. If left empty, then todays date will be added. | Yes |
| Free field pos 31 | `string` | Max chars 1 (leave empty if not used). | Yes |
| Zero pos 32 | `string` | Automatically sets value: 0 | No |
| Reserve pos 33 to 80 | `string` | Max chars 48 (leave empty if not used). | Yes |

#### File header
Settings for file header. Some settings are not customizable and will be automatically added. All values will be right padded with white space if nothing else is described.

| Property | Type | Description | Customizable |
| -------- | -------- | -------- | -------- |
| 020 pos 1 to 4 | `string` | Automatically sets value: %020 | No |
| Destination node pos 5 to 14 | `string` | Max chars 10 (leave empty if not used). | Yes |
| Source node pos 15 to 24 | `string` | Max chars 10 (leave empty if not used). | Yes |
| External reference 1 pos 25 to 31 | `string` | Max chars 7. Dates must be specified as yyMMdd. If left empty, then todays date will be added. | Yes |
| Number of items pos 32 to 38 | `string` | Max chars 7 (leave empty if not used). | Yes |
| External reference 2 pos 39 to 48 | `string` | Max chars 10 (leave empty if not used). | Yes |
| Reserve pos 49 to 80 | `string` | Max chars 32 (leave empty if not used). | Yes |

#### File trailer
Settings for file  trailer. Some settings are not customizable and will be automatically added. All values will be right padded with white space if nothing else is described.

| Property | Type | Description | Customizable |
| -------- | -------- | -------- | -------- |
| 022 pos 1 to 4 | `string` | Automatically sets value: %022 | No |
| Number of Records pos 5 to 11 | `string` | Max chars 7. Will be right padded with "0" (leave empty if not used). | Yes |
| Key verification value hmac pos 12 43 | `string` | Calculated HMAC based on secret key - 32 chars long HEX value. | No |
| File content hmac pos 44 to 75 | `string` | Calculated HMAC based on source file content - 32 chars long HEX value. | No |
| Reserve pos 76 to 80 | `string` | Max chars 5 (leave empty if not used). | Yes | |

#### Transmission trailer
Settings for transmission trailer. Some settings are not customizable and will be automatically added. All values will be right padded with white space if nothing else is described.

| Property | Type | Description | Customizable |
| -------- | -------- | -------- | -------- |
| 002 pos 1 to 4" | `string` | Automatically sets value: %002 | No |
| Reserve pos 5 to 80 | `string` | Max chars 76 (leave empty if not used). | Yes |

### Returns

| Property | Type | Description |
| -------- | -------- | -------- |
| Result | `Object` | Object is returned |

**Result example with only automatically assigned fields in Transmission/File Header/Trailer**
```json
{
	"SourceFilePath": "C:\\Lab\\file\\HMAC\\testfall2d.txt",
	"TargetFilePath": "C:\\Lab\\file\\HMAC\\output\\generated2.txt",
	"TempDirPath": "C:\\Lab\\file\\HMAC\\temp\\",
	"TransmissionHeader": {
		"_001Pos1To4": "%001",
		"NodeIdPos5To14": "          ",
		"PasswordPos15To20": "      ",
		"DeliveryPos21": "0",
		"FileTypePos22To24": "   ",
		"ExternalReferencePos25To30": "221019",
		"FreeFieldPos31": " ",
		"ZeroPos32": "0",
		"ReservePos33To80": "                                                ",
		"TransmissionHeaderLine": "%001                0   221019 0                                                "
	},
	"FileHeader": {
		"_020Pos1To4": "%020",
		"DestinationNodePos5To14": "          ",
		"SourceNodePos15To24": "          ",
		"ExternalReference1Pos25To31": "221019 ",
		"NumberOfItemsPos32To38": "       ",
		"ExternalReference2Pos39To48": "          ",
		"ReservePos49To80": "                                ",
		"FileHeaderLine": "%020                    221019                                                  "
	},
	"FileTrailer": {
		"_002Pos1To4": "%022",
		"NumberOfRecords_Pos_5_To_11": "0000000",
		"KeyVerificationValueHmac_Pos_12_43": "FF365893D899291C3BF505FB3175E880",
		"FileContentHmac_Pos_44_75": "41DE294A10A0758C7EE3849EA9B67D71",
		"ReservePos76To80": "     ",
		"FileTrailerLine": "%0220000000FF365893D899291C3BF505FB3175E88041DE294A10A0758C7EE3849EA9B67D71     "
	},
	"TransmissionTrailer": {
		"_002Pos1To4": "%002",
		"ReservePos5To80": "                                                                            ",
		"TransmissionTrailerLine": "%002                                                                            "
	}
}
```

### Test
Nordea Sweden have test cases that can be run with this task:

https://www.nordea.se/foretag/produkter/betala/programleverantorer/filkommunikation.html#tab=HMAC

# Building

Clone a copy of the repository

`git clone https://github.com/CommunityHiQ/Frends.Sweden.Nordea.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repository on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ------- | ------- |
| 0.0.1   | Development still going on |
