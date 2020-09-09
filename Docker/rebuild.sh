#!/bin/bash

docker build -t modulrjail .

docker run --rm -v src:/src/files modulrjail SetWithArray.java SetWithArrayTest.java
