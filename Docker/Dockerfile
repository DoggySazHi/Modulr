FROM openjdk:14
COPY junit-platform-console-standalone-1.6.2.jar /src/
COPY CompileAndTest.sh /src/
COPY security.policy /src/
WORKDIR /src
ENTRYPOINT ["/src/CompileAndTest.sh"]
