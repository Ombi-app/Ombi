// ***********************************************
// Enhanced custom commands with better TypeScript support
// ***********************************************

import 'cypress-wait-until';

// Type definitions for custom commands
declare global {
  namespace Cypress {
    interface Chainable {
      ensureSetup(): Chainable<void>;
      landingSettings(enabled: boolean): Chainable<void>;
      loginWithCreds(username: string, password: string): Chainable<void>;
      login(): Chainable<void>;
      removeLogin(): Chainable<void>;
      verifyNotification(text: string): Chainable<void>;
      createUser(username: string, password: string, claims: string[]): Chainable<void>;
      generateUniqueId(): Chainable<string>;
      getByData(selector: string): Chainable<JQuery<HTMLElement>>;
      getByDataLike(selector: string): Chainable<JQuery<HTMLElement>>;
      triggerHover(elements: JQuery<HTMLElement>): Chainable<void>;
      waitForApiResponse(alias: string, timeout?: number): Chainable<void>;
      clearTestData(): Chainable<void>;
      seedTestData(fixture: string): Chainable<void>;
      api(options: any): Chainable<any>;
    }
  }
}

// Idempotently make sure Ombi has finished its first-run setup (i.e. the admin
// user exists and the wizard is marked complete). Historically every spec
// depended on the wizard feature having been run first via the UI - if that run
// failed or was skipped, the whole suite cascaded into failures because the app
// stayed on the wizard page. This command talks directly to the wizard API
// (the same endpoint the UI calls) so any spec can guarantee a usable app on its
// own, regardless of execution order.
//
// The endpoint is [AllowAnonymous] and only succeeds when no local user exists,
// so calling it repeatedly is safe: once the admin is created it simply returns
// "existing user" which we deliberately ignore.
Cypress.Commands.add('ensureSetup', () => {
  cy.request({
    method: 'POST',
    url: '/api/v1/Identity/Wizard',
    body: {
      username: Cypress.env('username'),
      password: Cypress.env('password'),
      usePlexAdminAccount: false,
    },
    failOnStatusCode: false,
  }).then((resp) => {
    cy.log(`ensureSetup: wizard API responded ${resp.status}`);
  });
});

// Enhanced landing page settings command
Cypress.Commands.add("landingSettings", (enabled: boolean) => {
  cy.fixture('login/landingPageSettings').then((settings) => {
    settings.enabled = enabled;
    cy.intercept("GET", "**/Settings/LandingPage", settings).as("landingPageSettings");
  });
});

// Enhanced login with credentials
Cypress.Commands.add('loginWithCreds', (username: string, password: string) => {
  cy.request({
    method: 'POST',
    url: '/api/v1/token',
    body: { username, password },
    failOnStatusCode: false
  }).then((resp) => {
    if (resp.status === 200) {
      window.localStorage.setItem('id_token', resp.body.access_token);
    }
  });
  
  // Log outside of the promise chain
  cy.log(`Login attempt for user: ${username}`);
});

// Enhanced default login
Cypress.Commands.add('login', () => {
  cy.clearLocalStorage();
  cy.clearCookies();
  
  const username = Cypress.env('username');
  const password = Cypress.env('password');
  
  if (!username || !password) {
    throw new Error('Username and password must be set in environment variables');
  }
  
  cy.loginWithCreds(username, password);
});

// Enhanced login removal
Cypress.Commands.add('removeLogin', () => {
  cy.clearLocalStorage();
  cy.clearCookies();
  cy.log('Cleared authentication data');
});

// Enhanced notification verification with better error handling
Cypress.Commands.add('verifyNotification', (text: string) => {
  cy.contains(text, { timeout: 10000 })
    .should('be.visible');
});

// Enhanced user creation with better error handling
Cypress.Commands.add('createUser', (username: string, password: string, claims: string[]) => {
  const token = window.localStorage.getItem('id_token');
  if (!token) {
    throw new Error('No authentication token found. Please login first.');
  }
  
  cy.request({
    method: 'POST',
    url: '/api/v1/identity',
    body: {
      UserName: username,
      Password: password,
      Claims: claims,
    },
    headers: {
      'Authorization': `Bearer ${token}`,
    },
    failOnStatusCode: false
  }).then((resp) => {
    if (resp.status !== 200) {
      // Use console.log instead of cy.log inside promise
      console.log(`Failed to create user ${username}: ${resp.status}`);
    }
  });
  
  // Log outside of the promise chain
  cy.log(`Creating user: ${username}`);
});

// Enhanced unique ID generation
Cypress.Commands.add('generateUniqueId', () => {
  const uniqueSeed = Date.now().toString();
  const id = Cypress._.uniqueId(uniqueSeed);
  cy.wrap(id);
});

// Enhanced data attribute selectors with better typing
Cypress.Commands.add("getByData", (selector: string) => {
  return cy.get(`[data-test="${selector}"]`);
});

Cypress.Commands.add("getByDataLike", (selector: string) => {
  return cy.get(`[data-test*="${selector}"]`);
});

// Enhanced hover trigger with better event handling
Cypress.Commands.add('triggerHover', function(elements: JQuery<HTMLElement>) {
  elements.each((index, element) => {
    const mouseoverEvent = new MouseEvent('mouseover', {
      bubbles: true,
      cancelable: true,
      view: window
    });
    element.dispatchEvent(mouseoverEvent);
  });
});

// New command: Wait for API response with timeout
Cypress.Commands.add('waitForApiResponse', (alias: string, timeout: number = 10000) => {
  cy.wait(`@${alias}`, { timeout });
});

// New command: Clear test data
Cypress.Commands.add('clearTestData', () => {
  cy.clearLocalStorage();
  cy.clearCookies();
  cy.clearAllSessionStorage();
  cy.log('All test data cleared');
});

// New command: Seed test data from fixture
Cypress.Commands.add('seedTestData', (fixture: string) => {
  cy.fixture(fixture).then((data) => {
    // Implementation depends on your seeding strategy
    // Example: cy.request('POST', '/api/v1/test/seed', data);
  });
  
  // Log outside of the promise chain
  cy.log(`Seeding test data from ${fixture}`);
});

  
  