/// <reference types="Cypress" />

describe('Voting Feature', function () {
  beforeEach(function () {
    cy.login('automation', 'password').then(() => {


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

      // Login as the regular user now
      cy.clearLocalStorage();

      cy.login('basicUser', 'password').then(() => {

        cy.visit('/vote');
      });

    });
  });

  it('Loads votes page', function () {
    // cy.login('basicUser','password');
    cy.contains("Vote");
  });

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


});