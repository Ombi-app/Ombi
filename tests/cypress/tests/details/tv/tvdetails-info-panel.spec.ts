import { tvDetailsPage as Page } from "@/integration/page-objects";

describe('TV Details Information Panel',() => {
    beforeEach(() => {
        cy.login();
    });

    it('Status should be ended',() => {
        Page.visit('1399');
        Page.informationPanel.status.contains("Ended")
      });


      it('Streaming Data should display',() => {
        cy.intercept("GET", "**/search/stream/tv/**", { fixture: 'details/tv/streamingResponse'}).as("streamingResponse");

        Page.visit('1399');
        cy.wait('@streamingResponse')

        Page.informationPanel.getStreaming('Super1').should('be.visible');
        Page.informationPanel.getStreaming('JamiesNetwork').should('be.visible');
      });


    it('Streaming Data should be in correct order',() => {
      cy.intercept("GET", "**/search/stream/tv/**", { fixture: 'details/tv/streamingResponse'}).as("streamingResponse");

      Page.visit('1399');
      cy.wait('@streamingResponse')

      cy.get('.stream-small').then((items) => {
        const results = items.map((index, html) => Cypress.$(html).attr('id')).get();
        console.log(results);
          return results;
      }).should('deep.eq', ['streamSuper1',"streamJamiesNetwork"]);
    });
});