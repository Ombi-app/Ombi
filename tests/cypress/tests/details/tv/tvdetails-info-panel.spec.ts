describe('TV Details Information Panel',() => {
    beforeEach(() => {
        cy.login();
    });

    it('Status should be ended',() => {
        cy.visit('/details/tv/121361');
        cy.get('#status').contains("Ended")
      });


      it('Streaming Data should display',() => {
        cy.intercept("GET", "**/search/stream/tv/**", { fixture: 'details/tv/streamingResponse'}).as("streamingResponse");

        cy.visit('/details/tv/121361');
        cy.wait('@streamingResponse')

        cy.get('#streamSuper1').should('be.visible');
        cy.get('#streamJamiesNetwork').should('be.visible');
      });


    it.only('Streaming Data should be in correct order',() => {
      cy.intercept("GET", "**/search/stream/tv/**", { fixture: 'details/tv/streamingResponse'}).as("streamingResponse");

      cy.visit('/details/tv/121361');
      cy.wait('@streamingResponse')

      cy.get('.stream-small').then((items) => {
        const results = items.map((index, html) => Cypress.$(html).attr('id')).get();
        console.log(results);
          return results;
      }).should('deep.eq', ['streamSuper1',"streamJamiesNetwork"]);
    });
});