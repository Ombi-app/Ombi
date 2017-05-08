SERVICE="Ombi"
# change directory to location of project.json
pushd ./ 
# run dotnet publish, specify release build
dotnet publish -c Release
# equivalent to cd .. (go back to previous directory)
#popd
# Create a docker image tagged with the name of the project:latest
docker build -t "$SERVICE":latest .
# Check to see if this container exists.
CONTAINER=`docker ps --all | grep "$SERVICE"`
# if it doesn't, then just run this.
if [ -z "$CONTAINER" ]; then
  docker run -i -p 8000:5000 --name $SERVICE -t $SERVICE:latest
# if it does exist; nuke it and then run the new one
else
  docker rm $SERVICE
  docker run -i -p 8000:5000 --name $SERVICE -t $SERVICE:latest
fi

read -p "Press enter to continue"