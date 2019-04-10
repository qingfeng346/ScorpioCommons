$version = "1.0.4"


$today = Get-Date
$date = $today.ToString('yyyy-MM-dd')
Remove-Item ./bin/* -Force -Recurse

Set-Location ./ScorpioCommons
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

Set-Location ../
