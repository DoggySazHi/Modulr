#!/bin/bash

docker build -t cmd .

docker run -v src:/src/files cmd SetWithArray.java SetWithArrayTest.java
