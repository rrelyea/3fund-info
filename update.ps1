& dotnet run --project .\src\daily.csproj
& git add -A perf\*.html
& git commit -m "price update"
& git push
copy-item perf\*.html ..\rrelyea.github.io\3fund\ -force
& cd ..\rrelyea.github.io
& git add 3fund\*.html
& git commit -m "price update"
& git push
& cd ..\3fund-info