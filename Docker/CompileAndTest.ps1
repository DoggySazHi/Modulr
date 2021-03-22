# See CompileAndTest.sh for details.

$success = $true
$randomkey = -join ((48..57) + (65..70) | Get-Random -Count 6 | % {[char]$_})

cp c:\src\files\* .

foreach($var in $args) {
    if(!$var.EndsWith(".java")) {
        continue;
    }
    echo "$randomkey ========== BEGIN COMPILING $var =========="

    & javac $var --% -cp *;. -Xlint:all -Xmaxwarns 100
    if(!$?) {
        echo "!! ======= FAILED COMPILATION!!! ======= !!"
        $success = $false
        break;
    }

    echo "$randomkey ========== END COMPILING $var =========="
}

if(!$success) {
    echo "$randomkey ========== FAILED COMPILATION - NO TEST =========="
    exit 1
}

echo "$randomkey ========== BEGIN TEST =========="

Get-ChildItem . -recurse -include *.java | remove-item
& java --% -Xmx64M -cp *;. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time

echo "$randomkey ========== END TEST =========="
exit 0
