 #!/bin/bash
 set -eu # 
 
 accountadminuser=nitin.datta@gmail.com
 project=dotnet-helloworld-101
 projectname=dotnet-helloworld
 region=us-east1
 zone=us-east1-b
 billigaccount=0186F7-9C055A-2D43E9
 
 #Create Project
gcloud projects create $project --name=$projectname --labels=type=dotnet-helloworld --set-as-default
# Enable Billing
gcloud alpha billing accounts projects link $project --billing-account=$billigaccount

gcloud config configurations create dotnet-helloworld
gcloud config set account $accountadminuser 
gcloud config set project  $project
gcloud config set compute/region $region
gcloud config set compute/zone $zone

# Enable API's
gcloud services enable sqladmin.googleapis.com
gcloud services enable deploymentmanager.googleapis.com
gcloud services enable compute.googleapis.com
gcloud services enable storage-component.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable servicenetworking.googleapis.com
gcloud services enable vpcaccess.googleapis.com
gcloud services enable appengineflex.googleapis.com
gcloud services enable cloudtasks.googleapis.com
gcloud services enable secretmanager.googleapis.com