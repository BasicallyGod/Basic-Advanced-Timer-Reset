param([ValidateSet('build','check','run')][string]$Action='run',[string]$DataDirectory)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
$sdk=Join-Path $repoRoot '.tools/dotnet/dotnet.exe'
if(!(Test-Path $sdk)){throw 'Run ./tools/native-notes.ps1 setup first.'}
$core=Join-Path $repoRoot '.tools/core-source'
if((& git -C $core rev-parse HEAD).Trim() -ne '361d0e32748be7c01e616d91176a844f805e8a08'){throw 'Core source must match the pinned baseline.'}
$env:DOTNET_CLI_HOME=Join-Path $repoRoot '.tools/cli'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$project=Join-Path $repoRoot 'src/Bat.Windows/Bat.Windows.csproj'
& $sdk restore $project --source https://api.nuget.org/v3/index.json --locked-mode
if($LASTEXITCODE -ne 0){throw 'Restore failed.'}
& $sdk build $project --no-restore --disable-build-servers -p:UseSharedCompilation=false
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
if($Action -eq 'check'){
    & $sdk run --project (Join-Path $repoRoot 'tests/Bat.Frame.Checks/Bat.Frame.Checks.csproj') --disable-build-servers -p:UseSharedCompilation=false
    if($LASTEXITCODE -ne 0){throw 'Application/recovery checks failed.'}
}
if($Action -eq 'run'){
    $arguments=@((Join-Path $repoRoot 'src/Bat.Windows/bin/Debug/net10.0-windows/Bat.Reset.Development.dll'))
    if($DataDirectory){$arguments+=@('--data-dir',$DataDirectory)}
    & $sdk @arguments
}
