cd /d D:\cutechess\match
rem delete backups
del oldcin_results.pgn
del oldcin.txt
rem create new backups
move cin_results.pgn oldcin_results.pgn
move cin.txt oldcin.txt
rem copy latest
del Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.exe

rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=Cinnamon cmd=cinnamon_2.0_x64-modern.exe proto=uci stderr=err.txt -each tc=30 -openings file=gm2001.bin -games 100 -repeat -concurrency 4 -pgnout cin_results.pgn >cin.txt 
pause

rem update runlog
del D:\Github\Floundroid\runlog.txt
copy cin.txt D:\Github\Floundroid\bat\runlog.txt
pause