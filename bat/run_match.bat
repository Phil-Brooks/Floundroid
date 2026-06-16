cd /d D:\cutechess\match
rem delete backups
del oldbaseline_results.pgn
del oldlog.txt
rem create new backups
move baseline_results.pgn oldbaseline_results.pgn
move log.txt oldlog.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=TSCP cmd=tscp181.exe proto=xboard -each tc=60/1+1 -openings file=gm2001.bin -games 100 -repeat -concurrency 4 -pgnout baseline_results.pgn>log.txt 
pause