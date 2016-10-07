# cdf2csv

Small tool to convert CDF files ([NetCDF](http://www.unidata.ucar.edu/software/netcdf/) text files) to CSV.

```
cdf2csv.exe inputFilePath.cdf
```

## Convert .nc or .nc4 files
This tool can't do the conversion directly. You can use **ncdump** (in the [original NetCDF-C tools](http://www.unidata.ucar.edu/downloads/netcdf/index.jsp)) to convert it to .cdf and then you can use **cdf2csv** already.

```
ncdump inputFilePath.nc > outputFilePath.cdf
```

## Can cdf2csv convert from csv to cdf?
No. The header info is lost in the conversion to csv and can not be reconstructed.

## System requeriments
- Windows with .Net Framework 4
- Linux or macOS with Mono Runtime 4.0