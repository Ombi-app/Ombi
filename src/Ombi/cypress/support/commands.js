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
// -- This is will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })

Cypress.Commands.add('login', (username, password) => {
    cy.clearLocalStorage();
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
Cypress.Commands.add('removeLogin', () => {
    window.localStorage.removeItem('id_token');
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

Cypress.Commands.add('verifyNotification', (text) => {
    cy.get('.ui-growl-title').should('be.visible');
    cy.get('.ui-growl-title').next().contains(text)
});
