/// <reference types="Cypress" />

describe('User Management Page', function () {
  beforeEach(function () {
    cy.login('automation', 'password');
    cy.createUser('userToDelete', 'password', [{
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


  it.only('Creates user with request limits', function () {
    cy.get('[data-test=adduserbtn').click();
    cy.url().should('include', '/user');

    // Setup the form
    cy.get('#username').type("user2");
    cy.get('#alias').type("alias2");
    cy.get('#emailAddress').type("user2@emailaddress.com");
    cy.get('#password').type("password");
    cy.get('#confirmPass').type("password");

    // setup the roles
    cy.contains('Roles').click()
    cy.get('#labelRequestMovie').click();

    cy.contains('Request Limits').click();
    cy.get('#movieRequestLimit').clear().type(2);
    cy.get('#musicRequestLimit').clear().type(3);
    cy.get('#episodeRequestLimit').clear().type(4);

    // submit user
    cy.get('[data-test=createuserbtn]').click();

    cy.verifyNotification('has been updated successfully');

    // Verify that the limits are set
    cy.get('#edituser2').click();
    cy.contains('Request Limits').click();
    cy.get('#movieRequestLimit').should('eq', 2);
    cy.get('#musicRequestLimit').should('eq', 3);
    cy.get('#tvRequestLimit').should('eq', 4);

  });


});