[CmdletBinding()]
Param 
(
    [Parameter(Mandatory = $true)][string]$version
)

dotnet remove .\src\Cortside.AmqpTools.DomainService\Cortside.AmqpTools.DomainService.csproj package AmqpTools.Core

dotnet add .\src\Cortside.AmqpTools.DomainService\Cortside.AmqpTools.DomainService.csproj package AmqpTools.Core --version $version --source '..\amqptools\artifacts\'