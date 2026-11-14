# SessionStart hook: tell Claude, in three lines, what it can reach right now, so it does not guess.
$ErrorActionPreference = 'SilentlyContinue'
$out = unity --no-banner --no-pager --non-interactive --json status 2>$null
$r = $null
try { $r = ($out -join "`n") | ConvertFrom-Json } catch {}
if ($r -and $r.success -and $r.data.count -gt 0) {
    foreach ($i in $r.data.instances) {
        Write-Output "Unity editor reachable: $($i.version) on port $($i.port), state $($i.state), project $($i.project)."
    }
} else {
    Write-Output "Unity editor: none reachable through the Unity CLI. Compile, test and capture need the project open in Unity; ask the user to open it rather than starting Unity.exe yourself."
}
$changed = @(git status --porcelain 2>$null)
Write-Output "Working tree: $($changed.Count) changed or untracked path(s) (git status --short for the list)."
$blender = Get-ChildItem "C:\Program Files\Blender Foundation\Blender*\blender.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($blender) { Write-Output "Blender installed at $($blender.FullName); the blender MCP server works only while Blender runs with its MCP add-on started (N panel > MCP for Blender > Start MCP Server)." }
else { Write-Output "Blender: not installed; the blender MCP server in .mcp.json will not connect." }
exit 0
