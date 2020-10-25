 #!/bin/bash
 set -eu # 
 
 accountadminuser=nitin.datta@spektragcp.com
 project=dotnet-helloworld-102
 projectname=dotnet-helloworld
 region=us-east1
 zone=us-east1-b
 #billigaccount=not required linked to org
 organization=1050950779650
 
 #Create Project
gcloud projects create $project --name=$projectname --labels=type=dotnet-helloworld --organization=$organization --set-as-default
# Enable Billing (not required org admin will do)
#gcloud alpha billing accounts projects link $project --billing-account=$billigaccount

gcloud config configurations create dotnet-helloworld-spectra
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