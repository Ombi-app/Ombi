import { plexSettingsPage as Page } from "@/integration/page-objects";

describe("Plex Settings Tests", () => {
  beforeEach(() => {
    cy.login();
    cy.clearPlexServers();
  });

  afterEach(() => {
    cy.clearMocks();
  })

  const plexTvApiResponse = `{
    "success": true,
    "message": null,
    "servers": {
      "server": [
        {
          "accessToken": "myaccessToken",
          "name": "AutomationServer",
          "address": "1.1.1.1",
          "port": "32400",
          "version": "1.30.0.6442-5070ad484",
          "scheme": "http",
          "host": "2.2.2.2",
          "localAddresses": "${Cypress.env("dockerhost")}",
          "machineIdentifier": "9999999999999999",
          "createdAt": "5555555555",
          "updatedAt": "6666666666",
          "owned": "1",
          "synced": "0",
          "sourceTitle": null,
          "ownerId": null,
          "home": null
        }
      ],
      "friendlyName": "myPlex",
      "identifier": "com.plexapp.plugins.myplex",
      "machineIdentifier": "3dd86546546546540ff065465460c2654654654654",
      "size": "1"
    }
  }
  `;

  it("Load Servers from Plex.TV Api and Save", () => {
    loadServerFromPlexTvApi();

    const modal = Page.plexServerModal;
    modal.serverName.should('have.value','AutomationServer');
    modal.hostName.should('have.value', Cypress.env("dockerhost"));
    modal.port.should('have.value','32400');
    modal.authToken.should('have.value','myaccessToken');
    modal.machineIdentifier.should('have.value','9999999999999999');

    modal.saveButton.click();

    Page.plexServerGrid.serverCardButton('AutomationServer').should('be.visible');

    Page.submit.click();

    cy.wait("@plexSave");

  });

  it("Load Servers from Plex.TV Api and Edit", () => {
    loadServerFromPlexTvApi();

    const modal = Page.plexServerModal;
    modal.saveButton.click();

    Page.plexServerGrid.serverCardButton('AutomationServer').should('be.visible');

    Page.submit.click();

    cy.wait("@plexSave");

    // Edit server
    Page.plexServerGrid.serverCardButton('AutomationServer').click();
    modal.serverName.should('have.value','AutomationServer');
    modal.hostName.should('have.value', Cypress.env("dockerhost"));
    modal.port.should('have.value','32400');
    modal.authToken.should('have.value','myaccessToken');
    modal.machineIdentifier.should('have.value','9999999999999999');

  });

  it("Load Servers from Plex.TV Api and Test", () => {
    cy.fixture('/mocks/plex/plex-test.mock').then((json) => {
      cy.addMock(json);
    });
    loadServerFromPlexTvApi();
    cy.intercept("POST", "api/v1/tester/plex", (req) => {
      req.reply((res) => {
        res.send(plexTvApiResponse);
      });
    }).as("testResponse");

    const modal = Page.plexServerModal;

    modal.testButton.click();
    cy.wait("@testResponse").then(() => {
      cy.contains("Successfully connected to the Plex server AutomationServer");
    });

  });

  it("Load Libraries from New Server", () => {
    cy.fixture('/mocks/plex/plex-libraries.mock').then((json) => {
      cy.addMock(json);
    });
    cy.intercept("POST", "api/v1/Plex/Libraries").as("libRequest");
    newServer();

    const modal = Page.plexServerModal;
    modal.loadLibraries.click();

    cy.wait("@libRequest");

    modal.getLib(0).click();
    modal.getLib(0).should('contain.text',"lib1");
  });

  it("Remove server", () => {
    loadServerFromPlexTvApi();
    const modal = Page.plexServerModal;
    modal.saveButton.click();

    newServer(false);
    modal.saveButton.click();

    Page.plexServerGrid.serverCardButton('AutomationServer').click();
    modal.deleteButton.click();
    Page.plexServerGrid.serverCardButton('ManualServer').click();
    modal.deleteButton.click();

    Page.plexServerGrid.serverCardButton('AutomationServer').should('not.exist');
    Page.plexServerGrid.serverCardButton('ManualServer').should('not.exist');
  });

  function loadServerFromPlexTvApi(visitPage = true) {
    cy.intercept("POST", "api/v1/Plex/servers", (req) => {
      req.reply((res) => {
        res.send(plexTvApiResponse);
      });
    }).as("serverResponse");

    cy.intercept("POST", "api/v1/Settings/Plex").as('plexSave');

    if (visitPage) {
      Page.visit();
    }

    Page.plexCredentials.username.type('username');
    Page.plexCredentials.password.type('password');

    Page.plexCredentials.loadServers.click();

    cy.wait("@serverResponse");

    Page.plexCredentials.serverDropdown.click().get('mat-option').contains('AutomationServer').click();
  }

  function newServer(visitPage = true) {
    if (visitPage) {
      Page.visit();
    }

    Page.plexServerGrid.newServerButton.click();
    const modal = Page.plexServerModal;

    const server = JSON.parse(plexTvApiResponse);
    modal.serverName.clear();
    modal.serverName.type("ManualServer");
    modal.hostName.type(server.servers.server[0].localAddresses);
    modal.port.type(server.servers.server[0].port);
    modal.authToken.type(server.servers.server[0].accessToken);
    modal.machineIdentifier.type(server.servers.server[0].machineIdentifier);
  }

});


