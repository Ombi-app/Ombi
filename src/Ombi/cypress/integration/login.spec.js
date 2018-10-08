/// <reference types="Cypress" />

describe('Login Page', function () {
  it('Invalid Password', function () {
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#inputEmail').type('automation');
    cy.get('#inputPassword').type('incorrectpw');

    cy.get('[data-test=signinbtn]').click();
    cy.verifyNotification('Incorrect username');
  });

  it('Invalid Username', function () {
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#inputEmail').type('bad username');
    cy.get('#inputPassword').type('incorrectpw');

    cy.get('[data-test=signinbtn]').click();
    cy.verifyNotification('Incorrect username');
  });

  it('Correct Login', function () {
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#inputEmail').type('automation');
    cy.get('#inputPassword').type('password');

    cy.get('[data-test=signinbtn]').click();
    cy.url().should('include', '/search')
  });
})