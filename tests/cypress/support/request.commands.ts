// Type definitions for request commands
declare global {
  namespace Cypress {
    interface Chainable {
      requestGenericMovie(): Chainable<void>;
      requestMovie(movieId: number): Chainable<void>;
      requestAllTv(tvId: number): Chainable<any>;
      removeAllMovieRequests(): Chainable<void>;
      clearAllRequests(): Chainable<void>;
    }
  }
}

// Remove every existing movie and TV request so each spec starts from a known,
// empty request state.
//
// The suite runs against a single, persistent database and several specs make
// *real* requests that survive the run (e.g. cy.requestMovie / cy.requestAllTv).
// Without this, re-running the suite (or running specs in a different order)
// fails spuriously: a show another spec already requested is no longer in the
// "not requested" state these specs assume. Clearing up-front makes the suite
// idempotent and order-independent.
//
// This authenticates as the admin directly (it runs before any UI login, so it
// cannot rely on a token in localStorage) and is intentionally tolerant of
// failures - if the admin doesn't exist yet or an individual delete fails there
// is simply nothing to clean up, and we must not fail the whole spec here.
Cypress.Commands.add('clearAllRequests', () => {
    const username = Cypress.env('username');
    const password = Cypress.env('password');

    cy.request({
        method: 'POST',
        url: '/api/v1/token',
        body: { username, password },
        failOnStatusCode: false,
    }).then((tokenResp) => {
        const token = tokenResp.status === 200 ? tokenResp.body?.access_token : undefined;
        if (!token) {
            return;
        }
        const headers = { Authorization: `Bearer ${token}` };

        // Movies have a bulk-delete endpoint.
        cy.request({
            method: 'DELETE',
            url: '/api/v1/request/movie/all',
            headers,
            failOnStatusCode: false,
        });

        // TV has no bulk delete, so remove each parent request individually.
        cy.request({
            method: 'GET',
            url: '/api/v1/request/tv',
            headers,
            failOnStatusCode: false,
        }).then((tvResp) => {
            const requests: Array<{ id: number }> = Array.isArray(tvResp.body) ? tvResp.body : [];
            requests.forEach((req) => {
                cy.request({
                    method: 'DELETE',
                    url: `/api/v1/request/tv/${req.id}`,
                    headers,
                    failOnStatusCode: false,
                });
            });
        });
    });
});

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