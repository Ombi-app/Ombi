
Cypress.Commands.add('addMock', (mapping) => {
    cy.request({
        method: 'POST',
        url: 'http://localhost:32400/__admin/mappings',
        body: mapping
    })
})

Cypress.Commands.add('clearMocks', () => {
    cy.request({
        method: 'DELETE',
        url: 'http://localhost:32400/__admin/mappings'
    })
})