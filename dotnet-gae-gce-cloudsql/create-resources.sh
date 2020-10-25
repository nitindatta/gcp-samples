 #!/bin/bash
set -eu # Create Configuration
 region=us-east1
 zone=us-east1-b
 # AppEngine Configurations
 appname=dotnet-helloworld
 project=dotnet-helloworld-103
 # Db Configurations
 dbname=dotnet-helloworld
 username=demouser
 instancepassword=$1
 dbpassword=$2
 dbinstanceid=dotnet-helloworld-101
 dbversion=SQLSERVER_2017_EXPRESS

# Create Queue For Async Messages
 queueid=dotnet-helloworld
 secretkey=SqlUserSecret

# Create Windows Instance
windowsinstance=dotnet-helloworld-windows-iis-100
windowsimage=win-iis-dotnet-helloworld
# Networking
vpc=dotnet-helloworld-100

 ###############################Set up Resources####################################

# Create VPC Network
if gcloud compute networks describe $vpc | grep --line-buffered 'name:'; then
    echo "VPC is with this id already create in this project"
else
    gcloud compute networks create $vpc \
        --subnet-mode=custom \
        --bgp-routing-mode=regional
fi

subnet="$region-$vpc-subnetwork1"

if gcloud compute networks subnets describe $subnet | grep --line-buffered 'name:'; then
    echo "Subnet: $subnet is with this id already create in this project"
else
    gcloud compute networks subnets create $subnet \
    --network=$vpc \
    --range=192.168.0.0/16 \
    --enable-flow-logs \
    --enable-private-ip-google-access \
    --region=$region
fi

# # Route for windows activation
# if  gcloud compute routes describe windows-activation | grep --line-buffered 'name:'; then
#     echo "windows-activation is with this id already create in this project"
# else
#     gcloud compute routes create windows-activation \
#         --destination-range=35.190.247.13/32 \
#         --network=$vpc \
#         --next-hop-gateway=default-internet-gateway
# fi
# Create Reserved Address Range for allowing access to cloud sql service
if  gcloud compute addresses describe google-managed-services-$vpc --project=$project --global | grep --line-buffered 'name:' ; then
    echo "Reserved Address Range Already created"

else
    echo "Creating Reserved Address Range: google-managed-services-$vpc "
    gcloud compute addresses create google-managed-services-$vpc \
        --global \
        --purpose=VPC_PEERING \
        --prefix-length=16 \
        --description="peering range for Google" \
        --network=$vpc \
        --project=$project
fi

if  gcloud services vpc-peerings list --network $vpc | grep --line-buffered $vpc ; then
    echo "VPC Peering Already done"

else
    gcloud services vpc-peerings connect \
        --service=servicenetworking.googleapis.com \
        --ranges=google-managed-services-$vpc \
        --network=$vpc \
        --project=$project
fi

#  # Create SQL Instance
 if gcloud sql instances describe $dbinstanceid | grep --line-buffered 'name:'; then
    echo "Sql Instance Already Exists"
else
    gcloud beta sql instances create $dbinstanceid \
    --tier=db-custom-1-3840 \
    --database-version=$dbversion \
    --root-password=$instancepassword \
    --storage-size=10 \
    --zone=$zone \
    --no-backup \
    --require-ssl \
    --network=$vpc
    --project=$project
fi

# # # Create SQL Database Will be created by app
# # #  if gcloud sql databases describe $dbname -i $dbinstanceid | grep --line-buffered 'name:'; then
# # #     echo "Sql Database Already Exists"
# # # else
# # #     gcloud sql databases create $dbname \
# # #     --instance=$dbinstanceid
# # # fi

if gcloud sql users list -i $dbinstanceid --filter NAME=$username | grep --line-buffered $username; then
    echo "User Already Exists"
else
    gcloud sql users create $username \
    --instance=$dbinstanceid
fi

# gcloud sql users set-password $username --instance=$dbinstanceid --password=$dbpassword

# Create AppEngineFlexible App
if gcloud sql databases describe $dbname -i $dbinstanceid | grep --line-buffered 'name:'; then
    echo "App Engine is already create in this project"
else
    gcloud app create --region $region
