cd /d D:\cutechess\match
rem delete backups
del oldhal_results8.pgn
del oldhal8.txt
rem create new backups
move hal_results8.pgn oldhal_results8.pgn
move hal8.txt oldhal8.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=Halogen6.0 cmd=Halogen6-x64-popcnt.exe proto=uci stderr=err.txt -each tc=30 -openings file=gm2001.bin -games 8 -repeat -concurrency 4 -pgnout hal_results8.pgn -debug all>hal8.txt 
pause
