// Type definitions for request commands
declare global {
  namespace Cypress {
    interface Chainable {
      requestGenericMovie(): Chainable<void>;
      requestMovie(movieId: number): Chainable<void>;
      requestAllTv(tvId: number): Chainable<any>;
      removeAllMovieRequests(): Chainable<void>;
    }
  }
}

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
    return cy.request({
        method: 'POST',
        url: '/api/v2/requests/tv',
        body: {
            TheMovieDbId: tvId,
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

export {};