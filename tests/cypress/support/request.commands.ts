
Cypress.Commands.add('requestGenericMovie', () => {
    cy.request({
        method: 'POST',
        url: '/api/v1/request/movie',
        body: {
            TheMovieDbId: 299536
        },
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
    })
})

Cypress.Commands.add('requestMovie', (movieId) => {
    cy.request({
        method: 'POST',
        url: '/api/v1/request/movie',
        body: {
            TheMovieDbId: movieId
        },
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
    })
})

Cypress.Commands.add('requestAllTv', (tvId) => {
    cy.request({
        method: 'POST',
        url: '/api/v1/request/tv',
        body: {
            TvDbId: tvId,
            RequestAll: true
        },
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
    });
})

Cypress.Commands.add('removeAllMovieRequests', () => {
    cy.request({
        method: 'DELETE',
        url: '/api/v1/request/movie/all',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
    });
})