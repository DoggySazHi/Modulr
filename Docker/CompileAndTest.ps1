# See CompileAndTest.sh for details.

$success = $true
$randomkey = -join ((48..57) + (65..70) | Get-Random -Count 6 | % {[char]$_})

cp c:\src\files\*.java .

foreach($var in $args) {
    if(!$var.EndsWith(".java")) {
        continue;
    }
    echo "$randomkey ========== BEGIN COMPILING $var =========="

    & javac $var --% -cp .\junit-platform-console-standalone-1.6.2.jar;. -Xlint:all -Xmaxwarns 100
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
& java --% -Djava.security.manager -Djava.security.policy=security.policy -Xmx64M -jar junit-platform-console-standalone-1.6.2.jar --disable-banner --include-engine=junit-jupiter --scan-class-path --class-path=. --details=tree --details-theme=ascii --disable-ansi-colors --fail-if-no-tests

echo "$randomkey ========== END TEST =========="
exit 0