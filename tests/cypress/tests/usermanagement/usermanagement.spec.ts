
describe('User Management Page', () => {
    beforeEach(() => {
      cy.login();
      cy.createUser('userToDelete', 'password', [{
        value: "requestmovie",
        Enabled: "true",
      }]);

      cy.createUser('userToEdit', 'password', [{
        value: "disabled",
        Enabled: "true",
      }]);

      cy.visit('/usermanagement');
    });

    it('Loads users table', () => {
      cy.contains("Users");
      cy.contains("Add User To Ombi");
    });

    it('Creates basic user', () => {
      cy.get('[data-test=adduserbtn').click();
      cy.url().should('include', '/user');

      cy.generateUniqueId().then(username => {
        // Setup the form
        cy.get('#username').type(username);
        cy.get('#alias').type("alias1");
        cy.get('#emailAddress').type(username + "@emailaddress.com");
        cy.get('#password').type("password");
        cy.get('#confirmPass').type("password");

        // setup the roles
        cy.contains('Roles').click()
        cy.get('#roleRequestTv').click();
        cy.get('#roleRequestMovie').click();

        // submit user
        cy.get('[data-test=createuserbtn]').click();

        cy.verifyNotification('has been created successfully');

        // Also check if the user is in the table
        cy.contains(username);
        });
    });

    it('Tries to create user without roles', () => {
      cy.get('[data-test=adduserbtn').click();
      cy.url().should('include', '/user');

      // Setup the form
      cy.get('#username').type("user1");
      cy.get('#alias').type("alias1");
      cy.get('#emailAddress').type("user1@emailaddress.com");
      cy.get('#password').type("password");
      cy.get('#confirmPass').type("password");

      // submit user
      cy.get('[data-test=createuserbtn]').click();

      cy.verifyNotification('Please assign a role');

    });

    it('Tries to create user when passwords do not match', () => {
      cy.get('[data-test=adduserbtn').click();
      cy.url().should('include', '/user');

      // Setup the form
      cy.get('#username').type("user1");
      cy.get('#alias').type("alias1");
      cy.get('#emailAddress').type("user1@emailaddress.com");
      cy.get('#password').type("password");
      cy.get('#confirmPass').type("pass22word");

      // submit user
      cy.get('[data-test=createuserbtn]').click();

      cy.verifyNotification('Passwords do not match');
    });

    it('Delete a user', () => {
      cy.get('#edituserToDelete').click();
      cy.get('#username').should('have.value', 'userToDelete');
      cy.get('[data-test=deletebtn]').click();
      cy.verifyNotification('The user userToDelete was deleted');
    })


    it('Add request limits to a user', () => {
      cy.get('#edituserToEdit').click();

      cy.contains('Request Limits').click();
      cy.get('#movieRequestLimit').clear().type('2');
      cy.get('#musicRequestLimit').clear().type('3');
      cy.get('#episodeRequestLimit').clear().type('4');

      // submit user
      cy.get('[data-test=updatebtn]').click();

      cy.verifyNotification('successfully');

      // Verify that the limits are set
      cy.get('#edituserToEdit').click();
      cy.contains('Request Limits').click();
      cy.get('#movieRequestLimit').should('have.attr', 'ng-reflect-model', '2')
      cy.get('#musicRequestLimit').should('have.attr', 'ng-reflect-model', '3')
      cy.get('#episodeRequestLimit').should('have.attr', 'ng-reflect-model', '4')

    });

    it('Add notification preferences to user', () => {

      cy.get('#edituserToEdit').click();

      cy.contains('Notification Preferences').click();
      cy.get('#Discord').clear().type("Discord");
      cy.get('#Pushbullet').clear().type("Pushbullet");
      cy.get('#Pushover').clear().type("Pushover");
      cy.get('#Telegram').clear().type("Telegram");
      cy.get('#Slack').clear().type("Slack");
      cy.get('#Mattermost').clear().type("Mattermost");
      cy.get('#Gotify').clear().type("Gotify");
      cy.get('#WhatsApp').clear().type("Whatsapp");

      // submit user
      cy.get('[data-test=updatebtn]').click();

      cy.verifyNotification('successfully');

      // Verify that the limits are set 
      cy.get('#edituserToEdit').click();
      cy.contains('Notification Preferences').click();
      cy.get('#Discord').should('have.attr', 'ng-reflect-model', "Discord");
      cy.get('#Pushbullet').should('have.attr', 'ng-reflect-model', "Pushbullet");
      cy.get('#Pushover').should('have.attr', 'ng-reflect-model', "Pushover");
      cy.get('#Telegram').should('have.attr', 'ng-reflect-model', "Telegram");
      cy.get('#Slack').should('have.attr', 'ng-reflect-model', "Slack");
      cy.get('#Mattermost').should('have.attr', 'ng-reflect-model', "Mattermost");
      cy.get('#Gotify').should('have.attr', 'ng-reflect-model', "Gotify");
      cy.get('#WhatsApp').should('have.attr', 'ng-reflect-model', "Whatsapp");

    });

    it('Modify roles', () => {

      cy.get('#edituserToEdit').click();

      cy.contains('Roles').click();
      cy.get('#rolePowerUser').click();

      // submit user
      cy.get('[data-test=updatebtn]').click();

      cy.verifyNotification('successfully');

      cy.get('#edituserToEdit').click();
      cy.get('#rolePowerUser').should('have.attr',  'ng-reflect-model', 'true');
    });
  });