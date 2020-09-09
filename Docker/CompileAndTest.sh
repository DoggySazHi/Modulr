#!/bin/bash

success=true
randomkey=$(tr -dc 'A-F0-9' < /dev/urandom | head -c6)

cp -r /src/src/* .

for var in "$@"
do
    echo "$randomkey ========== BEGIN COMPILING $var =========="
#    cat $var
    javac -cp junit-platform-console-standalone-1.6.2.jar:. -Xlint:all -Xmaxwarns 100 $var
    if [ $? != 0 ];
    then
        echo "$randomkey ========== FAILED COMPILATION!!! =========="
        success=false
    fi
    echo "$randomkey ========== END COMPILING $var =========="
done

if [ $success == false ];
then
    echo "$randomkey ========== FAILED COMPILATION - NO TEST =========="
    exit 1
fi

# Limit!
ulimit -t 20
#ulimit -v 98304
time
ulimit -a
java -Djava.security.manager -Djava.security.policy=security.policy -Xmx64M -jar junit-platform-console-standalone-1.6.2.jar --disable-banner --include-engine=junit-jupiter --scan-class-path --class-path=. --details=tree --fail-if-no-tests
exit 0
