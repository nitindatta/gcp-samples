 #!/bin/bash
 set -eu 
 dotnet publish
 gcloud beta app deploy ./bin/Debug/netcoreapp3.1/publish/app.yml

 gcloud beta app deploy cron.yml