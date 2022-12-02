import { plexSettingsPage as Page } from "@/integration/page-objects";

describe("Plex Settings Tests", () => {
  beforeEach(() => {
    cy.login();
    cy.clearPlexServers();
  });

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
          "localAddresses": "localhost",
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
    modal.hostName.should('have.value','localhost');
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
    modal.hostName.should('have.value','localhost');
    modal.port.should('have.value','32400');
    modal.authToken.should('have.value','myaccessToken');
    modal.machineIdentifier.should('have.value','9999999999999999');

  });

  // Need to finish the witemock container
  it.skip("Load Servers from Plex.TV Api and Test", () => {
    loadServerFromPlexTvApi();
    cy.intercept("POST", "api/v1/tester/plex", (req) => {
      req.reply((res) => {
        res.send(plexTvApiResponse);
      });
    }).as("testResponse");

    const modal = Page.plexServerModal;

    modal.testButton.click();
    cy.wait("@testResponse");

  });

function loadServerFromPlexTvApi() {
  cy.intercept("POST", "api/v1/Plex/servers", (req) => {
    req.reply((res) => {
      res.send(plexTvApiResponse);
    });
  }).as("serverResponse");

  cy.intercept("POST", "api/v1/Settings/Plex").as('plexSave');

  Page.visit();

  Page.plexCredentials.username.type('username');
  Page.plexCredentials.password.type('password');

  Page.plexCredentials.loadServers.click();

  cy.wait("@serverResponse");

  Page.plexCredentials.serverDropdown.click().get('mat-option').contains('AutomationServer').click();
}

});


