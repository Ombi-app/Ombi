// Test setup and initialization utilities
import './commands';

// Global setup that runs before all tests
beforeEach(() => {
  // Check if application is already set up
  cy.checkApplicationSetup();
});

// Check if the application is already configured
Cypress.Commands.add('checkApplicationSetup', () => {
  cy.log('Checking application setup status...');
  
  // Try to access the application to see if it's already configured
  cy.request({
    method: 'GET',
    url: '/api/v1/status',
    failOnStatusCode: false,
    timeout: 10000
  }).then((response) => {
    if (response.status === 200) {
      // Application is running and configured
      cy.log('Application is already configured, skipping wizard');
      return;
    }
    
    // Check if we're on the wizard page
    cy.visit('/').then(() => {
      cy.url().then((url) => {
        if (url.includes('/wizard') || url.includes('/setup')) {
          cy.log('Wizard detected, running setup...');
          cy.runWizardSetup();
        } else {
          cy.log('Application appears to be configured');
        }
      });
    });
  });
});

// Run the wizard setup
Cypress.Commands.add('runWizardSetup', () => {
  cy.log('Running wizard setup...');
  
  // Import wizard page object
  cy.fixture('wizard-config').then((config) => {
    // Run through wizard steps
    cy.get('[data-test="wizard-welcome-next"]').click();
    cy.get('[data-test="wizard-database-next"]').click();
    cy.get('[data-test="wizard-media-server-next"]').click();
    
    // Create local user
    cy.get('[data-test="wizard-username"]').type(Cypress.env('username'));
    cy.get('[data-test="wizard-password"]').type(Cypress.env('password'));
    cy.get('[data-test="wizard-user-next"]').click();
    
    // Complete configuration
    cy.get('[data-test="wizard-config-next"]').click();
    cy.get('[data-test="wizard-finish"]').click();
    
    // Wait for setup to complete
    cy.url().should('not.include', '/wizard');
    cy.log('Wizard setup completed successfully');
  });
});

// Check if we need to run setup for this test run
Cypress.Commands.add('ensureApplicationSetup', () => {
  // This command can be called explicitly in tests that need setup
  cy.checkApplicationSetup();
});

// Add to global commands interface
declare global {
  namespace Cypress {
    interface Chainable {
      checkApplicationSetup(): Chainable<void>;
      runWizardSetup(): Chainable<void>;
      ensureApplicationSetup(): Chainable<void>;
    }
  }
}


