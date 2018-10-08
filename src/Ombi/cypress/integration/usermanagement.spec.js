/// <reference types="Cypress" />

describe('User Management Page', function () {
  beforeEach(function () {
    cy.request({
      method: 'POST',
      url: 'http://localhost:3577/api/v1/token',
      body: {
          username: 'automation',
          password: 'password',
      }
    })
      .then((resp) => {
        window.localStorage.setItem('id_token', resp.body.access_token)
      });
      
    cy.visit('/usermanagement');
  });

  it('Loads users table', function () {
    cy.contains("User Management");
    cy.contains("Add User To Ombi");
  });

  it.only('Creates basic user', function(){
    cy.get('[data-test=adduserbtn').click();
    cy.url().should('include','/user');

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
    
    cy.get('.ui-growl-title').should('be.visible');
    cy.get('.ui-growl-title').next().contains('has been created successfully')
  });
  
})