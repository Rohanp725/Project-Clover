# Stop hook: a session may not end with C# that Unity cannot compile.
# Runs only when .cs files changed since the last commit and the Unity editor is reachable through the Unity CLI.
$ErrorActionPreference = 'SilentlyContinue'
$raw = [Console]::In.ReadToEnd()
$in = $null
try { $in = $raw | ConvertFrom-Json } catch {}
if ($in -and $in.stop_hook_active) { exit 0 }   # we already sent Claude back once; do not loop

$changed = @(git status --porcelain -- '*.cs' 2>$null)
if ($changed.Count -eq 0) { exit 0 }

$out = unity --no-banner --no-pager --non-interactive --json recompile --timeout 120 2>$null
if (-not $out) { exit 0 }
$r = $null
try { $r = ($out -join "`n") | ConvertFrom-Json } catch { exit 0 }
if (-not $r -or -not $r.success) { exit 0 }   # no editor connected: nothing we can check here

$errors = @($r.data.errors)
if ($r.data.failed -or $r.data.compilationFailed -or $errors.Count -gt 0) {
    $lines = $errors | Select-Object -First 15 | ForEach-Object {
        if ($_ -is [string]) { $_ } elseif ($_.file) { "$($_.file):$($_.line): $($_.message)" } else { ($_ | ConvertTo-Json -Compress) }
    }
    [Console]::Error.WriteLine("Unity still reports compile errors after your C# edits. Fix them before stopping:`n" + ($lines -join "`n"))
    exit 2
}
exit 0
