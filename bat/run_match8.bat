cd /d D:\cutechess\match
rem delete backups
del oldbaseline_results8.pgn
del oldlog8.txt
rem create new backups
move baseline_results8.pgn oldbaseline_results8.pgn
move log8.txt oldlog8.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=TSCP cmd=tscp181.exe proto=xboard -each tc=30 -openings file=gm2001.bin -games 8 -repeat -concurrency 4 -pgnout baseline_results8.pgn -debug all>log8.txt 
pause
