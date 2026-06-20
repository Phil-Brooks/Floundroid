cd /d D:\cutechess\match
rem delete backups
del oldself_results.pgn
del oldself.txt
rem create new backups
move self_results.pgn oldself_results.pgn
move self.txt oldself.txt
rem copy latest
del Floundroid.exe
del Floundroid.pdb
del Floundroid2.exe
del Floundroid2.pdb
copy D:\Github\Floundroid\publish\Floundroid.exe
copy D:\Github\Floundroid\publish\Floundroid.pdb
copy Floundroid.exe Floundroid2.exe
copy Floundroid.pdb Floundroid2.pdb


rem @echo off
cutechess-cli.exe -engine name=Floundroid cmd=Floundroid.exe proto=uci stderr=err.txt -engine name=Floundroid2 cmd=Floundroid2.exe proto=uci stderr=err.txt -each tc=30 -openings file=gm2001.bin -games 100 -repeat -concurrency 4 -pgnout self_results.pgn >self.txt 
pause
