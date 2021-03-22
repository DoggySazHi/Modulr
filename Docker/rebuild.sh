#!/bin/bash

dos2unix CompileAndTest.sh

wget https://github.com/DoggySazHi/Modulr.Stipulator/releases/latest/download/Modulr.Stipulator.jar

docker build -t modulrjail .

# docker run --rm -v src:/src/files modulrjail SetWithArray.java SetWithArrayTest.java

# docker run -it --entrypoint bash modulrjail
