[cmdletBinding()]
Param(
	[string]$dockerlogin = "acme.azurecr.io"
)
$build = gc $PSScriptRoot\src\build.json -raw | ConvertFrom-json

$image = "$dockerlogin/loandocuments"
$tag = $build.build.tag

docker build --pull -t ${image}:${tag} .
docker images | select-string $image

if (!$tag.EndsWith("-local")) {
	docker push ${image}:${tag}
}
