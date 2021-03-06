#!/bin/bash

success=true
randomkey=$(tr -dc 'A-F0-9' < /dev/urandom | head -c6)

cp /src/files/* .

for var in "$@" ; do

    # Skip non-java files (ex. if you require a txt file in the tester)
    if [[ $var != *.java ]];
    then
        continue
    fi
    
    echo "$randomkey ========== BEGIN COMPILING $var =========="
    javac -cp *:. -Xlint:all -Xmaxwarns 100 $var
    if [ $? != 0 ]; # If it's a non-zero exit code (aka bad compilation)
    then
        echo "!! ======= FAILED COMPILATION!!! ======= !!"
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
# ulimit -v 98304
# time
# ulimit -a
echo "$randomkey ========== BEGIN TEST =========="
# Delete all java files, recursively, so a program can't try to read the tester!
# (unless they somehow decompile the class on the fly)
shopt -s globstar
rm -r -- **/*.java
java -Xmx64M -cp *:. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time
echo "$randomkey ========== END TEST =========="
exit 0
