import { BasePage } from "../../base.page";

class PlexCredentials {
    get username(): Cypress.Chainable<any> {
        return cy.get('#username');
    }

    get password(): Cypress.Chainable<any> {
        return cy.get('#password');
    }

    get loadServers(): Cypress.Chainable<any> {
        return cy.get('#loadServers');
    }

    get serverDropdown(): Cypress.Chainable<any> {
        return cy.get('#servers');
    }
}

class PlexServerModal {
    get serverName(): Cypress.Chainable<any> {
        return cy.get('#serverName');
    }
    get hostName(): Cypress.Chainable<any> {
        return cy.get('#ip');
    }
    get port(): Cypress.Chainable<any> {
        return cy.get('#port');
    }
    get ssl(): Cypress.Chainable<any> {
        return cy.get('#ssl');
    }
    get authToken(): Cypress.Chainable<any> {
        return cy.get('#authToken');
    }
    get machineIdentifier(): Cypress.Chainable<any> {
        return cy.get('#machineId');
    }
    get externalHostname(): Cypress.Chainable<any> {
        return cy.get('#externalHostname');
    }
    get batchSize(): Cypress.Chainable<any> {
        return cy.get('#batchSize');
    }
    get loadLibraries(): Cypress.Chainable<any> {
        return cy.get('#loadLibs');
    }
    get testButton(): Cypress.Chainable<any> {
        return cy.get('#testPlexButton');
    }
    get deleteButton(): Cypress.Chainable<any> {
        return cy.get('#deleteServer');
    }
    get cancelButton(): Cypress.Chainable<any> {
        return cy.get('#cancel');
    }
    get saveButton(): Cypress.Chainable<any> {
        return cy.get('#saveServer');
    }

    getLib(index: number): Cypress.Chainable<any> {
        return cy.get(`#lib-${index}`);
    }
}

class PlexServersGrid {
    serverCardButton(name: string): Cypress.Chainable<any> {
        return cy.get(`#${name}-button`);
    }

    get newServerButton(): Cypress.Chainable<any> {
        return cy.get('#newServer');
    }
}

class PlexSettingsPage extends BasePage {


    get enableCheckbox(): Cypress.Chainable<any> {
        return cy.get('#enable');
    }

    get enableWatchlist(): Cypress.Chainable<any> {
        return cy.get('#enableWatchlistImport');
    }

    get submit(): Cypress.Chainable<any> {
        return cy.get('#save');
    }

    get fullySync(): Cypress.Chainable<any> {
        return cy.get('#fullSync');
    }

    get partialSync(): Cypress.Chainable<any> {
        return cy.get('#recentlyAddedSync');
    }

    get clearAndResync(): Cypress.Chainable<any> {
        return cy.get('#clearData');
    }

    get runWatchlist(): Cypress.Chainable<any> {
        return cy.get('#watchlistImport');
    }

    plexCredentials = new PlexCredentials();
    plexServerModal = new PlexServerModal();
    plexServerGrid = new PlexServersGrid();

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/Settings/Plex`, options);
    }

}

export const plexSettingsPage = new PlexSettingsPage();
