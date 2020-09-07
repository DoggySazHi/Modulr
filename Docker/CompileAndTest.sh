#!/bin/bash

success=true

cp -r /src/src/* .

for var in "$@"
do
    echo "========== BEGIN COMPILING $var =========="
    cat $var
    javac -cp junit-platform-console-standalone-1.6.2.jar:. -Xlint:all -Xmaxwarns 100 $var
    if [ $? != 0 ];
    then
        echo "========== FAILED COMPILATION!!! =========="
        success=false
    fi
    echo "========== END COMPILING $var =========="
done

if [ $success == false ];
then
    echo "========== FAILED COMPILATION - NO TEST =========="
    exit 1
fi

java -Djava.security.manager -Djava.security.policy=security.policy -Xmx64M -jar junit-platform-console-standalone-1.6.2.jar --disable-banner --include-engine=junit-jupiter --scan-class-path --class-path=. --details=tree --fail-if-no-tests
exit 0
