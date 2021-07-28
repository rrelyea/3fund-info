& dotnet run --project .\src\daily.csproj
& git add -A .\*.html
& git add -A .\*.txt
& git commit -m "price update"
& git push
Copy-Item -Path .\vti -Filter *.html -Destination ..\rrelyea.github.io\3fund -Recurse -Force
Copy-Item -Path .\vtsax -Filter *.html -Destination ..\rrelyea.github.io\3fund -Recurse -Force
Copy-Item -Path .\fzrox -Filter *.html -Destination ..\rrelyea.github.io\3fund -Recurse -Force
& cd ..\rrelyea.github.io
& git add 3fund\vti\*.html
& git add 3fund\vtsax\*.html
& git add 3fund\fzrox\*.html
& git commit -m "price update"
& git push
& cd ..\3fund-info