backend:
	cd src/Ombi && dotnet watch run -- --host http://*:3577

frontend: 
	cd src/Ombi/ClientApp && yarn start

install-frontend: 
	cd src/Ombi/ClientApp && yarn

install-frontend-tests: 
	cd tests && yarn

frontend-tests: 
	cd tests && npx cypress run

backend-tests:
	cd src/Ombi.Core.Tests && dotnet test