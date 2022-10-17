# Frends.Sweden.Nordea

frends Community Task for Nordea

[![Actions Status](https://github.com/CommunityHiQ/Frends.Sweden.Nordea/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Sweden.Nordea/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Sweden.Nordea) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
- [Nordea](#Nordea)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Sweden.Nordea

# Tasks

## FileProtectionHmac

### Task Parameters

### General
General settings

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Secret key | `string` | Nordea assigned secret key - 32 chars long in HEX value. | `1234567890ABCDEF1234567890ABCDEF` |
| Source file path | `string` | Source file path (must be encoded in ISO-8859-1). | `C:\InFolder\FileUsedforHmacCalculation.txt` |
| Target file path | `string` | Target file path (will be encoded in ISO-8859-1). | `C:\OutFolder\SignedFileWithAddedPosts.txt` |
| Use temp file dir | bool | A temporary directory where a temp file used for HMAC calculation will be added and deleted after calculation is done. If set to No (default false), then source file dir will be used instead. The temp file will allways be named &lt;source file name&gt;_tmp. | false |
| Target file path | `string` | Temp directory. Only used if property Use temp file dir is set to true| `C:\InFolder\Temp` |

### Transmission header
Settings for transmission header. Some settings are not customizable and will be automatically added. All values will be right padded if nothing else is described.

| Property | Type | Description | Customizable | Example |
| -------- | -------- | -------- | -------- | -------- |
| 001 pos 1 to 4 | `string` | Automatically sets value: %001 | No | |
| NodeId pos 5 To 14 | `string` | Max chars 10 (leave empty if not used). | Yes | |
| Password pos 15 to 20 | `string` | Max chars 6 (leave empty if not used). | Yes | |
| Delivery pos 21 | `string` | Automatically sets value: 0 | No | |
| File type pos 22 to 24 | `string` | Max chars 3 (leave empty if not used). | Yes | |
| External reference pos 25 to 30 | `string` | Max chars 6. Dates must be specified as yyMMdd. If left empty, then todays date will be added. | Yes | |
| Free field pos 31 | `string` | Max chars 1 (leave empty if not used). | Yes | |
| Zero pos 32 | `string` | Automatically sets value: 0 | No | |
| Reserve pos 33 to 80 | `string` | Max chars 48 (leave empty if not used). | Yes | |

### File header
Settings for file header. Some settings are not customizable and will be automatically added. All values will be right padded if nothing else is described.

| Property | Type | Description | Customizable | Example |
| -------- | -------- | -------- | -------- | -------- |
| 020 pos 1 to 4 | `string` | Automatically sets value: %020 | No | |
| Destination node pos 5 to 14 | `string` | Max chars 10 (leave empty if not used). | Yes | |
| Source node pos 15 to 24 | `string` | Max chars 10 (leave empty if not used). | Yes | |
| External reference 1 pos 25 to 31 | `string` | Max chars 7. Dates must be specified as yyMMdd. If left empty, then todays date will be added. | Yes | |
| Number of items pos 32 to 38 | `string` | Max chars 7 (leave empty if not used). | Yes | |
| External reference 2 pos 39 to 48 | `string` | Max chars 10 (leave empty if not used). | Yes | |
| Reserve pos 49 to 80 | `string` | Max chars 32 (leave empty if not used). | Yes | |

### File trailer
Settings for file  trailer. Some settings are not customizable and will be automatically added. All values will be right padded if nothing else is described.

| Property | Type | Description | Customizable | Example |
| -------- | -------- | -------- | -------- | -------- |
| 002 pos 1 to 4 | `string` | Automatically sets value: %002 | No | |
| Number of Records pos 5 to 11 | `string` | Max chars 7. Will be right padded with "0" (leave empty if not used). | Yes | |
| Key verification value hmac pos 12 43 | `string` | Calculated HMAC based on secret key - 32 chars long HEX value | No | |
| File content hmac pos 44 to 75 | `string` | Calculated HMAC based on source file content - 32 chars long HEX value | No | |
| Reserve pos 76 to 80 | `string` | Max chars 5 (leave empty if not used). | Yes | |

### Transmission trailer
Settings for transmission trailer. Some settings are not customizable and will be automatically added.

### Returns

A result object with parameters.

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Replication | `string` | Repeated string. | `foo, foo, foo` |

Usage:
To fetch result use syntax:

`#result.Replication`

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
