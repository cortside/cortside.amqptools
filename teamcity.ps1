<#
    .SYNOPSIS
        Build pipeline controller script for use by TeamCity
    .DESCRIPTION
        This script is responsible for creating any artifacts needed for the build pipeline.
#>

[CmdletBinding()]
Param 
(
	[Parameter(Mandatory = $false)][string]$buildCounter = "0",
	[Parameter(Mandatory = $false)][switch]$pushImage,
	[Parameter(ValueFromRemainingArguments)][string[]]$arguments
)

# script should stop on all errors
$ErrorActionPreference = 'Stop';

# build the docker image(s)
& .\deploy\build-dockerimages.ps1 -buildCounter $buildCounter -pushImage:$pushImage.IsPresent

# TEMPORARY build of octopus zip package until octopus deploy can change
Write-Output "prepping package for octopus"
$config = get-content ./repository.json | ConvertFrom-Json
$buildconfig = get-content ./buildconfig.json | ConvertFrom-Json

$branch = $buildconfig.branch
$octopusChannel = "Non-Production"
if ($branch -like "master")	{
	$octopusChannel = "Production"
}	

Write-Output "found imageversion in buildconfig.json: $($buildconfig.ImageVersion)"
Write-Output "##teamcity[setParameter name='env.OctopusChannel' value='$($octopusChannel)']"
Write-Output "##teamcity[setParameter name='env.OctopusVersion' value='$($buildconfig.ImageVersion)']"
Write-Output "##teamcity[buildNumber '$($buildconfig.version)']"

# make sure output file for zip exists
If(test-path ./deploy/ps) {
	rm -Recurse -Force ./deploy/ps
}
mkdir ./deploy/ps  |Out-Null

[Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem') | Out-Null
$zipfile = "./deploy/ps/$($buildconfig.service).$($buildconfig.ImageVersion).zip"
Write-Output "creating zip file with name $zipfile for channel $octopusChannel"
Compress-Archive -Path ./deploy/ps -DestinationPath $zipfile
Compress-Archive -Path ./deploy/kubernetes/* -Update -DestinationPath $zipfile
$files = (gci -Path "src/$($config.build.publishableProject)" appsettings.json)
Compress-Archive -Path $files[0].FullName -Update -DestinationPath $zipfile
Compress-Archive -Path .\update-database.ps1 -Update -DestinationPath $zipfile
Compress-Archive -Path .\repository.psm1 -Update -DestinationPath $zipfile
Compress-Archive -Path .\repository.json -Update -DestinationPath $zipfile
Compress-Archive -Path .\buildconfig.json -Update -DestinationPath $zipfile

# create output temp directory for sql
If(test-path ./temp) {
	rm -Recurse -Force ./temp
}
mkdir ./temp  |Out-Null

# make sure sql directory is there with something in it
If(!(test-path ./src/sql)) {
	mkdir ./src/sql  |Out-Null
}
cp ./buildconfig.json ./src/sql # to make sure there is a file there to zip (that is not hidden)

Copy-Item -Path .\src\sql -Destination .\temp\src\sql -Recurse # to preserve structure for the script
Get-ChildItem -Path .\temp\src | Compress-Archive -Update -DestinationPath $zipfile
Remove-Item -Path .\temp -Recurse -Force

# show octopus zipfile and contents
(gci $zipfile).FullName
[IO.Compression.ZipFile]::OpenRead($zipfile).Entries.FullName |% { "$zipfile`:$_" }
