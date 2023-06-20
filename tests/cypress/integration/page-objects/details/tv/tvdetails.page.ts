import { BasePage } from "../../base.page";
import { AdminRequestDialog } from "../../shared/AdminRequestDialog";

class TvRequestPanel {

    seasonTab(seasonNumber: number): Cypress.Chainable<any> {
        return cy.getByData("classStatus"+seasonNumber);
    }

    getSeasonMasterCheckbox(seasonNumber: number): Cypress.Chainable<any> {
        return cy.getByData("masterCheckbox"+seasonNumber);
    }

    getEpisodeSeasonCheckbox(seasonNumber: number, episodeNumber?: number): Cypress.Chainable<any> {
        return cy.getByData("episodeCheckbox"+seasonNumber+episodeNumber);
    }

    getEpisodeCheckbox(seasonNumber: number): Cypress.Chainable<any> {
        return cy.getByDataLike("episodeCheckbox"+seasonNumber);
    }

    getEpisodeStatus(seasonNumber: number, episodeNumber?: number): Cypress.Chainable<any> {
        if (episodeNumber) {
            return cy.getByData('episodeStatus'+seasonNumber+episodeNumber);
        }
        return cy.getByDataLike('episodeStatus'+seasonNumber);
    }
}

class RequestFabButton {

    get requestSelected(): Cypress.Chainable<any> {
        return cy.get('#requestSelected');
    }

    get requestLatest(): Cypress.Chainable<any> {
        return cy.get('#requestLatest');
    }

    get requestFirst(): Cypress.Chainable<any> {
        return cy.get('#requestFirst');
    }

    get fab(): Cypress.Chainable<any> {
        return cy.get('#addFabBtn');
    }
}

class TvDetailsInformationPanel {

    get status(): Cypress.Chainable<any> {
        return cy.get('#status');
    }

    getStreaming(streamName: string): Cypress.Chainable<any> {
        return cy.get(`#stream${streamName}`);
    }
}

class TvDetailsPage extends BasePage {

    get title(): Cypress.Chainable<any> {
        return cy.get('#mediaTitle');
    }

    get availableButton(): Cypress.Chainable<any> {
        return cy.get('#availableBtn');
    }

    get requestButton(): Cypress.Chainable<any> {
        return cy.get('#requestBtn');
    }

    get partiallyAvailableButton(): Cypress.Chainable<any> {
        return cy.get('#partiallyAvailableBtn');
    }

    reportIssueButton(timeout: number): Cypress.Chainable<any> {
        return cy.get('#reportIssueBtn', { timeout: timeout});
    }


    informationPanel = new TvDetailsInformationPanel();
    requestFabButton = new RequestFabButton();
    requestPanel = new TvRequestPanel();
    adminOptionsDialog = new AdminRequestDialog();

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/details/tv/` + id, options);
    }
}

export const tvDetailsPage = new TvDetailsPage();