fi

# # Create Queue
if gcloud tasks queues describe $queueid | grep --line-buffered 'name:'; then
    echo "Queue is with this id already create in this project"
else
    gcloud tasks queues create $queueid
fi



# # # Create Vault to keep credentials
printf '{"username":"%s","password":"%s"}\n' "$username" "$dbpassword" > temp.txt
gcloud secrets create $secretkey --data-file=temp.txt
#sleep 2
rm temp.txt

# # # Only receive traffic from LB
# # #gcloud beta app services update default --ingress internal-and-cloud-load-balancing

# gcloud sql instances describe $dbinstanceid | grep --line-buffered 'connectionName:'


# if  gcloud compute firewall-rules describe rdp-rule | grep --line-buffered 'ERROR:'; then
#   echo "rdp rule is with this id already create in this project"
# else  
#     gcloud compute firewall-rules create rdp-rule \
#     --allow=tcp:3389 \
#     --source-ranges=0.0.0.0/0 \
#     --network=$vpc \
#     --target-tags=rdp-tag
#     --priority=1005
# fi

# if  gcloud compute firewall-rules describe web-rule  | grep --line-buffered 'ERROR:'; then
#     echo "web-rule is with this id already create in this project"

# else
#     gcloud compute firewall-rules create web-rule \
#     --allow=tcp:80 \
#     --source-ranges=0.0.0.0/0 \
#     --network=$vpc \
#     --target-tags=www-tag
#     ----priority=1006
# fi
# rule to allow loadbalancer health check
# gcloud compute firewall-rules create fw-allow-health-check \
#     --network=default \
#     --action=allow \
#     --direction=ingress \
#     --source-ranges=130.211.0.0/22,35.191.0.0/16 \
#     --target-tags=allow-health-check \
#     --rules=tcp
#     --priority=1001



# if  gcloud compute instances list | grep --line-buffered $windowsinstance; then
#     echo "instance:$windowsinstance is with this id already created in this project"

# else
#     echo "creating image"
#     gcloud compute instances create $windowsinstance \
#         --image=$windowsimage \
#         --image-project=$project \
#         --machine-type=e2-small \
#         --boot-disk-size=50 \
#         --boot-disk-type pd-ssd \
#         --zone=$zone \
#         --subnet=$subnet \
#         --network=$vpc \
#         --tags=rdp-tag,www-tag,allow-health-check
# fi
# Create Image Instance 
#  gcloud compute instances delete dotnet-helloworld-windows-iis-100 --keep-disks boot
 
#  gcloud compute images create win-iis-dotnet-helloworld --source-disk  dotnet-helloworld-windows-iis-100 --family win-iis-dotnet-helloworld-family  --source-disk-zone us-east1-b

# # # Create Windows Instance Template
lbbackendtemplate=lb-backend-template
gcloud compute instance-templates create $lbbackendtemplate \
   --region=$region \
   --network=$vpc \
   --subnet=$subnet \
   --tags=rdp-tag,www-tag,allow-health-check \
   --image=$windowsimage \
   --machine-type=e2-small \
   --image-project=$project \
   --scopes=cloud-platform \
   --boot-disk-size=50 \
   --boot-disk-type pd-ssd

#Create Managed Instance Group From Template
instancegroup=lb-backend-dotnet-helloworld

gcloud compute instance-groups managed create $instancegroup \
   --template=$lbbackendtemplate --size=1 --zone=$zone

#Adding Named Ports to instance group
gcloud compute instance-groups unmanaged set-named-ports $instancegroup \
    --named-ports http:80 \
    --zone $zone

#Create Health Check
gcloud compute health-checks create http http-basic-check \
    --port 80
#Create BackendServce
backendservice=web-backend-dotnet-service
gcloud compute backend-services create $backendservice \
    --protocol=HTTP \
    --port-name=http \
    --health-checks=http-basic-check \
    --global

#Attach Back End Service With Instance Group
gcloud compute backend-services add-backend $backendservice \
    --instance-group=$instancegroup \
    --instance-group-zone=$zone \
    --global
    
# # Set password
# gcloud compute reset-windows-password $windowsinstance


