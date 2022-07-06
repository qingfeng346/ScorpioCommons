$version = "1.1.18"


$today = Get-Date
$date = $today.ToString('yyyy-MM-dd')
Remove-Item ./bin/* -Force -Recurse

Set-Location ./Scorpio.Commons
$fileData = @"
namespace Scorpio.Commons {
    public static class Version {
        public const string version = "$version";
        public const string date = "$date";
    }
}
"@
$fileData | Out-File -Encoding utf8 ./src/Version.cs
dotnet restore
dotnet build
dotnet pack -p:PackageVersion=$version -o ../bin

Set-Location ../bin
$apiKey = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String("b3kyZDVxNmEyenZ1enlsZm5rcHFhaGU0eGlxMnJuZHViY2ZuMmQ2MnE3YmoyYQ=="))
$nupkgName = "Scorpio.Commons.$version.nupkg"
dotnet nuget push $nupkgName -k $apiKey -s https://api.nuget.org/v3/index.json
Set-Location ../