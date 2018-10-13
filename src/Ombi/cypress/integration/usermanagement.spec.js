/// <reference types="Cypress" />

describe('User Management Page', function () {
  beforeEach(function () {
    cy.login('automation', 'password');
    cy.createUser('userToDelete', 'password', [{
      value: "requestmovie",
      Enabled: "true",
    }]);

    cy.createUser('userToEdit', 'password', [{
      value: "requestmovie",
      Enabled: "true",
    }]);

    cy.visit('/usermanagement');
  });

  it('Loads users table', function () {
    cy.contains("User Management");
    cy.contains("Add User To Ombi");
  });

  it('Creates basic user', function () {
    cy.get('[data-test=adduserbtn').click();
    cy.url().should('include', '/user');

    // Setup the form
    cy.get('#username').type("user1");
    cy.get('#alias').type("alias1");
    cy.get('#emailAddress').type("user1@emailaddress.com");
    cy.get('#password').type("password");
    cy.get('#confirmPass').type("password");

    // setup the roles
    cy.contains('Roles').click()
    cy.get('#labelRequestTv').click();
    cy.get('#labelRequestMovie').click();

    // submit user
    cy.get('[data-test=createuserbtn]').click();

    cy.verifyNotification('has been created successfully');

    // Also check if the user is in the table
    cy.contains('alias1');
  });

  it('Tries to create user without roles', function () {
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

  it('Tries to create user when passwords do not match', function () {
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

  it('Delete a user', function () {
    cy.get('#edituserToDelete').click();
    cy.contains('User: userToDelete');
    cy.get('[data-test=deletebtn]').click();
    cy.contains('Are you sure that you want to delete this user?');
    cy.contains('Yes').click();
    cy.verifyNotification('was deleted');
  })


  it('Add request limits to a user', function () {
    cy.get('#edituserToEdit').click();

    cy.contains('Request Limits').click();
    cy.get('#movieRequestLimit').clear().type(2);
    cy.get('#musicRequestLimit').clear().type(3);
    cy.get('#episodeRequestLimit').clear().type(4);

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

  it('Add notification preferences to user', function () {
  
    cy.get('#edituserToEdit').click();

    cy.contains('Notification Preferences').click();
    cy.get('[data-test=Discord]').clear().type("Discord");
    cy.get('[data-test=Pushbullet]').clear().type("Pushbullet");
    cy.get('[data-test=Pushover]').clear().type("Pushover");
    cy.get('[data-test=Telegram]').clear().type("Telegram");
    cy.get('[data-test=Slack]').clear().type("Slack");
    cy.get('[data-test=Mattermost]').clear().type("Mattermost");

    // submit user
    cy.get('[data-test=updatebtn]').click();

    cy.verifyNotification('successfully');

    // Verify that the limits are set 
    cy.get('#edituserToEdit').click();
    cy.contains('Notification Preferences').click();
    cy.get('[data-test=Discord]').should('have.attr', 'ng-reflect-model', "Discord");
    cy.get('[data-test=Pushbullet]').should('have.attr', 'ng-reflect-model', "Pushbullet");
    cy.get('[data-test=Pushover]').should('have.attr', 'ng-reflect-model', "Pushover");
    cy.get('[data-test=Telegram]').should('have.attr', 'ng-reflect-model', "Telegram");
    cy.get('[data-test=Slack]').should('have.attr', 'ng-reflect-model', "Slack");
    cy.get('[data-test=Mattermost]').should('have.attr', 'ng-reflect-model', "Mattermost");

  });

  it('Modify roles', function () {
  
    cy.get('#edituserToEdit').click();

    cy.contains('Roles').click();
    cy.get('#labelRequestMovie').click();
    cy.get('#labelRequestTv').click();

    // submit user
    cy.get('[data-test=updatebtn]').click();

    cy.verifyNotification('successfully');

    // Verify that the limits are set
    cy.get('#edituserToEdit').click();
    cy.contains('Roles').click();
    cy.get('#createRequestMovie').should('have.attr',  'ng-reflect-model', 'true');
    cy.get('#createRequestTv').should('have.attr', 'ng-reflect-model', 'true');
    cy.get('#createDisabled').should('have.attr', 'ng-reflect-model', 'false');

  });

  it('Update local users info', function () {
  
    cy.get('#userDropdown').click();
    cy.get('#updateUserDetails').click();

    cy.url().should('include','/updatedetails');

    cy.get('#emailAddress').clear().type("user11@emailaddress.com");
    cy.get('#currentPassword').type("password");

    cy.get('[data-test=submitbtn]').click();

    cy.verifyNotification('All of your details have now been updated');
  });

  it('Update local users info with bad password', function () {
  
    cy.get('#userDropdown').click();
    cy.get('#updateUserDetails').click();

    cy.url().should('include','/updatedetails');

    cy.get('#emailAddress').clear().type("user11@emailaddress.com");
    cy.get('#currentPassword').type("password32113123123");

    cy.get('[data-test=submitbtn]').click();

    cy.verifyNotification('Your password is incorrect');
  });


});