cd d:\MEox\UITer\SE104\LibraryManagement\THUVIENZ
dotnet add THUVIENZ.csproj package Microsoft.Data.SqlClient
dotnet add THUVIENZ.csproj package System.Configuration.ConfigurationManager

mkdir Models -ErrorAction SilentlyContinue
mkdir ViewModels -ErrorAction SilentlyContinue
mkdir DAL -ErrorAction SilentlyContinue
mkdir BLL -ErrorAction SilentlyContinue
mkdir Core -ErrorAction SilentlyContinue

if (Test-Path View) { Rename-Item View Views }
elseif (-not (Test-Path Views)) { mkdir Views -ErrorAction SilentlyContinue }

if (Test-Path Login.xaml) { Move-Item Login.xaml Views\Login.xaml }
if (Test-Path Login.xaml.cs) { Move-Item Login.xaml.cs Views\Login.xaml.cs }
if (Test-Path MainWindow.xaml) { Move-Item MainWindow.xaml Views\MainWindow.xaml }
if (Test-Path MainWindow.xaml.cs) { Move-Item MainWindow.xaml.cs Views\MainWindow.xaml.cs }

Get-ChildItem Views\*.cs | ForEach-Object {
    $txt = Get-Content $_.FullName -Raw
    $txt = $txt -replace 'namespace THUVIENZ\r?\n?\{', 'namespace THUVIENZ.Views
{'
    Set-Content $_.FullName $txt -Encoding UTF8
}

Get-ChildItem Views\*.xaml | ForEach-Object {
    $txt = Get-Content $_.FullName -Raw
    $txt = $txt -replace 'x:Class="THUVIENZ\.', 'x:Class="THUVIENZ.Views.'
    Set-Content $_.FullName $txt -Encoding UTF8
}

$appXaml = Get-Content App.xaml -Raw
$appXaml = $appXaml -replace 'StartupUri="Login.xaml"', 'StartupUri="Views/Login.xaml"'
Set-Content App.xaml $appXaml -Encoding UTF8
