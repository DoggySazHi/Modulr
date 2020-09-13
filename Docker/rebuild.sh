#!/bin/bash

dos2unix CompileAndTest.sh

docker build -t modulrjail .

# docker run --rm -v src:/src/files modulrjail SetWithArray.java SetWithArrayTest.java

# docker run -it --entrypoint bash modulrjail
