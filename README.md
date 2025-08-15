# Mtr Reporting
Parser for csv output of MTR.

MTR generates one file per run of the program - and generates output with a header in the format:

```
Mtr_Version,Start_Time,Status,Host,Hop,Ip,Loss%,Snt, ,Last,Avg,Best,Wrst,StDev,
MTR.0.95,1755251707,OK,192.168.1.50,1,192.168.0.1,0.00,60,0,0.22,0.26,0.22,0.56,0.05
```

As you can see the second column is seconds since epoch - which needs to be converted to a proper timestamp.

Also if you run MTR several times and pipe the output to the same file so you have a file with the same header multiple times, then the header needs to be stripped.

When this has been done and time has been converted, then its possible to import into a program like Excel and create some nice graphs.