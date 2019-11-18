$headnode = "localhost"
$cores = 15
if ($cores -le 256) {$count = 20*$cores;}
else {$count = 100*$cores;}
1..10 | % { .\TestClient.exe -h $headnode -m $cores -min 1 -n $count -r 3000 -i 16384 -batch 1 -responsehandler -save "CTQ"
    rename-item -path CTQ.png -newname ('CTQ-' + [DateTime]::NOW.toString().replace('/', '-').replace(':', '-') + '.png')
    .\TestClient.exe -h $headnode -m $cores -min 1 -n $count -r 3000 -i 1024 -batch 1 -responsehandler -save "CTQ"
    rename-item -path CTQ.png -newname ('CTQ-' + [DateTime]::NOW.toString().replace('/', '-').replace(':', '-') + '.png') }