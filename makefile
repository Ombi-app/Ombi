run-backend:
	cd src/Ombi && dotnet watch run -- --host http://*:3577

run-frontend: 
	cd src/Ombi/ClientApp && yarn start

install-frontend: 
	cd src/Ombi/ClientApp && yarn

install-frontend-tests: 
	cd tests && yarn

run-frontend-tests: 
	cd tests && npx cypress run

run-backend-tests:
	cd src/Ombi.Core.Tests && dotnet test