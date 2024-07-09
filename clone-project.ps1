$myNewProjectName = 'AmqpTools'
  
$myExtensions = @("*.cs", "*.csproj", "*.sln", "*.ps1 -exclude clone-project.ps1", "*.sh", "*.json", "*.xml", "*.ncrunchsolution", "*.user", "*.toml")

foreach ($extension in $myExtensions) {
  # Replace the names in the files and the files
  $configFiles = iex "Get-ChildItem -Path .\* $extension -rec"
  foreach ($file in $configFiles) {
    ((Get-Content $file.PSPath) -replace "Template", "$myNewProjectName" ) |
    out-file $file.PSPath
    rename-item -path $file.FullName -NewName ($file.name -replace 'Template', "$myNewProjectName")
  }
}

gci . -rec -Directory -Filter *Template* | foreach-object { rename-item -path $_.FullName -NewName ($_.name -replace 'Template', "$myNewProjectName") }
$myNewProjectNameLower = $myNewProjectName.tolower()
((Get-Content .\deploy\docker\docker-compose.yml) -replace "REPLACE_ME", "$myNewProjectNameLower") | out-file -encoding ascii .\deploy\docker\docker-compose.yml

# make sure all file encoding is good
& "$PSScriptRoot\convert-encoding.ps1" -filePaths (gci -Exclude .git*, *.dll -Recurse | where { $_.Attributes -ne 'Directory' } | % { $_.FullName })
