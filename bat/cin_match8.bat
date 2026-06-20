cd /d D:\cutechess\match
rem delete backups
del oldcin_results8.pgn
del oldcin8.txt
rem create new backups
move cin_results8.pgn oldcin_results8.pgn
move cin8.txt oldcin8.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=Cinnamon cmd=cinnamon_2.0_x64-modern.exe proto=uci stderr=err.txt -each tc=30 -openings file=gm2001.bin -games 8 -repeat -concurrency 4 -pgnout cin_results8.pgn >cin8.txt 
pause
