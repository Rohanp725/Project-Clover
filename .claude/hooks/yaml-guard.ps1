# PreToolUse hook for Bash and PowerShell: refuse shell commands that write into Unity's serialized files.
# Edit/Write on them are already denied in settings.json; this closes the sed/redirect/cp/mv/rm route.
# It blocks only when a writing operator is followed, in the same pipeline segment, by a path token ending in one of the
# protected extensions. Reading, grepping, finding and diffing them stays allowed, and so does text that merely contains
# a protected extension inside a longer name (".unity-mcp").
$ErrorActionPreference = 'SilentlyContinue'
$raw = [Console]::In.ReadToEnd()
$in = $null
try { $in = $raw | ConvertFrom-Json } catch { exit 0 }
if (-not $in -or $in.tool_name -notin @('Bash', 'PowerShell')) { exit 0 }
$cmd = [string]$in.tool_input.command
if (-not $cmd) { exit 0 }

$ops = '(?:^|[\s;&|(])(?:sed\s+-i|tee\s|cp\s|mv\s|rm\s|Set-Content|Out-File|Add-Content|Copy-Item|Move-Item|Remove-Item|>>?)'
$segment = '[^|;&]*?'
$end = '(?=[\s"''|;&<>)]|$)'
$path = '["'']?(?:[^\s"''|;&<>]*\.(?:unity|prefab|meta|mat|anim|controller)' + $end + '|Assets[\\/][^\s"''|;&<>]*\.asset' + $end + ')'

if ($cmd -match "$ops$segment$path") {
    [Console]::Error.WriteLine("Blocked: this command writes a Unity serialized file (.unity/.prefab/.meta/.mat/.anim/.controller or an Assets .asset). Unity owns those: make the change through the Unity CLI (unity command ...) or an editor script so Unity writes the YAML and the .meta itself. Reading, grepping or diffing them is fine.")
    exit 2
}
exit 0
