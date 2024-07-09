[cmdletBinding()]
Param()

Push-Location "$PSScriptRoot/src/Cortside.AmqpTools.WebApi"

cmd /c start cmd /k "title AmqpTools Api & dotnet run"

Pop-Location
