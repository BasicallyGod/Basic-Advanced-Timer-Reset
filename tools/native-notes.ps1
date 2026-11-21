param([ValidateSet('setup','build','check','run')][string]$Action='run', [string]$DataDirectory)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
$toolsPath=Join-Path $repoRoot '.tools'
$dotnetPath=Join-Path $toolsPath 'dotnet/dotnet.exe'
$coreCommit='361d0e32748be7c01e616d91176a844f805e8a08'
$appProject=Join-Path $repoRoot 'experiments/native-notes/NativeNotes.csproj'
$checksProject=Join-Path $repoRoot 'experiments/native-notes-checks/NativeNotes.Checks.csproj'
$env:DOTNET_CLI_HOME=Join-Path $toolsPath 'cli'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
function Checked([scriptblock]$Command) { & $Command; if($LASTEXITCODE -ne 0){throw "Command failed with exit code $LASTEXITCODE"} }
if($Action -eq 'setup') {
    New-Item -ItemType Directory -Force $toolsPath | Out-Null
    if(!(Test-Path $dotnetPath)) {
        $installer=Join-Path $toolsPath 'dotnet-install.ps1'
        Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
        & $installer -Version '10.0.404' -InstallDir (Join-Path $toolsPath 'dotnet') -NoPath
        if(!(Test-Path $dotnetPath)){throw 'SDK setup did not complete.'}
    }
    $coreSource=Join-Path $toolsPath 'core-source'
    if(!(Test-Path (Join-Path $coreSource '.git'))) {Checked {git clone --no-checkout https://github.com/BasicallyGod/Basic-Timer-Core.git $coreSource}}
    Checked {git -C $coreSource checkout --detach $coreCommit}
    $actual=(& git -C $coreSource rev-parse HEAD).Trim()
    if($actual -ne $coreCommit){throw 'Shared Core source did not match the pinned baseline.'}
}
if(!(Test-Path $dotnetPath)){throw 'Run ./tools/native-notes.ps1 setup first.'}
$sourcePath=Join-Path $toolsPath 'core-source'
$actual=(& git -C $sourcePath rev-parse HEAD).Trim()
if($actual -ne $coreCommit){throw 'Shared Core source differs from the pinned experiment baseline. Run setup.'}
foreach($project in @($appProject,$checksProject)) {
    Checked {& $dotnetPath restore $project --source https://api.nuget.org/v3/index.json --locked-mode}
}
if($Action -eq 'check') {Checked {& $dotnetPath run --project $checksProject --no-restore};exit}
Checked {& $dotnetPath build $appProject --no-restore}
if($Action -eq 'run') {
    $runArgs=@('run','--project',$appProject,'--no-build','--')
    if($DataDirectory){$runArgs+=@('--data-dir',$DataDirectory)}
    Checked {& $dotnetPath @runArgs}
}
