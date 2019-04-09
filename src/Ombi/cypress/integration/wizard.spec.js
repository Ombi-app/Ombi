/// <reference types="Cypress" />

describe('Wizard Setup Tests', function() {
    it('Setup Wizard User', function() {
      cy.visit('/');
      cy.url().should('include', 'Wizard')

      cy.get('[data-test=nextbtn]').click();
        
      // Media server page
      cy.contains('Please choose your media server');
      cy.get('[data-test=skipbtn]').click();

      // Create user
      cy.contains('Create the Admin account');
      cy.get('#adminUsername').type('automation');
      cy.get('#adminPassword').type('password');

      // Submit user
      cy.get('[data-test=createuserbtn]').click();
      cy.contains('Sign in');
    })
  })