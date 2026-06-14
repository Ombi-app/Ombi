// ***********************************************************
// This example support/index.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'
import './request.commands';
import './plex-settings.commands';
import './mock-data.commands';
import "cypress-real-events/support";
import "@bahmutov/cy-api/support";

// Ensure every spec starts against a fully set-up Ombi instance.
//
// The wizard feature (cypress/features/01-wizard) intentionally exercises the
// first-run experience and must see a pristine, un-configured app, so we skip
// setup for it. Every other spec used to implicitly rely on the wizard having
// been run first; that hard ordering dependency made the suite fragile (one
// wizard hiccup failed everything downstream) and meant individual specs could
// not be run in isolation. Creating the admin user up-front via the API removes
// that coupling entirely.
before(function () {
  const spec = (Cypress.spec && (Cypress.spec.relative || Cypress.spec.name)) || "";
  if (spec.includes("01-wizard")) {
    return;
  }
  cy.ensureSetup();

  // Start every spec from an empty request state. The suite shares one
  // persistent database and several specs make real, persisted requests, so
  // without this a re-run (or a different spec order) fails spuriously because a
  // show is no longer "not requested". See cy.clearAllRequests for details.
  cy.clearAllRequests();
});

// Alternatively you can use CommonJS syntax:
// require('./commands')
