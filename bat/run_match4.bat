cd /d D:\cutechess\match
rem delete backups
del oldbaseline_results4.pgn
del oldlog4.txt
rem create new backups
move baseline_results4.pgn oldbaseline_results4.pgn
move log4.txt oldlog4.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=TSCP cmd=tscp181.exe proto=xboard -each tc=30 -openings file=gm2001.bin -games 4 -repeat -concurrency 4 -pgnout baseline_results4.pgn -debug all>log4.txt 
pause