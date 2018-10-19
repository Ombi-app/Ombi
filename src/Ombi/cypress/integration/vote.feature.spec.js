/// <reference types="Cypress" />

describe('Voting Feature', function () {
  beforeEach(function () {
    cy.login('automation', 'password').then(() => {

      cy.removeAllMovieRequests();

      cy.createUser('basicUser', 'password', [{
        value: "requestmovie",
        Enabled: "true",
      }, {
        value: "requesttv",
        Enabled: "true",
      }, {
        value: "requestmusic",
        Enabled: "true",
      },
      ]);

      cy.createUser('basicUser2', 'password', [{
        value: "requestmovie",
        Enabled: "true",
      }, {
        value: "requesttv",
        Enabled: "true",
      }, {
        value: "requestmusic",
        Enabled: "true",
      },
      ]);

      // Enable voting
      cy.request({
        method: 'POST',
        url: '/api/v1/Settings/vote',
        body: {
          Enabled: true,
          MovieVoteMax: 2,
          MusicVoteMax: 2,
          TvShowVoteMax: 2,
        },
        headers: {
          'Authorization': 'Bearer ' + window.localStorage.getItem('id_token'),
        }
      });

      // Login as regular user
      cy.login('basicUser', 'password').then(() => {

      cy.visit('/vote');
      });

    });
  });

  ///
  /// Make sure we can load the page
  ///
  it('Loads votes page', function () {
    // cy.login('basicUser','password');
    cy.contains("Vote");
  });

  ///
  /// Make sure that when we request a movie it automatically get's upvoted
  ///
  it('Request Movie automatically upvotes when I am the requestor', function () {
    cy.requestMovie(335983).then(() => {
      cy.visit('/vote');
      cy.get('#completedVotes').click();
      cy.contains('Venom').should('have.attr', 'data-test').then(($id) => {
        cy.get('#' + $id + 'upvote').should('have.attr', 'disabled');
        cy.get('#' + $id + 'downvote').should('not.have.attr', 'disabled');
      });
    });
  });

  ///
  /// Make sure that when we request a tv show it automatically get's upvoted
  ///
  it('Request TV automatically upvotes when I am the requestor', function () {
    cy.requestAllTv(305288).then(() => {
      cy.visit('/vote');
      cy.get('#completedVotes').click();
      cy.contains('Stranger Things').should('have.attr', 'data-test').then(($id) => {
        cy.get('#' + $id + 'upvote').should('have.attr', 'disabled');
        cy.get('#' + $id + 'downvote').should('not.have.attr', 'disabled');
      });
    });
  });

  ///
  /// Upvotes a movie with a different user, the votes should eq 2
  /// Meaning it should be approved now
  ///
  it.only('Upvote Movie to be approved', function () {
    cy.login('basicUser2', 'password').then(() => {
      cy.requestMovie(439079).then(() => {
        cy.login('basicUser', 'password').then(() => {

          cy.visit('/vote');
          cy.contains('The Nun').should('have.attr', 'data-test').then(($id) => {
            cy.get('#' + $id + 'upvote').click();
            cy.verifyNotification('Voted!');

            // Verify it's in the completed panel
            cy.get('#completedVotes').click(); cy.contains('The Nun').should('have.attr', 'data-test').then(($id) => {
              cy.get('#' + $id + 'upvote').should('have.attr', 'disabled');
              cy.get('#' + $id + 'downvote').should('not.have.attr', 'disabled');
            });
          });
        });
      });
    });
  });

  it.only('Downvote Movie', function () {
    cy.login('basicUser2', 'password').then(() => {
      cy.requestMovie(439079).then(() => {
        cy.login('basicUser', 'password').then(() => {

          cy.visit('/vote');
          cy.contains('The Nun').should('have.attr', 'data-test').then(($id) => {
            cy.get('#' + $id + 'downvote').click();
            cy.verifyNotification('Voted!');

            // Verify it's in the completed panel
            cy.get('#completedVotes').click(); cy.contains('The Nun').should('have.attr', 'data-test').then(($id) => {
              cy.get('#' + $id + 'upvote').should('not.have.attr', 'disabled');
              cy.get('#' + $id + 'downvote').should('have.attr', 'disabled');
            });
          });
        });
      });
    });
  });


});