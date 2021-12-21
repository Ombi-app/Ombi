backend:
	cd src/Ombi && dotnet watch run -- --host http://*:3577

frontend: 
	cd src/Ombi/ClientApp && yarn start

install-frontend: 
	cd src/Ombi/ClientApp && yarn

install-tests: 
	cd tests && yarn

tests: 
	cd tests && npx cypress open
