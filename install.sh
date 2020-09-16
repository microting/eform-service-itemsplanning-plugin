#!/bin/bash

if [ ! -d "/var/www/microting/eform-service-items-group-planning-plugin" ]; then
  cd /var/www/microting
  su ubuntu -c \
  "git clone https://github.com/microting/eform-service-items-group-planning-plugin.git -b stable"
fi

cd /var/www/microting/eform-service-items-group-planning-plugin
git pull
su ubuntu -c \
"dotnet restore ServiceItemsGroupPlanningPlugin.sln"

echo "################## START GITVERSION ##################"
export GITVERSION=`git describe --abbrev=0 --tags | cut -d "v" -f 2`
echo $GITVERSION
echo "################## END GITVERSION ##################"
su ubuntu -c \
"dotnet publish ServiceItemsGroupPlanningPlugin.sln -o out /p:Version=$GITVERSION --runtime linux-x64 --configuration Release"

su ubuntu -c \
"mkdir -p /var/www/microting/eform-debian-service/MicrotingService/out/Plugins/"

rm -fR /var/www/microting/eform-debian-service/MicrotingService/out/Plugins/ServiceItemsGroupPlanningPlugin

su ubuntu -c \
"cp -av /var/www/microting/eform-service-items-group-planning-plugin/out /var/www/microting/eform-debian-service/MicrotingService/out/Plugins/ServiceItemsGroupPlanningPlugin"
/rabbitmqadmin declare queue name=eform-service-itemsplanning-plugin durable=true
