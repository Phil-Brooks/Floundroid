cd /d D:\cutechess\match
rem delete backups
del oldhal_results.pgn
del oldhal.txt
rem create new backups
move hal_results.pgn oldhal_results.pgn
move hal.txt oldhal.txt
rem copy latest
del Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.exe

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=Halogen6.0 cmd=Halogen6-x64-popcnt.exe proto=uci stderr=err.txt -each tc=30 -openings file=gm2001.bin -games 100 -repeat -concurrency 4 -pgnout hal_results.pgn >hal.txt 
pause

rem update runlog
del D:\Github\Floundroid\runlog.txt
copy hal.txt D:\Github\Floundroid\bat\runlog.txt
pause