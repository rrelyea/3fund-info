msbuild src\daily.csproj -p:configuration=release -v:m
& .\src\bin\release\net5.0\daily.exe
& git add -A perf\*
& git commit -m "price update"
& git push