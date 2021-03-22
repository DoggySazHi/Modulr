FROM openjdk:14
COPY Modulr.Stipulator.jar /src/
COPY CompileAndTest.sh /src/
COPY security.policy /src/
WORKDIR /src
ENTRYPOINT ["/src/CompileAndTest.sh"]
