
Cypress.Commands.add('clearPlexServers', () => {
    cy.request({
        method: 'POST',
        url: '/api/v1/Settings/Plex/',
        body: `{"enable":false,"enableWatchlistImport":false,"monitorAll":false,"installId":"0c5c597d-56ea-4f34-8f59-18d34ec82482","servers":[],"id":2}`,
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
            'Content-Type':"application/json"
        }
    })
})