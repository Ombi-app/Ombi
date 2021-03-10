// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add("login", (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add("drag", { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add("dismiss", { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })

Cypress.Commands.add("landingSettings", (enabled) => {
    cy.fixture('login/landingPageSettings').then((settings)  => {
        settings.enabled = enabled;
        cy.intercept("GET", "**/Settings/LandingPage", settings).as("landingPageSettingsDisabled");
      })
})

Cypress.Commands.add('login', (username, password) => {
    cy.request({
        method: 'POST',
        url: '/api/v1/token',
        body: {
            username: username,
            password: password,
        }
    })
        .then((resp) => {
            window.localStorage.setItem('id_token', resp.body.access_token);
        });
  });

  Cypress.Commands.add('login', () => {
    cy.clearLocalStorage();
    cy.request({
        method: 'POST',
        url: '/api/v1/token',
        body: {
            username: Cypress.env('username'),
            password: Cypress.env('password'),
        }
    })
        .then((resp) => {
            window.localStorage.setItem('id_token', resp.body.access_token);
        });
  });

Cypress.Commands.add('removeLogin', () => {
  window.localStorage.removeItem('id_token');
});

Cypress.Commands.add('verifyNotification', (text) => {
    cy.contains(text);
});

Cypress.Commands.add('createUser', (username, password, claims) => {
    cy.request({
        method: 'POST',
        url: '/api/v1/identity',
        body: {
            UserName: username,
            Password: password,
            Claims: claims,
        },
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
    })
})

Cypress.Commands.add('generateUniqueId', () => {
    const uniqueSeed = Date.now().toString();
    const id = Cypress._.uniqueId(uniqueSeed);
    return id;
});

Cypress.Commands.add("getByData", (selector, ...args) => {
    return cy.get(`[data-test=${selector}]`, ...args);
  });

Cypress.Commands.add("getByData", (selector) => {
    return cy.get(`[data-test=${selector}]`);
  });

  Cypress.Commands.add("getByDataLike", (selector) => {
    return cy.get(`[data-test*=${selector}]`);
  });

  Cypress.Commands.add('triggerHover', function(elements) {

      fireEvent(elements, 'mouseover');

  
    function fireEvent(element, event) {
      if (element.fireEvent) {
        element.fireEvent('on' + event);
      } else {
        var evObj = document.createEvent('Events');
  
        evObj.initEvent(event, true, false);
  
        element.dispatchEvent(evObj);
      }
    }
  
  });