& dotnet run --project .\src\daily.csproj
& git add -A perf\*
& git commit -m "price update"
& git push
Copy-Item -Path perf\ -Filter *.html -Destination ..\rrelyea.github.io\3fund\ -Recurse
& cd ..\rrelyea.github.io
& git add ..\3fund\*.html
& git commit -m "price update"
& git push
& cd ..\3fund-info